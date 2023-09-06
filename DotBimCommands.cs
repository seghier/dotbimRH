using DotBimCommands.Interfaces;
using Rhino;
using Rhino.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DotBimCommands
{
    [CommandStyle(Style.ScriptRunner)]
    public class OpenDotBim : Command
    {
        public override string EnglishName => "OpenDotbim";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Dotbim File",
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
                RhinoApp.WriteLine("Not a Dotbim file.");
                return Result.Failure;
            }

            try
            {
                // Use the dotbim library to open and process the BIM file
                var model = dotbim.File.Read(filename);
                var rhinoGeometries = Tools.ConvertBimMeshesAndElementsIntoRhinoMeshes(model.Meshes, model.Elements);

                foreach (var geo in rhinoGeometries)
                {
                    // Add the Rhino geometry to the Rhino document
                    if (geo != null)
                        doc.Objects.Add(geo);
                }
                // Redraw the viewport to display the newly added geometry
                doc.Views.Redraw();

                RhinoApp.WriteLine("Dotbim file opened and visualized successfully!");
                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error opening Dotbim file: {ex.Message}");
                return Result.Failure;
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
}

