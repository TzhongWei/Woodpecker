using System.IO;
using Rhino.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;


namespace Woodpecker.Animation.Util.IO
{
    public static class JsonRead
    {
        public static T Load<T>(string file)
        {
            try
            {
                if (!File.Exists(file))
                    return default(T);

                var json = File.ReadAllText(file);

                if (string.IsNullOrWhiteSpace(json))
                    return default(T);

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }
    }

    public static class JsonWrite
    {
        public static bool Save<T>(string file, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(file, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
}
    