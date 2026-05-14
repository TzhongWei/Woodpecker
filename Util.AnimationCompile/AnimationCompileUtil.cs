using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Rhino;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    internal static class AnimationCompileUtil
    {
        private static void CheckAnimationPath(AnimationSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.InputFolder))
            {
                var newInputPath = ProjectAppManager.Get_DataRoot + "/AnimationFrames";
                setting.InputFolder = newInputPath;
            }
            if (string.IsNullOrWhiteSpace(setting.OutputFolder))
            {
                setting.OutputFolder = ProjectAppManager.Get_DataRoot + "/AnimationOutput";
            }
        }
        internal static bool MirrorFrames(AnimationSetting setting, bool horizontal = true)
        {
            try{
            var extension = setting.FrameExtension.TrimStart('.');
            var files = Directory.GetFiles(
                setting.InputFolder,
                $"{setting.FramePrefix}*.{extension}")
                .OrderBy(x => x)
                .ToList();

            foreach (var file in files)
            {
                using (var bitmap = new System.Drawing.Bitmap(file))
                {
                    bitmap.RotateFlip(horizontal
                        ? System.Drawing.RotateFlipType.RotateNoneFlipX
                        : System.Drawing.RotateFlipType.RotateNoneFlipY);

                    bitmap.Save(file);
                }
            }
            return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool Compile(AnimationSetting setting)
        {
            RhinoApp.WriteLine("Start Animation Compile");
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            CheckAnimationPath(setting);
            Directory.CreateDirectory(ProjectAppManager.Get_DataRoot);

            ValidateSetting(setting);

            var ffmpegPath = FindExecutable("ffmpeg");
            if (string.IsNullOrWhiteSpace(ffmpegPath))
            {
                AskToInstallFfmpeg();
                throw new FileNotFoundException("ffmpeg was not found. Install ffmpeg or make sure it is available in PATH.");
            }

            try
            {
                var outputPath = Path.Combine(setting.OutputFolder, setting.OutputName);
                var inputPattern = $"{setting.FramePrefix}%0{setting.FrameDigits}d.{setting.FrameExtension.TrimStart('.')}";

                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = BuildFfmpegArguments(setting, inputPattern, outputPath),
                    WorkingDirectory = setting.InputFolder,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(psi))
                {
                    if (process == null)
                        throw new Exception("Failed to start ffmpeg process.");
                    var error = process.StandardError.ReadToEnd();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Animation compile failed.", ex);

            }
            RhinoApp.WriteLine("Finish Animation Compile");
            return true;
        }
        private static void ValidateSetting(AnimationSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.InputFolder) || !Directory.Exists(setting.InputFolder))
                throw new DirectoryNotFoundException($"Input folder does not exist: {setting.InputFolder}");

            if (string.IsNullOrWhiteSpace(setting.OutputFolder))
                throw new DirectoryNotFoundException("Output folder is empty.");

            if (setting.FrameDuration <= 0)
                throw new ArgumentException("FrameDuration must be greater than 0.", nameof(setting.FrameDuration));

            if (setting.FrameDigits <= 0)
                throw new ArgumentException("FrameDigits must be greater than 0.", nameof(setting.FrameDigits));

            Directory.CreateDirectory(setting.OutputFolder);

            setting.FramePrefix = setting.FramePrefix ?? string.Empty;

            setting.FrameExtension = string.IsNullOrWhiteSpace(setting.FrameExtension)
                ? "jpg"
                : setting.FrameExtension.TrimStart('.');

            setting.OutputName = string.IsNullOrWhiteSpace(setting.OutputName)
                ? "result.mov"
                : setting.OutputName;

            if (!setting.OutputName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
                setting.OutputName += ".mov";

            var firstFrame = Path.Combine(
                setting.InputFolder,
                $"{setting.FramePrefix}{0.ToString().PadLeft(setting.FrameDigits, '0')}.{setting.FrameExtension}");

            if (!File.Exists(firstFrame))
                throw new FileNotFoundException($"First frame does not exist: {firstFrame}", firstFrame);
        }

        private static string BuildFfmpegArguments(AnimationSetting setting, string inputPattern, string outputPath)
        {

            if (File.Exists(outputPath))
            {
                if (setting.Overwrite)
                {
                    File.Delete(outputPath);
                }
                else
                {
                    var backupMov = outputPath + ".bak";
                    File.Copy(outputPath, backupMov);
                }
            }


            return string.Join(" ", new[]
            {
                "-y",
                "-framerate", setting.FPS.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "-i", Quote(inputPattern),
                "-c:v", "libx264",
                "-pix_fmt", "yuv420p",
                "-crf", "18",
                Quote(outputPath)
            });
        }

        private static string FindExecutable(string name)
        {
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var executableName = IsWindows() ? $"{name}.exe" : name;
            var candidates = path
                .Split(Path.PathSeparator)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Path.Combine(x, executableName))
                .Concat(GetDefaultExecutableLocations(name));

            return candidates.FirstOrDefault(File.Exists) ?? string.Empty;
        }

        private static IEnumerable<string> GetDefaultExecutableLocations(string name)
        {
            if (IsWindows())
            {
                yield return $@"C:\ffmpeg\bin\{name}.exe";

                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (!string.IsNullOrWhiteSpace(programFiles))
                    yield return Path.Combine(programFiles, "ffmpeg", "bin", $"{name}.exe");

                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(localAppData))
                    yield return Path.Combine(localAppData, "Microsoft", "WinGet", "Links", $"{name}.exe");

                yield return $@"C:\ProgramData\chocolatey\bin\{name}.exe";
            }
            else if (IsMacOS())
            {
                yield return $"/opt/homebrew/bin/{name}";
                yield return $"/usr/local/bin/{name}";
                yield return $"/usr/bin/{name}";
            }
            else
            {
                yield return $"/usr/bin/{name}";
                yield return $"/usr/local/bin/{name}";
                yield return $"/snap/bin/{name}";
            }
        }

        private static bool IsWindows()
        => Environment.OSVersion.Platform == PlatformID.Win32NT;

        private static bool IsMacOS()
        => Environment.OSVersion.Platform == PlatformID.Unix &&
           Directory.Exists("/Applications") &&
           Directory.Exists("/System");

        private static void AskToInstallFfmpeg()
        {
            var message =
                "ffmpeg was not found. It is required to compile animation frames into a .mov file.\n\n" +
                GetFfmpegInstallHint() +
                "\n\nOpen the ffmpeg download page?";

            var result = MessageBox.Show(
                message,
                "ffmpeg not found",
                MessageBoxButtons.YesNo,
                MessageBoxType.Warning);

            if (result == DialogResult.Yes)
                OpenUrl("https://ffmpeg.org/download.html");
        }

        private static string GetFfmpegInstallHint()
        {
            if (IsWindows())
                return "Windows: install ffmpeg and add ffmpeg.exe to PATH.";

            if (IsMacOS())
                return "macOS: install with Homebrew using `brew install ffmpeg`, or download ffmpeg manually.";

            return "Install ffmpeg with your system package manager and make sure it is available in PATH.";
        }

        private static void OpenUrl(string url)
        {
            try
            {
                if (IsWindows())
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (IsMacOS())
                {
                    Process.Start("open", url);
                }
                else
                {
                    Process.Start("xdg-open", url);
                }
            }
            catch
            {
                RhinoApp.WriteLine($"Please open this URL to install ffmpeg: {url}");
            }
        }

        private static string Quote(string value)
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

    }
}
