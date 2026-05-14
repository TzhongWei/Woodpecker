using Newtonsoft.Json.Serialization;
using System.IO;
using Rhino.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using System.Drawing;
using System.Text.RegularExpressions;
using System;
using System.Globalization;
using System.Security.Policy;


namespace Woodpecker.Animation.Util.IO
{
    public static class ColourCodeUtil
    {
        #region ColourCode Tool

        /// <summary>
        /// convert a dictionary of colour codes in color format to a dictionary of CSS format objects. The input dictionary maps Color to lists of CSS color strings (in rgb or rgba format), and the output dictionary maps the same colour to lists of CSS string.
        /// </summary>
        /// <param name="ColourCode"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ColourDictionaryToString(Dictionary<string, List<Color>> ColourCode)
        {
            var newDic = new Dictionary<string, List<string>>();
            foreach (var CC in ColourCode)
            {
                newDic[CC.Key] = CC.Value.Select(x => ParseColourCss(x)).ToList();
            }
            return newDic;
        }
        /// <summary>
        /// Convert a dictionary of colour codes in CSS format to a dictionary of System.Drawing.Color objects. The input dictionary maps colour names to lists of CSS color strings (in rgb or rgba format), and the output dictionary maps the same colour names to lists of Color objects.
        /// </summary>
        /// <param name="ColourCodeCss">A dictionary mapping colour names to lists of CSS color strings.</param>
        /// <returns>A dictionary mapping colour names to lists of Color objects.</returns>
        public static Dictionary<string, List<Color>> StringToColourDictionary(Dictionary<string, List<string>> ColourCodeCss)
        {
            var colourCode = new Dictionary<string, List<Color>>();
            foreach (var key in ColourCodeCss.Keys)
            {
                var value = ColourCodeCss[key];
                var colourList = new List<Color>();
                foreach (var colour in value)
                {
                    if (colour.StartsWith("rgba"))
                    {
                        var rgba = colour.Substring(5, colour.Length - 6).Split(',').Select(x => x.Trim()).ToArray();
                        var r = int.Parse(rgba[0]);
                        var g = int.Parse(rgba[1]);
                        var b = int.Parse(rgba[2]);
                        var a = (int)(float.Parse(rgba[3]) * 255);
                        colourList.Add(Color.FromArgb(a, r, g, b));
                    }
                    else if (colour.StartsWith("rgb"))
                    {
                        var rgb = colour.Substring(4, colour.Length - 5).Split(',').Select(x => x.Trim()).ToArray();
                        var r = int.Parse(rgb[0]);
                        var g = int.Parse(rgb[1]);
                        var b = int.Parse(rgb[2]);
                        colourList.Add(Color.FromArgb(255, r, g, b));
                    }
                    else
                    {
                        throw new Exception("Format error: colour string must start with 'rgb' or 'rgba'.");
                    }
                }
                colourCode[key] = colourList;
            }
            return colourCode;
        }
        public static string ParseColourCss(Color color)
        {
            var r = color.R;
            var g = color.G;
            var b = color.B;
            var a = color.A;
            if (a < 255)
            {
                var alpha = (float)a / 255;
                return $"rgba({r}, {g}, {b}, {alpha})";
            }
            else
            {
                return $"rgb({r}, {g}, {b})";
            }
        }
        /// <summary>
        /// Parse a CSS color string (in rgb or rgba format) and return a System.Drawing.Color object. If the input string is not in a valid format, it returns an empty Color object.
        /// </summary>
        /// <param name="css">The CSS color string to parse.</param>
        /// <returns>The parsed Color object.</returns>
        public static Color ParseCssColour(string css)
        {
            css = css.Trim().ToLowerInvariant();
            var rgbMatch = Regex.Match(css, @"rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
            if (rgbMatch.Success)
            {
                return Color.FromArgb(
                    255,
                    int.Parse(rgbMatch.Groups[1].Value),
                    int.Parse(rgbMatch.Groups[2].Value),
                    int.Parse(rgbMatch.Groups[3].Value)
                );
            }

            var rgbaMatch = Regex.Match(css, @"rgba\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-9.]+)\s*\)");
            if (rgbaMatch.Success)
            {
                double a = double.Parse(rgbaMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                int alpha = (int)Math.Round(a * 255.0);

                return Color.FromArgb(
                    alpha,
                    int.Parse(rgbaMatch.Groups[1].Value),
                    int.Parse(rgbaMatch.Groups[2].Value),
                    int.Parse(rgbaMatch.Groups[3].Value)
                );
            }

            return new Color();
        }

        #endregion
        /// <summary>
        /// Get the colour code from the file data, if the file does not exist, it will create a new one with default colour code and return it.
        /// <b> Please use GetColourFromPath </b>
        /// </summary>
        /// <param name="path">The path to the colour code file.</param>
        /// <returns>A dictionary mapping colour names to lists of colors.</returns>
        [Obsolete]
        public static Dictionary<string, List<Color>> GetColourCode(string path)
        {
            var code = JsonRead.Load<Dictionary<string, List<string>>>(path);
            var colourCode = new Dictionary<string, List<Color>>();
            foreach (var key in code.Keys)
            {
                var value = code[key];
                var colourList = new List<Color>();
                foreach (var colour in value)
                {
                    if (colour.StartsWith("rgba"))
                    {
                        var rgba = colour.Substring(5, colour.Length - 6).Split(',').Select(x => x.Trim()).ToArray();
                        var r = int.Parse(rgba[0]);
                        var g = int.Parse(rgba[1]);
                        var b = int.Parse(rgba[2]);
                        var a = (int)(float.Parse(rgba[3]) * 255);
                        colourList.Add(Color.FromArgb(a, r, g, b));
                    }
                    else if (colour.StartsWith("rgb"))
                    {
                        var rgb = colour.Substring(4, colour.Length - 5).Split(',').Select(x => x.Trim()).ToArray();
                        var r = int.Parse(rgb[0]);
                        var g = int.Parse(rgb[1]);
                        var b = int.Parse(rgb[2]);
                        colourList.Add(Color.FromArgb(255, r, g, b));
                    }
                }
                colourCode[key] = colourList;
            }
            return colourCode;
        }
        /// <summary>
        /// Save the colour code to the file, the colour code is a dictionary mapping colour names to lists of colors, the colors will be saved in rgba or rgb format depending on their alpha value.
        /// </summary>
        /// <param name="path">The path to the colour code file.</param>
        /// <param name="colourCode">The colour code dictionary.</param>
        [Obsolete]
        public static bool SaveColourCode(string path, Dictionary<string, List<string>> colourCode)
        {
            if (File.Exists(path))
            {
                var backupPath = path + ".bak";
                File.Copy(path, backupPath, true);
            }
            return JsonWrite.Save(path, colourCode);
        }


        /// <summary>
        /// set the default colour code to the file, even the file is existing, it will overwrite it with the default colour code. The default colour code contains 8 colour categories: "Wood_Colour", "Wire", "CNC", "Stand", "Milling", "Gradient", "Blade" and "RotaryAxis". Each category has a list of colours in CSS format (rgb or rgba).
        /// </summary>
        [Obsolete]
        public static void SetDefaultColourCode_Old()
        {
            var dir = Path.Combine("./", "data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, "ColourCode.json");
            if (File.Exists(path))
            {
                var backupPath = path + ".bak";
                File.Copy(path, backupPath, true);
            }
            var code = new Dictionary<string, List<string>>();
            code["Wood_Colour"] = new List<string> { "rgba(219, 249, 73, 0.61)" };
            code["Wire"] = new List<string> { "rgb(255, 255, 255)" };
            code["CNC"] = new List<string> { "rgb(195, 195, 195)" };
            code["Stand"] = new List<string> { "rgb(0, 225, 255)" };
            code["Milling"] = new List<string> { "rgb(40, 242, 0)" };
            code["Gradient"] = new List<string> { "rgb(255, 255, 255)", "rgba(255, 121, 121, 0.39)", "rgba(254, 232, 35, 0.61)" };
            code["Blade"] = new List<string> { "rgb(191, 209, 251)" };
            code["RotaryAxis"] = new List<string> { "rgb(183, 183, 183)" };

            JsonWrite.Save(path, code);
        }
        /// <summary>
        /// Set the default colour code to the file, if the file already exists, it will not overwrite it and return false, if the file does not exist, it will create a new one with default colour code and return true.
        /// </summary>
        /// <param name="path">The path to the colour code file.</param>
        /// <returns>true if the default colour code was set, false otherwise.</returns>
        [Obsolete]
        public static bool SetDefaultColourCode(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "")
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "data");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                path = Path.Combine(dir, "ColourCode.json");
            }
            if (!File.Exists(path))
            {
                var code = new Dictionary<string, List<string>>();
                code["Wood_Colour"] = new List<string> { "rgba(219, 249, 73, 0.61)" };
                code["Wire"] = new List<string> { "rgb(255, 255, 255)" };
                code["CNC"] = new List<string> { "rgb(195, 195, 195)" };
                code["Stand"] = new List<string> { "rgb(0, 225, 255)" };
                code["Milling"] = new List<string> { "rgb(40, 242, 0)" };
                code["Gradient"] = new List<string> { "rgb(255, 255, 255)", "rgba(255, 121, 121, 0.39)", "rgba(254, 232, 35, 0.61)" };
                code["Blade"] = new List<string> { "rgb(191, 209, 251)" };
                code["RotaryAxis"] = new List<string> { "rgb(183, 183, 183)" };

                JsonWrite.Save(path, code);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Draw a checkerboard pattern on the given graphics object within the specified rectangle and cell size. This is typically used to indicate transparency in color displays, where the checkerboard pattern serves as a background to show that certain areas are transparent. The method fills the rectangle with alternating light and dark squares based on the specified cell size.
        /// </summary>
        /// <param name="graphics">The graphics object on which to draw the checkerboard pattern.</param>
        /// <param name="rect">The rectangle within which to draw the checkerboard pattern.</param>
        /// <param name="cell">The size of each cell in the checkerboard pattern.</param>
        internal static void DrawCheckerboard(Graphics graphics, RectangleF rect, float cell)
        {
            using (var light = new SolidBrush(Color.FromArgb(245, 245, 245)))
            using (var dark = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                for (float yy = rect.Top; yy < rect.Bottom; yy += cell)
                {
                    for (float xx = rect.Left; xx < rect.Right; xx += cell)
                    {
                        bool even = (((int)((xx - rect.Left) / cell) + (int)((yy - rect.Top) / cell)) % 2 == 0);
                        graphics.FillRectangle(even ? light : dark, xx, yy, cell, cell);
                    }
                }
            }
        }
    }
}