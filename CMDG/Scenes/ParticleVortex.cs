using System;
using System.IO;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace CMDG
{
    /// Turbo Knight music video. Three moving vortexes attract particles that leave trails and shift hue over time.
    internal class ParticleVortex
    {
        static readonly double TrailHalfLife = 0.65;  // Seconds until trail intensity halves (exponential decay).
        static readonly double FlowSpeed = 25.0;      // Scale of vortex force to particle speed ("pixels" per second).
        static readonly double HueDrift = 0.020;       // Hue cycle speed (1/seconds).
        static readonly double HueBaseOffset = 0.3;   // Global hue offset so we don't start at red/yellow.
        static readonly double ParticleFadeInDuration = 10.0;  // Seconds from start, over which particles are gradually allowed to draw (0 → all).
        static readonly double ParticleCountDivisor = 8.0;   // Total screen space (W*H) divided by this = number of particles. Larger = fewer.
        static readonly double DemoLength = 130.0;             // Main demo duration (seconds). After this, vortexes fade out, gravity ramps in, and Y-wrap is disabled.
        static readonly double EndTransitionDuration = 20.0;  // Seconds over which vortex strength goes 1→0
        static readonly double EndGravity = 30.0;           // Gravity ramp rate (pixels/s² per second) after DemoLength; increases constantly with no cap.
        static readonly double AutoExitDelayAfterDemo = 15.0;  // Seconds after DemoLength when the demo automatically ends (same as user pressing ESC).

        static int Screen_Width, Screen_Height;       // Screen width and height (character cells).
        static double InvW, InvH;                     // 1/W and 1/H, used to convert pixel coords to normalized [0,1] for vortex math.


        // Particles that leave trails, wrap around the screen and are attracted by vortexes
        class Particle
        {
            public double x, y, vx, vy;               // Position and velocity for each particle.
            public double hueOffset;                  // Hue offset in [0, 0.1] for staggered cycling.
        }
        static Particle[] particles;
        static int particleCount;
        static double[] trail;                        // Per-pixel trail intensity in [0,1]; decayed each frame and increased where particles pass.
        static double[] trailHueOffset;               // Per-pixel hue offset [0, 0.1], blended from particles that contributed to the trail.


        // Background stars - one spawn happens with each bassdrum. 
        static readonly int StarsPerSpawn = 250;
        static readonly double StarSpawnIntervalSeconds = 0.5;
        static readonly double StarDecayIntervalSeconds = 0.1;
        static readonly int StarDecayAmount = 40;
        static readonly double StarDurationSeconds = StarDecayIntervalSeconds * (255.0 / (StarDecayAmount * 3));  // Time until a spawned star fades to 0.
        class Star
        {
            public int x, y;                          // Top-left of 2x2 block; must be in [0, W-2] x [0, H-2].
            public int brightness;                    // 255 at spawn; decays until <= 0 then star is removed. Used for r,g,b when drawing.
        }
        static System.Collections.Generic.List<Star> stars = new System.Collections.Generic.List<Star>();
        static double lastStarSpawnTime;
        static double lastStarDecayTime;
        static int starSpawnTriggerCount;  // 1-based; certain trigger ranges skipped for music sync (see UpdateStars).


        // Vertical beams: spawn rate can vary via GetBeamSpawnInterval(t).
        static readonly double BeamCenterDuration = 2.0;   // Seconds until center pixel is black, then beam expires.
        static readonly double BeamDecayRate = 0.5;       // Intensity loss per second (center 1→0 in 2s).
        static readonly double BeamMidStart = 0.8;         // Mid pixels start at this intensity (expire sooner).
        static readonly double BeamEdgeStart = 0.2;        // Edge pixels start here (expire soonest).
        static readonly int BeamWidth = 5;                 // Pixels wide (center ± 2).
        // Use ANSI palette blues so 16-color output stays blue (bright → dark), not purple: index 12 = bright, 4 = dark.
        static readonly Color32 BeamBrightBlue = new Color32(59, 120, 255);
        static readonly Color32 BeamDarkBlue = new Color32(0, 55, 218);
        static readonly double BeamBrightToDarkThreshold = 0.5;  // Intensity above this → bright blue; below → dark blue.
        class Beam
        {
            public int centerX;   // Center column; beam spans [centerX-2, centerX+2].
            public double spawnTime;
        }
        static System.Collections.Generic.List<Beam> beams = new System.Collections.Generic.List<Beam>();
        static double[]? beamSpawnTimes;   // Loaded from Particle_vortex_beam_times.txt (seconds from start).
        static int nextBeamTimeIndex;

        // Big beams: wider, red, spawned at set beat times during the breakdown.
        static readonly int BigBeamWidth = 15;             // Pixels wide (center ± 7).
        static readonly Color32 BigBeamBrightRed = new Color32(255, 90, 90);
        static readonly Color32 BigBeamDarkRed = new Color32(218, 0, 0);
        static readonly int[] BigBeamBeatTimes = { 161, 166, 168, 177, 182, 184 };   // Beats to spawn at
        static System.Collections.Generic.HashSet<int> bigBeamBeatSet = new System.Collections.Generic.HashSet<int>(BigBeamBeatTimes);
        class BigBeam
        {
            public int centerX;
            public double spawnTime;
        }
        static System.Collections.Generic.List<BigBeam> bigBeams = new System.Collections.Generic.List<BigBeam>();

        static Random rng = new Random(1337);

        // Audio playback
        static WaveOutEvent? audioOut;
        static AudioFileReader? audioFile;
        static bool musicStarted = false;

        // Credits text data
        static string[]? credits1;
        static string[]? credits2;
        static readonly double Credits1StartTime = 60.0;  // Seconds when first credits start
        static readonly double Credits1EndTime = 69.0;    // Seconds when first credits end
        static readonly double Credits2StartTime = 70.0;  // Seconds when second credits start
        static readonly double Credits2EndTime = 79.0;    // Seconds when second credits end
        static readonly int CreditsPadding = 3;           // Rows of black pixels on each side
        static readonly Color32 CreditsGreenColor = new Color32(0, 255, 0);  // Bright green
        static readonly Color32 CreditsBlackColor = new Color32(0, 0, 0);    // Black
        static readonly Color32 CreditsOutlineGrey = new Color32(128, 128, 128);  // Grey outline

        public static void Run()
        {
            InitializeFrame();
            while (true)
            {
                SceneControl.StartFrame();

                // Start music exactly when the first frame begins (ElapsedTime is 0 before first EndFrame)
                if (!musicStarted && SceneControl.ElapsedTime <= 0.001)
                {
                    StartMusic();
                    musicStarted = true;
                }

                double t = SceneControl.ElapsedTime;
                double dt = Math.Max(0.0001, SceneControl.DeltaTime);
                double baseHue = Mod(HueBaseOffset + HueDrift * t, 1.0);

                // Ramp up trail half-life along with particle count, over the specified fade-in duration.
                double halfLife;
                if (t >= ParticleFadeInDuration)
                {
                    halfLife = TrailHalfLife;
                }
                else
                {
                    halfLife = 0.1 + (TrailHalfLife - 0.1) * (t / ParticleFadeInDuration);
                }

                UpdateParticles(t, dt);
                ApplyTrailDecay(dt, halfLife);
                UpdateStars(t);
                UpdateBeams(t);
                UpdateBigBeams(t);
                DrawFrame(baseHue);

                SceneControl.EndFrame();
            }
        }

        // When true, the main thread ends the scene (same as ESC). Used to auto-exit after DemoLength + AutoExitDelayAfterDemo.
        public static bool CheckForExit() => SceneControl.ElapsedTime >= DemoLength + AutoExitDelayAfterDemo;

        public static void Exit() 
        { 
            StopMusic();
        }

        // Initialize the scene once, outside of the loop
        static void InitializeFrame()
        {
            if (trail != null && particles != null)
                return;

            Screen_Width = Config.ScreenWidth;
            Screen_Height = Config.ScreenHeight;
            InvW = 1.0 / Math.Max(1, Screen_Width);
            InvH = 1.0 / Math.Max(1, Screen_Height);

            trail = new double[Screen_Width * Screen_Height];
            trailHueOffset = new double[Screen_Width * Screen_Height];

            particleCount = (int)(Screen_Width * Screen_Height / ParticleCountDivisor);
            particles = new Particle[particleCount];

            // Initialize particles at random positions with zero velocity and random hue offset for staggered cycling
            for (int i = 0; i < particleCount; i++)
            {
                particles[i] = new Particle
                {
                    x = rng.NextDouble() * (Screen_Width - 1),
                    y = rng.NextDouble() * (Screen_Height - 1),
                    vx = 0,
                    vy = 0,
                    hueOffset = rng.NextDouble() * 0.1
                };
            }

            // Stars spawn immediately at t=0 (first trigger), then every StarSpawnIntervalSeconds
            lastStarSpawnTime = -StarSpawnIntervalSeconds;
            nextBeamTimeIndex = 0;
            LoadBeamTimes();

            // Initialize audio - load and prepare the music file
            InitializeAudio();

            // Load credits files
            LoadCredits();
        }

        // Initialize audio system and load the music file
        static void InitializeAudio()
        {
            try
            {
                // Initialize MediaFoundation (required for MP3 support on Windows)
                MediaFoundationApi.Startup();

                string musicPath = Path.Combine("Scenes", "Particle Vortex", "Particle_vortex_music.mp3");
                if (!File.Exists(musicPath))
                {
                    // Alternative path
                    musicPath = "Particle_vortex_music.mp3";
                }

                if (File.Exists(musicPath))
                {
                    audioFile = new AudioFileReader(musicPath);
                    audioOut = new WaveOutEvent();
                    audioOut.Init(audioFile);
                    // Don't start playback here - wait for first frame
                }
            }
            catch (Exception ex)
            {
                // If audio can't be loaded, continue demo silently
                System.Diagnostics.Debug.WriteLine($"Failed to load audio: {ex.Message}");
            }
        }

        // Start music playback (called at the start of the first frame)
        static void StartMusic()
        {
            try
            {
                if (audioOut != null && audioFile != null)
                {
                    audioOut.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start audio: {ex.Message}");
            }
        }

        // Stop and cleanup audio
        static void StopMusic()
        {
            try
            {
                if (audioOut != null)
                {
                    audioOut.Stop();
                    audioOut.Dispose();
                    audioOut = null;
                }
                if (audioFile != null)
                {
                    audioFile.Dispose();
                    audioFile = null;
                }
                MediaFoundationApi.Shutdown();
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }

        // Load beam spawn times from text file (one time in seconds per line).
        static void LoadBeamTimes()
        {
            try
            {
                string path = Path.Combine("Scenes", "Particle Vortex", "Particle_vortex_beam_times.txt");
                if (!File.Exists(path))
                    path = "Particle_vortex_beam_times.txt";
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);
                    var times = new System.Collections.Generic.List<double>();
                    foreach (string line in lines)
                    {
                        string s = line.Trim();
                        if (string.IsNullOrEmpty(s)) continue;
                        if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double time))
                            times.Add(time);
                    }
                    beamSpawnTimes = times.ToArray();
                }
                else
                {
                    beamSpawnTimes = Array.Empty<double>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load beam times: {ex.Message}");
                beamSpawnTimes = Array.Empty<double>();
            }
        }

        // Load credits text files
        static void LoadCredits()
        {
            try
            {
                string credits1Path = Path.Combine("Scenes", "Particle Vortex", "Particle_vortex_credits1.txt");
                string credits2Path = Path.Combine("Scenes", "Particle Vortex", "Particle_vortex_credits2.txt");

                if (File.Exists(credits1Path))  
                {
                    credits1 = File.ReadAllLines(credits1Path);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Credits file not found: {credits1Path}");
                }

                if (File.Exists(credits2Path))
                {
                    credits2 = File.ReadAllLines(credits2Path);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Credits file not found: {credits2Path}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load credits: {ex.Message}");
            }
        }

        // Moves each particle with vortex field (and gravity after DemoLength), wraps X always and Y only when t <= DemoLength.
        static void UpdateParticles(double t, double dt)
        {
            const double inertia = 0.85;  // Blend of previous velocity and new force
            double speed = FlowSpeed;

            int drawCount;
            if (t >= ParticleFadeInDuration)
            {
                drawCount = particleCount;
            }
            else
            {
                drawCount = (int)((t / ParticleFadeInDuration) * particleCount);
            }

            // After DemoLength: vortex strength 1→0 over EndTransitionDuration; gravity increases constantly with no cap.
            double vortexScale = 1.0;
            double gravity = 0.0;
            if (t > DemoLength)
            {
                double elapsed = t - DemoLength;
                if (elapsed >= EndTransitionDuration)
                    vortexScale = 0.0;
                else
                    vortexScale = 1.0 - elapsed / EndTransitionDuration;
                gravity = elapsed * EndGravity;  // pixels/s², grows linearly with time
            }

            bool wrapY = t <= DemoLength;

            // Three vortex centers in normalized [0,1], orbiting slowly over time.
            Vec2 c1 = new Vec2(0.5 + 0.25 * Math.Sin(t * 0.4), 0.5 + 0.16 * Math.Cos(t * 0.33));
            Vec2 c2 = new Vec2(0.5 + 0.30 * Math.Sin(t * 0.23 + 2.1), 0.5 + 0.22 * Math.Cos(t * 0.37 + 1.3));
            Vec2 c3 = new Vec2(0.5 + 0.22 * Math.Sin(t * 0.31 + 4.7), 0.5 + 0.18 * Math.Cos(t * 0.29 + 3.2));

            for (int i = 0; i < particleCount; i++)
            {
                Particle p = particles[i];
                double u = p.x * InvW;  // Normalized x in [0,1].
                double v = p.y * InvH;

                Vec2 f = Vortex(u, v, c1, 0.9) + Vortex(u, v, c2, -0.8) + Vortex(u, v, c3, 0.7);

                double vx = p.vx * inertia + f.x * speed * vortexScale * (1.0 - inertia);
                double vy = p.vy * inertia + f.y * speed * vortexScale * (1.0 - inertia);
                vy += gravity * dt;
                p.vx = vx;
                p.vy = vy;

                double nx = p.x + vx * dt;
                double ny = p.y + vy * dt;
                if (nx < 0) nx += Screen_Width; else if (nx >= Screen_Width) nx -= Screen_Width;
                if (wrapY)
                { if (ny < 0) ny += Screen_Height; else if (ny >= Screen_Height) ny -= Screen_Height; }

                if (i < drawCount)
                {
                    // If wrapped, don't interpolate (would draw a line across the screen).
                    bool wrappedX = Math.Abs(nx - p.x) > Screen_Width / 2.0;
                    bool wrappedY = wrapY && Math.Abs(ny - p.y) > Screen_Height / 2.0;
                    if (wrappedX || wrappedY)
                    {
                        AddToTrail((int)nx, (int)ny, 1.0, p.hueOffset);
                    }
                    else
                    {
                        double dx = nx - p.x, dy = ny - p.y;
                        int steps = (int)Math.Max(1, Math.Ceiling(Math.Max(Math.Abs(dx), Math.Abs(dy))));
                        for (int s = 0; s <= steps; s++)
                        {
                            double frac = (double)s / steps;
                            AddToTrail((int)(p.x + frac * dx), (int)(p.y + frac * dy), 1.0, p.hueOffset);
                        }
                    }
                }
                p.x = nx;
                p.y = ny;
            }
        }

        // Spawn stars once per interval; every decay interval subtract StarDecayAmount from brightness and remove when <= 0.
        // When stars spawn, DefaultCharacter becomes '.'; when that batch's duration is over, it becomes 'X' again.
        static void UpdateStars(double t)
        {
            while (lastStarSpawnTime + StarSpawnIntervalSeconds <= t)
            {
                lastStarSpawnTime += StarSpawnIntervalSeconds;
                starSpawnTriggerCount++;
                // Stars spawn based on BPM, but the following beats without bassdrum are skipped.
                bool skipForMusicSync =
                    (starSpawnTriggerCount >= 1 && starSpawnTriggerCount <= 16) ||    // beats 0–16
                    (starSpawnTriggerCount >= 30 && starSpawnTriggerCount <= 32) ||   // beats 29–32
                    (starSpawnTriggerCount >= 161 && starSpawnTriggerCount <= 196) || // beats 180–196
                    (t >= DemoLength);
                if (bigBeamBeatSet.Contains(starSpawnTriggerCount))
                {
                    int minBigX = 8;
                    int maxBigX = Math.Max(minBigX, Screen_Width - 8);
                    int centerX = rng.Next(minBigX, maxBigX + 1);
                    bigBeams.Add(new BigBeam { centerX = centerX, spawnTime = t });
                }
                if (!skipForMusicSync)
                {
                    Config.DefaultCharacter = '#';
                    int maxX = Math.Max(0, Screen_Width - 2);
                    int maxY = Math.Max(0, Screen_Height - 2);
                    for (int i = 0; i < StarsPerSpawn; i++)
                    {
                        int x = maxX > 0 ? rng.Next(0, maxX + 1) : 0;
                        int y = maxY > 0 ? rng.Next(0, maxY + 2) : 0;
                        stars.Add(new Star { x = x, y = y, brightness = 255 });
                    }
                }
            }
            if (t >= lastStarSpawnTime + StarDurationSeconds)
                Config.DefaultCharacter = 'X';

            while (lastStarDecayTime + StarDecayIntervalSeconds <= t)
            {
                lastStarDecayTime += StarDecayIntervalSeconds;
                for (int i = stars.Count - 1; i >= 0; i--)
                {
                    Star s = stars[i];
                    s.brightness -= StarDecayAmount;
                    if (s.brightness <= 0)
                        stars.RemoveAt(i);
                }
            }
        }

        static void UpdateBeams(double t)
        {
            if (beamSpawnTimes != null)
            {
                while (nextBeamTimeIndex < beamSpawnTimes.Length && t >= beamSpawnTimes[nextBeamTimeIndex])
                {
                    nextBeamTimeIndex++;
                    int minX = 2;
                    int maxX = Math.Max(minX, Screen_Width - 2);
                    int centerX = rng.Next(minX, maxX + 1);
                    beams.Add(new Beam { centerX = centerX, spawnTime = t });
                }
            }
            for (int i = beams.Count - 1; i >= 0; i--)
            {
                double age = t - beams[i].spawnTime;
                if (age >= BeamCenterDuration)
                    beams.RemoveAt(i);
            }
        }

        static void UpdateBigBeams(double t)
        {
            for (int i = bigBeams.Count - 1; i >= 0; i--)
            {
                double age = t - bigBeams[i].spawnTime;
                if (age >= BeamCenterDuration)
                    bigBeams.RemoveAt(i);
            }
        }

        // Exponential decay of trail buffer; halfLife ramps from 0.1 to TrailHalfLife over ParticleFadeInDuration.
        static void ApplyTrailDecay(double dt, double halfLife)
        {
            double decay = Math.Pow(0.5, dt / Math.Max(0.0001, halfLife));
            for (int i = 0; i < trail.Length; i++)
                trail[i] = Math.Max(0.0, trail[i] * decay);
        }

        // Trail buffer rendering order (bottom first): stars - beams - trails/particles - credits
        static void DrawFrame(double baseHue)
        {
            // 1. Draw all stars.
            foreach (Star s in stars)
            {
                byte bright = (byte)Clamp255(s.brightness);
                var c = new Color32(bright, bright, bright);
                Framebuffer.SetPixel(s.x, s.y, c);
            }

            // 2. Draw beams (after stars, before particles/trail).
            double t = SceneControl.ElapsedTime;
            foreach (Beam b in beams)
            {
                double age = t - b.spawnTime;
                // Center: bright blue, linear decay to black in BeamCenterDuration.
                double centerI = Math.Max(0, 1.0 - age / BeamCenterDuration);
                double midI = Math.Max(0, BeamMidStart - BeamDecayRate * age);
                double edgeI = Math.Max(0, BeamEdgeStart - BeamDecayRate * age);
                int cx = b.centerX;
                for (int y = 0; y < Screen_Height; y++)
                {
                    if (centerI > 0)
                    {
                        var c = centerI > BeamBrightToDarkThreshold ? BeamBrightBlue : BeamDarkBlue;
                        Framebuffer.SetPixel(cx, y, c);
                    }
                    if (midI > 0)
                    {
                        var c = midI > BeamBrightToDarkThreshold ? BeamBrightBlue : BeamDarkBlue;
                        if (cx - 1 >= 0) Framebuffer.SetPixel(cx - 1, y, c);
                        if (cx + 1 < Screen_Width) Framebuffer.SetPixel(cx + 1, y, c);
                    }
                    if (edgeI > 0)
                    {
                        var c = edgeI > BeamBrightToDarkThreshold ? BeamBrightBlue : BeamDarkBlue;
                        if (cx - 2 >= 0) Framebuffer.SetPixel(cx - 2, y, c);
                        if (cx + 2 < Screen_Width) Framebuffer.SetPixel(cx + 2, y, c);
                    }
                }
            }

            // 2b. Draw big beams (red, width 15).
            foreach (BigBeam b in bigBeams)
            {
                double age = t - b.spawnTime;
                double centerI = Math.Max(0, 1.0 - age / BeamCenterDuration);
                double midI = Math.Max(0, BeamMidStart - BeamDecayRate * age);
                double edgeI = Math.Max(0, BeamEdgeStart - BeamDecayRate * age);
                int cx = b.centerX;
                for (int y = 0; y < Screen_Height; y++)
                {
                    if (centerI > 0)
                    {
                        var c = centerI > BeamBrightToDarkThreshold ? BigBeamBrightRed : BigBeamDarkRed;
                        Framebuffer.SetPixel(cx, y, c);
                    }
                    for (int d = 1; d <= 7; d++)
                    {
                        double intensity = (d <= 2) ? midI : edgeI;
                        if (intensity > 0)
                        {
                            var col = intensity > BeamBrightToDarkThreshold ? BigBeamBrightRed : BigBeamDarkRed;
                            if (cx - d >= 0) Framebuffer.SetPixel(cx - d, y, col);
                            if (cx + d < Screen_Width) Framebuffer.SetPixel(cx + d, y, col);
                        }
                    }
                }
            }

            // 3. Draw trails over (overwrite only where trail has content and doesn't map to black in 16-color ANSI).
            const int AnsiBlackIndex = 0;  // ANSI color index 0 is black; don't draw trail there so stars/background show.
            int idx = 0;
            for (int y = 0; y < Screen_Height; y++)
            {
                for (int x = 0; x < Screen_Width; x++, idx++)
                {
                    double brightness = Clamp01(trail[idx]);
                    if (brightness <= 0.0001) continue;
                    double h = Mod(baseHue + trailHueOffset[idx], 1.0);
                    RGB rgb = HSV(h, 0.85, brightness);
                    var trailColor = new Color32((byte)Clamp255(rgb.r), (byte)Clamp255(rgb.g), (byte)Clamp255(rgb.b));
                    if (ColorConverter.GetClosestAnsiColorIndexFromMap(trailColor) == AnsiBlackIndex)
                        continue;  // Would display as black; leave star/background visible.
                    char ch = (char)rng.Next(0x20, 0x7F);
                    Framebuffer.SetPixel(x, y, trailColor, ch);
                }
            }

            // 4. Draw credits last (on top of everything)
            DrawCredits(t);
        }

        // Velocity field of a single vortex at (u,v) - center and spin define the vortex.
        static Vec2 Vortex(double u, double v, Vec2 center, double spin)
        {
            Vec2 d = new Vec2(u - center.x, v - center.y);
            double r2 = d.x * d.x * 1.6 + d.y * d.y;
            double m = spin / (0.018 + r2);
            return Rot90(d) * m;
        }

        // Adds amount to trail at (x,y), clamped to 1.0. Hue offset is blended by contribution.
        static void AddToTrail(int x, int y, double amount, double hueOffset)
        {
            if ((uint)x >= (uint)Screen_Width || (uint)y >= (uint)Screen_Height) return;
            int i = y * Screen_Width + x;
            double oldVal = trail[i];
            double actualAdded = (oldVal + amount > 1.0) ? (1.0 - oldVal) : amount;
            double newVal = oldVal + actualAdded;
            if (newVal > 0)
                trailHueOffset[i] = (oldVal * trailHueOffset[i] + actualAdded * hueOffset) / newVal;
            trail[i] = newVal;
        }

        // 2D double vector used for positions and forces.
        struct Vec2
        {
            public double x, y;
            public Vec2(double x, double y) { this.x = x; this.y = y; }
            public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
            public static Vec2 operator *(Vec2 a, double k) => new Vec2(a.x * k, a.y * k);
        }

        // Rotates vector 90° counter-clockwise for tangential vortex velocity
        static Vec2 Rot90(Vec2 v) => new Vec2(-v.y, v.x);

        // Modulo that always returns a value in [0, m) even when x is negative.
        static double Mod(double x, double m)
        {
            double r = x % m;
            return r < 0 ? r + m : r;
        }

        static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        static int Clamp255(int x) => x < 0 ? 0 : (x > 255 ? 255 : x);

        //RGB color with components 0–255.
        struct RGB
        {
            public int r, g, b;
            public RGB(int r, int g, int b) { this.r = r; this.g = g; this.b = b; }
        }

        //Converts HSV 0-1 color to RGB 0–255.
        static RGB HSV(double h, double s, double v)
        {
            h = Mod(h, 1.0);
            s = Clamp01(s);
            v = Clamp01(v);
            if (s <= 0.000001)
            {
                int vi = (int)(v * 255.0 + 0.5);
                return new RGB(vi, vi, vi);
            }

            double hf = h * 6.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double p = v * (1.0 - s);
            double q = v * (1.0 - s * f);
            double t = v * (1.0 - s * (1.0 - f));

            double r = 0, g = 0, b = 0;
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
            return new RGB((int)(r * 255.0 + 0.5), (int)(g * 255.0 + 0.5), (int)(b * 255.0 + 0.5));
        }

        // Draw credits text at the top left corner
        static void DrawCredits(double t)
        {
            string[]? creditsToDraw = null;

            // Determine which credits to show based on time
            if (t >= Credits1StartTime && t < Credits1EndTime)
            {
                creditsToDraw = credits1;
            }
            else if (t >= Credits2StartTime && t < Credits2EndTime)
            {
                creditsToDraw = credits2;
            }

            if (creditsToDraw == null || creditsToDraw.Length == 0)
                return;

            // Find the maximum width of the credits text
            int maxWidth = 0;
            foreach (string line in creditsToDraw)
            {
                if (line.Length > maxWidth)
                    maxWidth = line.Length;
            }

            // Calculate the area we need to draw (text + padding on all sides)
            int textHeight = creditsToDraw.Length;
            int totalWidth = maxWidth + CreditsPadding * 2;
            int totalHeight = textHeight + CreditsPadding * 2;

            // Start position for text: top left corner (0, 0) + padding
            int startX = CreditsPadding;
            int startY = CreditsPadding;

            // First, draw rectangular backdrop: grey outline (outermost pixels), black interior
            for (int y = 0; y < totalHeight && y < Screen_Height; y++)
            {
                for (int x = 0; x < totalWidth && x < Screen_Width; x++)
                {
                    bool isOutline = (x == 0 || x == totalWidth - 1 || y == 0 || y == totalHeight - 1);
                    Color32 c = isOutline ? CreditsOutlineGrey : CreditsBlackColor;
                    Framebuffer.SetPixel(x, y, c);
                }
            }

            // Then, draw only the green 'X' characters on top of the black background
            for (int lineIdx = 0; lineIdx < creditsToDraw.Length; lineIdx++)
            {
                int y = startY + lineIdx;
                if (y >= Screen_Height) break; // Don't draw beyond screen height

                string line = creditsToDraw[lineIdx];

                // Draw only the 'X' characters in green (spaces are already black from the backdrop)
                for (int x = 0; x < line.Length; x++)
                {
                    int screenX = startX + x;
                    if (screenX >= Screen_Width) break; // Don't draw beyond screen width

                    char ch = line[x];
                    if (ch == 'X')
                    {
                        Framebuffer.SetPixel(screenX, y, CreditsGreenColor);
                    }
                }
            }
        }
    }
}
