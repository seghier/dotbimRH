using Rhino;
using Rhino.Commands;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace import_DOTBIM
{
    [CommandStyle(Style.ScriptRunner)]
    public class OpenDotBim : Command
    {
        public override string EnglishName => "ImportDOTBIM";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import BIM File",
                Filter = "BIM Files (*.bim)|*.bim",
            };

            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
                return Result.Cancel;

            string filename = openFileDialog.FileName;

            if (!System.IO.File.Exists(filename))
            {
                RhinoApp.WriteLine("File not found");
                return Result.Failure;
            }

            var extension = Path.GetExtension(filename);

            if (!string.Equals(extension, ".bim", StringComparison.InvariantCultureIgnoreCase))
            {
                RhinoApp.WriteLine("Not a BIM file.");
                return Result.Failure;
            }

            try
            {
                // Use the dotbim library to open and process the BIM file
                var model = dotbim.File.Read(filename);
                var rhinoGeometries = Tools.ConvertBimMeshesAndElementsIntoRhinoMeshes(model.Meshes, model.Elements);

                foreach (var geo in rhinoGeometries)
                {
                    if (geo != null)
                    {
                        int id = rhinoGeometries.IndexOf(geo);
                        var minfo = model.Elements[id].Info;
                        var attributes = new Rhino.DocObjects.ObjectAttributes();

                        string mguid = model.Elements[id].Guid;
                        string mtype = model.Elements[id].Type;

                        attributes.SetUserString("Guid", mguid);
                        attributes.SetUserString("Type", mtype);

                        attributes.ObjectId = Guid.Parse(mguid);
                        foreach (var kvp in minfo)
                        {
                            attributes.SetUserString(kvp.Key, kvp.Value);
                            geo.SetUserString(kvp.Key, kvp.Value);
                        }

                        // to use in Grasshopper
                        geo.SetUserString("Guid", mguid);
                        geo.SetUserString("Type", mtype);
                        geo.Compact();

                        doc.Objects.Add(geo, attributes);
                    }
                }

                // Redraw the viewport to display the newly added geometry

                doc.Views.Redraw();

                RhinoApp.WriteLine("BIM file opened and visualized successfully!");
                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error opening BIM file: {ex.Message}");
                return Result.Failure;
            }
        }

    }

}

/*/
public class SaveDotBim : Command
{
    List<IElementSetConvertable> elementSetConvertables = new List<IElementSetConvertable>();
    Dictionary<string, string> info = new Dictionary<string, string>{{"", ""}};

public override string EnglishName => "SaveDotbim";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save Dotbim File",
            Filter = "BIM Files (*.bim)|*.bim",
        };

        var dialogResult = saveFileDialog.ShowDialog();

        if (dialogResult != DialogResult.OK)
            return Result.Cancel;

        string filename = saveFileDialog.FileName;

        try
        {
            var ditbimFile = Tools.CreateFile(elementSetConvertables, info);
            ditbimFile.Save(filename);

            RhinoApp.WriteLine("Dotbim file saved successfully!");
            return Result.Success;
        }

        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error saving Dotbim file: {ex.Message}");
            return Result.Failure;
        }
    }

    List<BimElement> MeshToDotim(List<Rhino.Geometry.Mesh> meshes, string type, System.Drawing.Color color, Dictionary<string, string> info)
    {
        List<BimElement> elements = new List<BimElement>();
        foreach (var mesh in meshes)
        {
            BimElement bimElement = new BimElement(mesh, type, color, info);
            elements.Add(bimElement);
        }
        return elements;
    }
/*/


