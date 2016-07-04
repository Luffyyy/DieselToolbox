using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace DieselToolbox
{
    public class ListViewBundleItem : ListBoxItem
    {
        public string package { get; set; }

        public string packageName { get; set; }

        public string size { get; set; }
    }

    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow()
        {
            InitializeComponent();
        }

        public object focusedObject;

        public void SetFile(FileEntry file)
        {
            this.Title = file.Name + " Properties";
            this.txtFileName.Text = file.Name;

            int currentSize = 0;
            foreach(BundleEntryHolder be in file.bundleEntries)
                currentSize += be.Entry.Length;

            currentSize = currentSize / file.bundleEntries.Count;

            if (currentSize < 1024)
                this.txtFileSize.Text = currentSize.ToString() + " B";
            else
                this.txtFileSize.Text = string.Format("{0:n0}", currentSize / 1024) + " KB";
            


            this.txtFileType.Text = file.extension;

            foreach (BundleEntryHolder entry in file.bundleEntries)
            {
                ListViewBundleItem lstItem = new ListViewBundleItem
                {
                    package = Path.GetFileName(entry.Package),
                    packageName = entry.PackageName,
                    size = entry.GetFileSizeString(),
                    Tag = entry
                };
                this.lstBundleEntries.Items.Add(lstItem);
            }

            focusedObject = file;
        }

        public void SetFolder(FolderStruct folderStruct)
        {
            
        }
    }

    
}
