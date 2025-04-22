using NAudio.Wave;
using NVorbis;
using static CMDG.Util;

// The demo "A Quick Hello" for Revision 2025.
// Reads screen content into characters and uses them to show .png images.

// Requires Config.ReadConsoleFirst = true and ScreenHeight = 61
// (Set manually.)

namespace CMDG
{
    internal static class QuickHello
    {
        private static bool exitScene = false;
        private static Random random = new Random();
        private static int framesSinceLastAssignment = 0;
        private static int currentCharacterIndex = 0;
        private static int currentTargetIndex = 0;
        private const int FRAMES_BETWEEN_ASSIGNMENTS = 0;
        private static float pauseTimer = 0f;
        private const float PAUSE_DURATION = 3f; // Pause between images
        private const float EXTRA_PAUSE_DURATION = 1.5f; // Additional 1 second for specific images
        private static int currentPngIndex = 0; // Number of PNG currently being drawn

        // Audio players
        private static WaveOutEvent? musicPlayer;
        private static WaveOutEvent? reversePlayer;
        private static WaveOutEvent? squeakPlayer;
        private static IWaveProvider? musicStream;
        private static IWaveProvider? reverseStream;
        private static IWaveProvider? squeakStream;

        // The reverse and squeak sound at the end
        private static bool hasStartedReverse = false;
        private static float reverseTimer = 0f;
        private const float REVERSE_DURATION = 8.0f; // Duration to play reverse.wav before switching to squeak.wav
        private static bool hasPlayedSqueak = false;

