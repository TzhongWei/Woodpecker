using System.IO;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.CodeManager
{
    public interface ISelectExistFile: IEditableWindow
    {
        void Select_SingleExistingFileClicked();
        void After_Select_RefreshComponent(); 
    }
    public static class SelectExistFileExtensions
    {
        public static void Select_SingleExistingFileClicked<T>(T owner, string Text) where T : GH_Component, ISelectExistFile
        {
            using (var dialog = new Eto.Forms.OpenFileDialog())
            {
                dialog.Title = Text;
                dialog.MultiSelect = false;
                dialog.CheckFileExists = true;
                dialog.Filters.Add(new Eto.Forms.FileFilter("JSON files", ".json"));

                var result = dialog.ShowDialog(Instances.EtoDocumentEditor);
                if (result != Eto.Forms.DialogResult.Ok) return;

                var path = dialog.FileName;
                if (string.IsNullOrWhiteSpace(path)) return;

                owner.RecordUndoEvent("Select a colourcode file");
                if (!ColourCodeIO.ReadColourFromPath(path))
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to read colour code file: {path}");
                }

                owner.After_Select_RefreshComponent();
                owner.ExpireSolution(true);
            }
        }
    }
}