using System;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Security.Policy;

namespace Woodpecker.Animation.Util.IO
{
    public static class ProjectAppManager
    {
        #region PathSetting
        private static void _setroot()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            //var appdata = Assembly.GetExecutingAssembly().Location;
            var dir = Path.Combine(appdata, "Woodpecker", "Animation_Data");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _dataFolder = dir;
        }
        public static string Get_DataRoot => _dataFolder;
        private static string _dataFolder { get; set; }
        private static string _geometryCodeFileName { get; set; } = "GeometryCode.json";
        private static string _colourCodeFileName {get; set;} = "ColourCode.json";
        public static string ColourCodePath
        {
            get
            {
                return Path.Combine(_dataFolder, _colourCodeFileName);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _setroot();
                    _colourCodeFileName = "ColourCode.json";
                    return;
                }

                var directory = Path.GetDirectoryName(value);
                var fileName = Path.GetFileName(value);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    _dataFolder = directory;
                }

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    _colourCodeFileName = fileName;
                }
            }
        }
        public static string GeometryCodePath
        {
            get
            {
                return Path.Combine(_dataFolder, _geometryCodeFileName);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _setroot();
                    _geometryCodeFileName = "GeometryCode.json";
                    return;
                }

                var directory = Path.GetDirectoryName(value);
                var fileName = Path.GetFileName(value);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    _dataFolder = directory;
                }

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    _geometryCodeFileName = fileName;
                }
            }
        }
        #endregion
        static ProjectAppManager()
        {
            _setroot();
            _geometryCodeFileName = "GeometryCode.json";
            _colourCodeFileName = "ColourCode.json";
        }
        #region DataManager
        public static GeometryCodeParameters GCParameters { get; set; } = null;
        public static ColourCodeParameters CCParameters {get; set;} = null;
        #endregion
    }
}