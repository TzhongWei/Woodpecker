namespace Woodpecker.Animation.Properties
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    internal class Resources
    {
        private const string SourceIconDirectory = @"/Volumes/File/IBOIS/Exhibition/03_Program/02_Code/IBOIS.Animation/icon";
        private static System.Resources.ResourceManager resourceMan;
        private static System.Globalization.CultureInfo resourceCulture;

        internal Resources() { }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("Woodpecker.Animation.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        private static System.Drawing.Bitmap LoadBitmap(string iconName)
        {
            var path = ResolveIconPath(iconName + ".png");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            using (var image = Image.FromFile(path))
            {
                return new Bitmap(image);
            }
        }

        private static string ResolveIconPath(string fileName)
        {
            var assemblyDirectory = GetAssemblyDirectory();

            var path = FindIconInDirectory(assemblyDirectory, fileName);
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            path = FindIconUpwards(assemblyDirectory, fileName);
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            path = FindIconInDirectory(AppContext.BaseDirectory, fileName);
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            path = FindIconUpwards(AppContext.BaseDirectory, fileName);
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            return FindIconInDirectory(SourceIconDirectory, fileName);
        }

        private static string GetAssemblyDirectory()
        {
            var assemblyLocation = typeof(Resources).Assembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
                assemblyLocation = Assembly.GetExecutingAssembly().Location;

            return string.IsNullOrWhiteSpace(assemblyLocation)
                ? null
                : Path.GetDirectoryName(assemblyLocation);
        }

        private static string FindIconInDirectory(string directory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            var direct = Path.Combine(directory, fileName);
            if (File.Exists(direct))
                return direct;

            var nested = Path.Combine(directory, "icon", fileName);
            return File.Exists(nested) ? nested : null;
        }

        private static string FindIconUpwards(string startDirectory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
                return null;

            var directory = new DirectoryInfo(startDirectory);
            for (var i = 0; directory != null && i < 8; i++)
            {
                var candidate = Path.Combine(directory.FullName, "icon", fileName);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return null;
        }

        internal static System.Drawing.Bitmap GH_Active_Circular_TL
        {
            get { return LoadBitmap("GH_Active_Circular_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Active_TL
        {
            get { return LoadBitmap("GH_Active_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Active_TL_In_Zone
        {
            get { return LoadBitmap("GH_Active_TL_In_Zone"); }
        }

        internal static System.Drawing.Bitmap GH_Add_End_Path
        {
            get { return LoadBitmap("GH_Add_End_Path"); }
        }

        internal static System.Drawing.Bitmap GH_Animation_Compile
        {
            get { return LoadBitmap("GH_Animation_Compile"); }
        }

        internal static System.Drawing.Bitmap GH_Animation_Compile_Setting
        {
            get { return LoadBitmap("GH_Animation_Compile_Setting"); }
        }

        internal static System.Drawing.Bitmap GH_Animation_Render
        {
            get { return LoadBitmap("GH_Animation_Render"); }
        }

        internal static System.Drawing.Bitmap GH_Animation_Render_Setting
        {
            get { return LoadBitmap("GH_Animation_Render_Setting"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Dolly
        {
            get { return LoadBitmap("GH_Cam_Dolly"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_FA2B
        {
            get { return LoadBitmap("GH_Cam_FA2B"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_LookAt
        {
            get { return LoadBitmap("GH_Cam_LookAt"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Orbit
        {
            get { return LoadBitmap("GH_Cam_Orbit"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Pan
        {
            get { return LoadBitmap("GH_Cam_Pan"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Param
        {
            get { return LoadBitmap("GH_Cam_Param"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Rotate
        {
            get { return LoadBitmap("GH_Cam_Rotate"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Zoom
        {
            get { return LoadBitmap("GH_Cam_Zoom"); }
        }

        internal static System.Drawing.Bitmap GH_Cam_Zoom_Target
        {
            get { return LoadBitmap("GH_Cam_Zoom_Target"); }
        }

        internal static System.Drawing.Bitmap GH_Colour_Load
        {
            get { return LoadBitmap("GH_Colour_Load"); }
        }

        internal static System.Drawing.Bitmap GH_Colour_Panel
        {
            get { return LoadBitmap("GH_Colour_Panel"); }
        }

        internal static System.Drawing.Bitmap GH_Colour_Sel
        {
            get { return LoadBitmap("GH_Colour_Sel"); }
        }

        internal static System.Drawing.Bitmap GH_Create_Cam
        {
            get { return LoadBitmap("GH_Create_Cam"); }
        }

        internal static System.Drawing.Bitmap GH_Create_Colour_Code
        {
            get { return LoadBitmap("GH_Create_Colour_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Create_Geom_Code
        {
            get { return LoadBitmap("GH_Create_Geom_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Create_Layer
        {
            get { return LoadBitmap("GH_Create_Layer"); }
        }

        internal static System.Drawing.Bitmap GH_Create_New_Colour_Code
        {
            get { return LoadBitmap("GH_Create_New_Colour_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Create_New_Geom_Code
        {
            get { return LoadBitmap("GH_Create_New_Geom_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Create_TL
        {
            get { return LoadBitmap("GH_Create_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Create_TL_By_Agg
        {
            get { return LoadBitmap("GH_Create_TL_By_Agg"); }
        }

        internal static System.Drawing.Bitmap GH_Dash_Crv
        {
            get { return LoadBitmap("GH_Dash_Crv"); }
        }

        internal static System.Drawing.Bitmap GH_Dash_Pattern_ValueList
        {
            get { return LoadBitmap("GH_Dash_Pattern_ValueList"); }
        }

        internal static System.Drawing.Bitmap GH_Del_Geom_Code
        {
            get { return LoadBitmap("GH_Del_Geom_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Display_By_Group
        {
            get { return LoadBitmap("GH_Display_By_Group"); }
        }

        internal static System.Drawing.Bitmap GH_Display_By_Name
        {
            get { return LoadBitmap("GH_Display_By_Name"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Geom
        {
            get { return LoadBitmap("GH_Display_Geom"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Geom_Cols
        {
            get { return LoadBitmap("GH_Display_Geom_Cols"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Vec
        {
            get { return LoadBitmap("GH_Display_Vec"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Vec_Crv
        {
            get { return LoadBitmap("GH_Display_Vec_Crv"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Vec_Setting
        {
            get { return LoadBitmap("GH_Display_Vec_Setting"); }
        }

        internal static System.Drawing.Bitmap GH_Display_Wire
        {
            get { return LoadBitmap("GH_Display_Wire"); }
        }

        internal static System.Drawing.Bitmap GH_Draw_Path
        {
            get { return LoadBitmap("GH_Draw_Path"); }
        }

        internal static System.Drawing.Bitmap GH_Easing
        {
            get { return LoadBitmap("GH_Easing"); }
        }

        internal static System.Drawing.Bitmap GH_Enable_By_Group
        {
            get { return LoadBitmap("GH_Enable_By_Group"); }
        }

        internal static System.Drawing.Bitmap GH_Enable_By_Name
        {
            get { return LoadBitmap("GH_Enable_By_Name"); }
        }

        internal static System.Drawing.Bitmap GH_File_Directory
        {
            get { return LoadBitmap("GH_File_Directory"); }
        }

        internal static System.Drawing.Bitmap GH_Get_Cam_Info
        {
            get { return LoadBitmap("GH_Get_Cam_Info"); }
        }

        internal static System.Drawing.Bitmap GH_Get_Colour_Code
        {
            get { return LoadBitmap("GH_Get_Colour_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Global_Slider
        {
            get { return LoadBitmap("GH_Global_Slider"); }
        }

        internal static System.Drawing.Bitmap GH_Global_Tag_IN
        {
            get { return LoadBitmap("GH_Global_Tag_IN"); }
        }

        internal static System.Drawing.Bitmap GH_Global_Tag_Out
        {
            get { return LoadBitmap("GH_Global_Tag_Out"); }
        }

        internal static System.Drawing.Bitmap GH_Interval_Range
        {
            get { return LoadBitmap("GH_Interval_Range"); }
        }

        internal static System.Drawing.Bitmap GH_Is_Active_TL
        {
            get { return LoadBitmap("GH_Is_Active_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Is_Tag
        {
            get { return LoadBitmap("GH_Is_Tag"); }
        }

        internal static System.Drawing.Bitmap GH_Iterative_Offset
        {
            get { return LoadBitmap("GH_Iterative_Offset"); }
        }

        internal static System.Drawing.Bitmap GH_Link_Path
        {
            get { return LoadBitmap("GH_Link_Path"); }
        }

        internal static System.Drawing.Bitmap GH_Load_Geom_Code
        {
            get { return LoadBitmap("GH_Load_Geom_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Match_List
        {
            get { return LoadBitmap("GH_Match_List"); }
        }

        internal static System.Drawing.Bitmap GH_Mirror_Frames
        {
            get { return LoadBitmap("GH_Mirror_Frames"); }
        }

        internal static System.Drawing.Bitmap GH_Orient_Action
        {
            get { return LoadBitmap("GH_Orient_Action"); }
        }

        internal static System.Drawing.Bitmap GH_Redefine_TL
        {
            get { return LoadBitmap("GH_Redefine_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Remap_T
        {
            get { return LoadBitmap("GH_Remap_T"); }
        }

        internal static System.Drawing.Bitmap GH_Rot_Cam
        {
            get { return LoadBitmap("GH_Rot_Cam"); }
        }

        internal static System.Drawing.Bitmap GH_Rotate_Fix_T_Action
        {
            get { return LoadBitmap("GH_Rotate_Fix_T_Action"); }
        }

        internal static System.Drawing.Bitmap GH_Rotate_M_T_Action
        {
            get { return LoadBitmap("GH_Rotate_M_T_Action"); }
        }

        internal static System.Drawing.Bitmap GH_Save_Colour_Code
        {
            get { return LoadBitmap("GH_Save_Colour_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Save_Geom_Code
        {
            get { return LoadBitmap("GH_Save_Geom_Code"); }
        }

        internal static System.Drawing.Bitmap GH_Seg_Non_TL
        {
            get { return LoadBitmap("GH_Seg_Non_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Seg_TL
        {
            get { return LoadBitmap("GH_Seg_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Sel_Geom
        {
            get { return LoadBitmap("GH_Sel_Geom"); }
        }

        internal static System.Drawing.Bitmap GH_Sequential_Geom_From_Layer
        {
            get { return LoadBitmap("GH_Sequential_Geom_From_Layer"); }
        }

        internal static System.Drawing.Bitmap GH_Shift_TL
        {
            get { return LoadBitmap("GH_Shift_TL"); }
        }

        internal static System.Drawing.Bitmap GH_Silhouette
        {
            get { return LoadBitmap("GH_Silhouette"); }
        }

        internal static System.Drawing.Bitmap GH_Sum_And
        {
            get { return LoadBitmap("GH_Sum_And"); }
        }

        internal static System.Drawing.Bitmap GH_Sum_Or
        {
            get { return LoadBitmap("GH_Sum_Or"); }
        }

        internal static System.Drawing.Bitmap GH_Tag_Channel_IN
        {
            get { return LoadBitmap("GH_Tag_Channel_IN"); }
        }

        internal static System.Drawing.Bitmap GH_Tag_Channel_Out
        {
            get { return LoadBitmap("GH_Tag_Channel_Out"); }
        }

        internal static System.Drawing.Bitmap GH_To_Cam
        {
            get { return LoadBitmap("GH_To_Cam"); }
        }

        internal static System.Drawing.Bitmap GH_Tranform_Action
        {
            get { return LoadBitmap("GH_Tranform_Action"); }
        }

        internal static System.Drawing.Bitmap GH_Translation_Action
        {
            get { return LoadBitmap("GH_Translation_Action"); }
        }

        internal static System.Drawing.Bitmap GH_ViewCaptureToFile
        {
            get { return LoadBitmap("GH_ViewCaptureToFile"); }
        }

        internal static System.Drawing.Bitmap GH_Visiable_Geom
        {
            get { return LoadBitmap("GH_Visiable_Geom"); }
        }

        internal static System.Drawing.Bitmap Woodpecker_Animation_Icon
        {
            get { return LoadBitmap("Woodpecker"); }
        }
    }
}
