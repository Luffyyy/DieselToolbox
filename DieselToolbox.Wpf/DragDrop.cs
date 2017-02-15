using DieselEngineFormats.Bundle;
using Eto.Wpf.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DieselToolbox.Wpf
{
    public class DragDropWPF : WpfControl<System.Windows.Controls.TextBox, DragDropController, DragDropController.ICallback>, DragDropController.IDragDropController
    {
        public bool OutputFullPaths { get; set; }

        public DragDropWPF()
        {
            this.Control = new TextBox();
            this.OutputFullPaths = false;
        }

        public void DoDragDrop(IChild child)
        {
            VirtualFileDataObject virtualFileDataObject = new VirtualFileDataObject();
            List<VirtualFileDataObject.FileDescriptor> files = new List<VirtualFileDataObject.FileDescriptor>();
            this.PopulateFile(files, (FileEntry)child, ((IViewable)child).Name);
            virtualFileDataObject.SetData(files);

            var effect = VirtualFileDataObject.DoDragDrop(this.Control, virtualFileDataObject, DragDropEffects.Copy);
            if (effect == DragDropEffects.None)
            {
                virtualFileDataObject = null;
            }
            Console.WriteLine("Finished Drag-Drop operation setup");
        }

        public void DoDragDrop(IParent parent)
        {
            VirtualFileDataObject virtualFileDataObject = new VirtualFileDataObject();
            List<VirtualFileDataObject.FileDescriptor> files = new List<VirtualFileDataObject.FileDescriptor>();
            this.PopulateFiles(files, parent, ((IViewable)parent).Name);
            virtualFileDataObject.SetData(files);

            var effect = VirtualFileDataObject.DoDragDrop(this.Control, virtualFileDataObject, DragDropEffects.Copy);
            if (effect == DragDropEffects.None)
            {
                virtualFileDataObject = null;
            }
            Console.WriteLine("Finished Drag-Drop operation setup");
        }

        public void PopulateFile(List<VirtualFileDataObject.FileDescriptor> files, FileEntry parent, string path = "")
        {
            if (parent.BundleEntries.Count == 0)
                return;

            string name = this.OutputFullPaths ? parent.Path : path;

            VirtualFileDataObject.FileDescriptor fileDescriptor = new VirtualFileDataObject.FileDescriptor()
            {
                Name = name,
                StreamContents = () =>
                {
                    MemoryStream stream = new MemoryStream();
                    PackageFileEntry maxBundleEntry = parent.MaxBundleEntry();
                    Console.WriteLine("Extracted {0} from package: {1}", name, maxBundleEntry.PackageName.ToString());
                    byte[] bytes = parent.FileBytes(maxBundleEntry);
                    if (bytes != null)
                        stream.Write(bytes, 0, bytes.Length);
                    return stream;
                }
            };
            files.Add(fileDescriptor);
        }

        public void PopulateFiles(List<VirtualFileDataObject.FileDescriptor> files, IParent parent, string path = "")
        {
            foreach (KeyValuePair<string, IChild> entry in parent.Children)
            {
                if (entry.Value is IParent)
                {
                    this.PopulateFiles(files, (IParent)entry.Value, Path.Combine(path ?? "", entry.Key ?? ""));
                }
                else if (entry.Value is FileEntry)
                {
                    this.PopulateFile(files, (FileEntry)entry.Value, Path.Combine(path ?? "", entry.Key));
                }
            }
        }
    }
}