        public static void Run()
        {
            // Initialize audio players
            try
            {
                var musicVorbisStream = new FileStream("Scenes/A Quick Hello/Assets/music.ogg", FileMode.Open);
                var musicVorbisReader = new VorbisReader(musicVorbisStream, false);
                musicStream = new VorbisWaveProvider(musicVorbisReader);
                musicPlayer = new WaveOutEvent();
                musicPlayer.Init(musicStream);
                musicPlayer.Play();

                var reverseVorbisStream = new FileStream("Scenes/A Quick Hello/Assets/reverse.ogg", FileMode.Open);
                var reverseVorbisReader = new VorbisReader(reverseVorbisStream, false);
                reverseStream = new VorbisWaveProvider(reverseVorbisReader);
                reversePlayer = new WaveOutEvent();
                reversePlayer.Init(reverseStream);
                
                var squeakVorbisStream = new FileStream("Scenes/A Quick Hello/Assets/squeak.ogg", FileMode.Open);
                var squeakVorbisReader = new VorbisReader(squeakVorbisStream, false);
                squeakStream = new VorbisWaveProvider(squeakVorbisReader);
                squeakPlayer = new WaveOutEvent();
                squeakPlayer.Init(squeakStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing audio: {ex.Message}");
            }

            // Get the characters that were read from the screen at startup
            var originalScreenCharacters = Util.ReadCharacters;

            // By default the screen is read one row at the time left to right. Sort the list so that it's instead one column at a time, top top bottom
            var screenCharacters = originalScreenCharacters
                .OrderBy(c => c.x)
                .ThenBy(c => c.y)
                .ToList();

            // Store a list of original character positions so the initial view can be restored
            var originalPositions = new List<ReadCharacter>(screenCharacters);

            // Check if we have enough characters to play the demo with
            if (screenCharacters.Count < 1000)
            {
                Console.WriteLine("This will look better if there are more characters on screen. Pls execute whatever command (e.g. dir or whatever you like) and try again. ':)");
                exitScene = true;
                throw new Exception("Not enough characters on screen"); // Stop the scene thread
            }

            // Read PNG files for target coordinates
            HelloPngReader.ReadPngFiles();
            var targetCoordinates = HelloPngReader.GetTargetCoordinates();

            // Add original positions as final target coordinates, in the original order
            for (int i = 0; i < screenCharacters.Count; i++)
            {
                targetCoordinates.Add(new TargetCoordinate(originalScreenCharacters[i].x, originalScreenCharacters[i].y));
            }

            // Create a list of moving characters based on the originals.
            var movingCharacters = new List<HelloCharacter>();
            for (int i = 0; i < screenCharacters.Count; i++)
            {
                movingCharacters.Add(new HelloCharacter(screenCharacters[i].character, screenCharacters[i].x, screenCharacters[i].y));
            }


            // Scene loop
            while (true && !exitScene)
            {
                SceneControl.StartFrame();

                // Update and draw each character
                for (int i = 0; i < movingCharacters.Count; i++)
                {
                    var character = movingCharacters[i];
                    // Update position with easing
                    character.UpdatePosition(SceneControl.DeltaTime);

                    // If this is the final phase (after all target coordinates are assigned)
                    if (currentTargetIndex >= targetCoordinates.Count && character.TargetX == character.OriginalX && character.TargetY == character.OriginalY)
                    {
                        // Update the character to match its original character
                        character.Character = originalPositions[i].character;
                    }

                    // Round coordinates to integers for drawing
                    int drawX = (int)Math.Round(character.X);
                    int drawY = (int)Math.Round(character.Y);

                    // Only draw if the position is within screen bounds
                    if (drawX >= 0 && drawX < Config.ScreenWidth && 
                        drawY >= 0 && drawY < Config.ScreenHeight)
                    {
                        Framebuffer.SetPixel(drawX, drawY, new Color32(200, 200, 200), character.Character);
                    }
                }

                // If in pause, update the timer
                if (pauseTimer > 0)
                {
                    pauseTimer -= (float)SceneControl.DeltaTime;
                }

                // If not in pause, assign new target coordinates to the moving characters

                // Originally intended to assign one new target per n frames, but turned out we need to assign multiple targets each frame.
                // -> FRAMES_BETWEEN_ASSIGNMENTS and framesSinceLastAssignment are currently 0 and irrelevant. But easy to restore if needed later.
                else if (framesSinceLastAssignment >= FRAMES_BETWEEN_ASSIGNMENTS)
                {
                    int targetsToAssign;
                    if (FRAMES_BETWEEN_ASSIGNMENTS == 0)
                    {
                        // Assign more targets per frame in the final "rewinding" phase.
                        bool isFinalPhase = currentTargetIndex >= targetCoordinates.Count - screenCharacters.Count;
                        if (isFinalPhase) { targetsToAssign = 10; } else { targetsToAssign = 5; }

                        // Switch to reverse.wav when entering final phase
                        if (isFinalPhase && !hasStartedReverse && musicPlayer != null && reversePlayer != null)
                        {
                            try
                            {
                                musicPlayer.Stop();
                                reversePlayer.Play();
                                hasStartedReverse = true;
                                reverseTimer = 0f; // Reset the timer when starting reverse.wav
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    else
                    {
                        targetsToAssign = 1;
                    }
                    
                    for (int i = 0; i < targetsToAssign; i++)
                    {
                        // Only assign new targets if we haven't used all of them yet
                        if (currentTargetIndex < targetCoordinates.Count)
                        {
                            // Target coordinates of -1 -1 are markers that signal a change between files
                            // In this case we assign remaining characters to the bottom "stash" of the screen and
                            // start from the first moving character again.
                            if (targetCoordinates[currentTargetIndex].X == -1 && targetCoordinates[currentTargetIndex].Y == -1)
                            {
                                // Move to next target (skip the marker)
                                currentTargetIndex++;
                                currentPngIndex++; // Increment PNG index
                                
                                if (currentTargetIndex < targetCoordinates.Count)
                                {
                                    while (currentCharacterIndex < screenCharacters.Count)
                                    {
                                        var remainingChar = movingCharacters[currentCharacterIndex];
                                        remainingChar.StartingX = remainingChar.X;
                                        remainingChar.StartingY = remainingChar.Y;
                                        
                                        // Don't move the character if it was already in the bottom stash.
                                        // If it wasn't in the stash, assign a random position in it.
                                        if (remainingChar.X < 2 || remainingChar.X >= 198 || 
                                            remainingChar.Y < 53 || remainingChar.Y >= 60)
                                        {
                                            remainingChar.TargetX = random.Next(2, 198);
                                            remainingChar.TargetY = random.Next(53, 60);
                                            remainingChar.ResetProgress();
                                        }                                       
                                        currentCharacterIndex++;
                                    }
                                    
                                    // Reset character index for the next PNG file
                                    currentCharacterIndex = 0;
                                    
                                    // Start the pause timer with additional time for specific images
                                    pauseTimer = PAUSE_DURATION;
                                    if (currentPngIndex == 5 || currentPngIndex == 6)
                                    {
                                        pauseTimer += EXTRA_PAUSE_DURATION;
                                    }
                                    break;
                                }
                            }

                            var character = movingCharacters[currentCharacterIndex];
                            
                            // Update starting position to current position before setting new target
                            character.StartingX = character.X;
                            character.StartingY = character.Y;
                            character.TargetX = targetCoordinates[currentTargetIndex].X;
                            character.TargetY = targetCoordinates[currentTargetIndex].Y;
                            character.ResetProgress();

                            // Move to next character and target
                            currentCharacterIndex = (currentCharacterIndex + 1) % screenCharacters.Count;
                            currentTargetIndex++;
                        }
                    }
                    
                    framesSinceLastAssignment = 0;
                }
                else
                {
                    framesSinceLastAssignment++;
                }

                // Update reverse timer and check if it's time to switch to squeak.wav
                if (hasStartedReverse && !hasPlayedSqueak)
                {
                    reverseTimer += (float)SceneControl.DeltaTime;
                    if (reverseTimer >= REVERSE_DURATION && reversePlayer != null && squeakPlayer != null)
                    {
                        try
                        {
                            reversePlayer.Stop();
                            squeakPlayer.Play();
                            hasPlayedSqueak = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error playing final sound: {ex.Message}");
                        }
                    }
                }

                SceneControl.EndFrame();
            }
        }

        public static void Exit()
        {
            // Clean up audio players
            try
            {
                musicPlayer?.Stop();
                musicPlayer?.Dispose();
                reversePlayer?.Stop();
                reversePlayer?.Dispose();
                squeakPlayer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up audio: {ex.Message}");
            }
        }

        public static bool CheckForExit()
        {
            return exitScene;
        }
    }
} 