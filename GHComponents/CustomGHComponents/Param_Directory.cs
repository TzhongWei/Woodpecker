using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using WinForms = System.Windows.Forms;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class Param_Directory : Param_String
    {
        public Param_Directory():base()
        {
            this.Name = "Directory Selection";
            this.NickName = "DSel";
            this.Category = "Woodpecker";
            this.Description = "Select a directory";
            this.SubCategory = "Util";
        }
        protected override Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("3c8b0dbb-e1e5-46e8-a6a1-ae0424711d54");

        public override void AppendAdditionalMenuItems(WinForms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Select directory...", SelectDirectoryClicked);
        }

        private void SelectDirectoryClicked(object sender, EventArgs e)
        {
            using (var dialog = new Eto.Forms.SelectFolderDialog())
            {
                dialog.Title = "Select directory";

                var currentPath = VolatileDataCount > 0 ? VolatileData.AllData(true).FirstOrDefault()?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
                {
                    dialog.Directory = currentPath;
                }

                var result = dialog.ShowDialog(Grasshopper.Instances.EtoDocumentEditor);
                if (result != Eto.Forms.DialogResult.Ok || string.IsNullOrWhiteSpace(dialog.Directory)) return;

                RecordUndoEvent("Select directory");
                PersistentData.Clear();
                PersistentData.Append(new GH_String(dialog.Directory));
                ExpireSolution(true);
            }
        }
    }
}
