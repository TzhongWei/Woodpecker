using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;

namespace Woodpecker.Animation.Util.IO
{
    public static class ColourCodeIO
    {
        public static void SetDefaultColourCode()
        {
            var code = new Dictionary<string, List<Color>>();
            code["Primary"] = new List<Color> { Color.Black };
            code["Primary_Wire"] = new List<Color> { Color.White };
            code["Secondary"] = new List<Color> { Color.Red };
            code["Secondary_Wire"] = new List<Color> { Color.Yellow };
            code["Tertiary"] = new List<Color> { Color.Blue, Color.Green, Color.Black };
            ProjectAppManager.CCParameters = new ColourCodeParameters(code);
        }
        
        public static bool SetDefaultColourCode_Setfile()
        {
            var path = ProjectAppManager.ColourCodePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                ProjectAppManager.ColourCodePath = GetDefaultColourCodePath();
                path = ProjectAppManager.ColourCodePath;
            }
            SetDefaultColourCode();
            return JsonWrite.Save(ProjectAppManager.GeometryCodePath, ProjectAppManager.CCParameters.Values);
        }
        internal static string GetDefaultColourCodePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            // ColourCode is a new version of json
            return Path.Combine(dir, "ColourCode.json");
        }
        public static bool ReadColourFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "")
            {
                ProjectAppManager.ColourCodePath = GetDefaultColourCodePath();
                return SetDefaultColourCode_Setfile();
            }
            if (!File.Exists(path))
            {
                // Set default ColourCode and not setting the path
                SetDefaultColourCode();
                return false;
            }
            else
            {
                try
                {
                    var csscode = JsonRead.Load<Dictionary<string, List<string>>>(path);
                    if (csscode == null)
                    {
                        SetDefaultColourCode();
                        return false;
                    }
                    var colourCode = ColourCodeUtil.StringToColourDictionary(csscode);
                    ProjectAppManager.ColourCodePath = path;
                    ProjectAppManager.CCParameters = new ColourCodeParameters(colourCode);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool SaveColourCode()
        {
            var path = ProjectAppManager.ColourCodePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                ProjectAppManager.ColourCodePath = GetDefaultColourCodePath();
                path = ProjectAppManager.ColourCodePath;
            }
            if (ProjectAppManager.CCParameters == null || !ProjectAppManager.CCParameters.IsValid)
            {
                SetDefaultColourCode();
            }
            var cssCode = ColourCodeUtil.ColourDictionaryToString(ProjectAppManager.CCParameters.Values);
            return JsonWrite.Save(ProjectAppManager.ColourCodePath, cssCode);
        }
        public static bool CreateNewColourCode(string path, Dictionary<string, List<string>> cssDirct)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(path, @"\.json$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                path += ".json";
            // Ensure the file path ends with a valid .json extension.
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return JsonWrite.Save(path, cssDirct);
        }
    }
}