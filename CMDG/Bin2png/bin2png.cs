// Converts CMDG .bin frame files (frames_bin folder) to PNG images (frames_png folder),
// and optionally creates a 3840x2160 60fps video with ffmpeg.
//
// Arguments:
//   -video   Only create video from existing PNGs (no bin→png).
//   -novideo Only create PNGs (no video).
//   (none)   Do both: bin→png, then png→video.
//
// To use, first run CMDG with DiskRenderer = true in config.cs
// You'll get a frames_bin folder. Place it in bin2png/bin/net8.0/, then run bin2png.exe.


using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

const string InputDir = "frames_bin";
const string OutputDir = "frames_png";
const string VideoOutputFile = "output.mp4";
const int TargetWidth = 3840;
const int TargetHeight = 2160;
const int VideoFps = 60;
const string FontName = "Consolas";
const float FontSize = 16f;

bool onlyVideo = args.Contains("-video", StringComparer.OrdinalIgnoreCase);
bool noVideo = args.Contains("-novideo", StringComparer.OrdinalIgnoreCase);

// If both flags, do both; otherwise -video => only video, -novideo => only PNGs
bool doPngs = (onlyVideo && noVideo) || !onlyVideo;
bool doVideo = (onlyVideo && noVideo) || !noVideo;

if (doPngs)
{
    if (!Directory.Exists(InputDir))
    {
        Console.WriteLine($"Input directory '{InputDir}' not found.");
        return 1;
    }

    Directory.CreateDirectory(OutputDir);

    string[] binFiles = Directory.GetFiles(InputDir, "*.bin")
        .OrderBy(Path.GetFileName, StringComparer.Ordinal)
        .ToArray();

    if (binFiles.Length == 0)
    {
        Console.WriteLine($"No .bin files found in '{InputDir}'.");
        if (!doVideo) return 0;
    }
    else
    {
        Console.WriteLine($"Converting {binFiles.Length} frames from {InputDir} to {OutputDir} (Consolas {FontSize}pt, multithreaded)...");

        (float cellW, float cellH) = MeasureCellSize(FontName, FontSize);

        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        int done = 0;
        Parallel.ForEach(binFiles, options, (binPath) =>
        {
            string name = Path.GetFileNameWithoutExtension(binPath);
            string outPath = Path.Combine(OutputDir, name + ".png");
            try
            {
                ConvertBinToPng(binPath, outPath, FontName, FontSize, cellW, cellH);
                int n = Interlocked.Increment(ref done);
                if (n % 100 == 0 || n == binFiles.Length)
                    Console.WriteLine($"  {n}/{binFiles.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error {binPath}: {ex.Message}");
            }
        });

        Console.WriteLine($"Done. Output in '{OutputDir}'.");
    }
}

if (doVideo)
{
    string? ffmpegPath = FindFfmpeg();
    if (ffmpegPath == null)
    {
        Console.WriteLine("ffmpeg.exe not found in current directory or next to executable.");
        return 1;
    }

    if (!Directory.Exists(OutputDir))
    {
        Console.WriteLine($"PNG directory '{OutputDir}' not found. Run without -video first to create PNGs.");
        return 1;
    }

    string[] pngFiles = Directory.GetFiles(OutputDir, "*.png")
        .OrderBy(Path.GetFileName, StringComparer.Ordinal)
        .ToArray();

    if (pngFiles.Length == 0)
    {
        Console.WriteLine($"No .png files found in '{OutputDir}'.");
        return 1;
    }

    Console.WriteLine($"Creating video @ {VideoFps}fps from {pngFiles.Length} frames (using PNG resolution)...");
    if (!RunFfmpeg(ffmpegPath, OutputDir, VideoOutputFile, VideoFps))
    {
        Console.WriteLine("ffmpeg failed.");
        return 1;
    }
    Console.WriteLine($"Video saved to '{VideoOutputFile}'.");
}

return 0;

static string? FindFfmpeg()
{
    string currentDir = Directory.GetCurrentDirectory();
    string exeDir = AppContext.BaseDirectory;
    string path1 = Path.Combine(currentDir, "ffmpeg.exe");
    string path2 = Path.Combine(exeDir, "ffmpeg.exe");
    if (File.Exists(path1)) return path1;
    if (File.Exists(path2)) return path2;
    return null;
}

static bool RunFfmpeg(string ffmpegPath, string pngDir, string outputFile, int fps)
{
    // High quality, animation tune: use PNG resolution directly, libx264, slow preset, crf 18
    string inputPattern = Path.Combine(pngDir, "%06d.png");
    string args = $"-y -framerate {fps} -i \"{inputPattern}\" -c:v libx264 -preset slow -tune animation -crf 18 -pix_fmt yuv420p \"{outputFile}\"";
    using var process = new Process();
    process.StartInfo.FileName = ffmpegPath;
    process.StartInfo.Arguments = args;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.Start();
    string stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        Console.WriteLine(stderr);
        return false;
    }
    return true;
}

static (float cellW, float cellH) MeasureCellSize(string fontName, float fontSize)
{
    // Use GenericTypographic for tight bounds (no extra padding between characters)
    using var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
    using var sf = new StringFormat(StringFormat.GenericTypographic) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };
    using var bmp = new Bitmap(1, 1);
    using var g = Graphics.FromImage(bmp);
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    var size = g.MeasureString("M", font, PointF.Empty, sf);
    float w = Math.Max(1f, size.Width);
    float h = Math.Max(1f, size.Height);
    return (w, h);
}

static void ConvertBinToPng(string binPath, string pngPath, string fontName, float fontSize, float cellW, float cellH)
{
    int w, h;
    byte[] rgb;
    char[] chars;
    using (var fs = new FileStream(binPath, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (var reader = new BinaryReader(fs))
    {
        w = reader.ReadInt32();
        h = reader.ReadInt32();
        int pixelCount = w * h;
        rgb = reader.ReadBytes(pixelCount * 3);
        chars = new char[pixelCount];
        for (int i = 0; i < pixelCount; i++)
            chars[i] = reader.ReadChar();
    }

    // Render content at natural size
    int contentW = (int)Math.Ceiling(w * cellW);
    int contentH = (int)Math.Ceiling(h * cellH);
    
    // Create target-size bitmap (3840x2160)
    using var bitmap = new Bitmap(TargetWidth, TargetHeight);
    using var g = Graphics.FromImage(bitmap);
    g.Clear(Color.Black);
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    using var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
    using var sf = new StringFormat(StringFormat.GenericTypographic) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };

    // Calculate offset to center the content
    int offsetX = (TargetWidth - contentW) / 2;
    int offsetY = (TargetHeight - contentH) / 2;

    // Draw characters centered in the 3840x2160 canvas
    for (int y = 0; y < h; y++)
    {
        for (int x = 0; x < w; x++)
        {
            int i = y * w + x;
            byte r = rgb[i * 3];
            byte gr = rgb[i * 3 + 1];
            byte b = rgb[i * 3 + 2];
            char ch = chars[i];
            using var brush = new SolidBrush(Color.FromArgb(r, gr, b));
            float px = offsetX + x * cellW;
            float py = offsetY + y * cellH;
            g.DrawString(ch.ToString(), font, brush, px, py, sf);
        }
    }

    bitmap.Save(pngPath, ImageFormat.Png);
}
