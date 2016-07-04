using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using DieselEngineFormats.Utils;
using DieselEngineFormats.Bundle;

namespace DieselToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        public BundleBrowser Browser;

        ProgressDialog prgDialog;

        public BrowserWindow()
        {
            InitializeComponent();
            this.mainFolderTree.SelectedItemChanged += ChangeFolder;
        }

        private void propertiesClicked(object sender, RoutedEventArgs e)
        {
            if (this.lstFolderContents.SelectedItem != null)
            {
                PropertiesWindow propWindow = new PropertiesWindow();

                if (this.lstFolderContents.SelectedItem is FileEntry)
                    propWindow.SetFile((FileEntry)this.lstFolderContents.SelectedItem);
                else if (this.lstFolderContents.SelectedItem is FolderStruct)
                    propWindow.SetFolder((FolderStruct)this.lstFolderContents.SelectedItem);

                propWindow.Show();
            }
        }

        private void FolderSelected(object sender, EventArgs e)
        {
            BundleBrowser brows = sender as BundleBrowser;

            this.lstFolderContents.ItemsSource = ((IdstringItem)cmbPackages.SelectedItem).UnHashed == "(Show All)" ? brows.SelectedFolder.ChildItems() : brows.SelectedFolder.ChildItems(((IdstringItem)cmbPackages.SelectedItem).UnHashed);
        }

        private void ChangeFolder(object sender, EventArgs e)
        {
            if (sender is TreeView)
            {
                if ((sender as TreeView).SelectedItem == null)
                    return;

                this.brdExplorer.Path = ((FolderStruct)(((TreeViewItem)((TreeView)sender).SelectedItem).Tag)).Path;
            }
        }

        private TreeViewItem AssetsTree;

        private BreadcrumbItem AssetsBread;

        private void WorkingDirectorySet(object sender, EventArgs e)
        {
            prgDialog.lblDesc.Content = "Finishing Processes";

            BundleBrowser sentBrowser = sender as BundleBrowser;

            AssetsTree = new TreeViewItem()
            {
                Header = "assets",
                Tag = sentBrowser.Root,
                HeaderTemplate = (DataTemplate)this.mainFolderTree.Resources["FolderTreeItem"]
            };
            AssetsTree.ExpandSubtree();
            sentBrowser.Root.AddToTree(AssetsTree);
            
            this.mainFolderTree.Items.Add(AssetsTree);

            AssetsBread = new BreadcrumbItem
            {
                Header = "assets",
                Image = (ImageSource)App.Current.FindResource("Icon_FolderClosed"),
                Tag = sentBrowser.Root
            };
            
            sentBrowser.Root.AddToBreadcrumb(AssetsBread);

            this.brdExplorer.IsEnabled = true;
            this.brdExplorer.Root = AssetsBread;

            List<IdstringItem> packages = sentBrowser.PackageIDToName(sentBrowser.PackageHeaders.Keys.ToList());
            packages.Sort(delegate(IdstringItem item1, IdstringItem item2)
            {
                return item1.UnHashed.CompareTo(item2.UnHashed);
            });
            this.cmbPackages.ItemsSource = packages;
            this.cmbPackages.SelectedIndex = 0;

            prgDialog.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "BLB Database|*.blb";
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = true;
            bool select = (bool)dlg.ShowDialog();
            if (select)
            {
                this.LoadDatabase(dlg.FileName);
            }
        }

        public void LoadDatabase(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("[Browser ERROR] Bundle database does not exist!");
                return;
            }
            this.Title = path;
            this.brdExplorer.Root = null;
            this.brdExplorer.Path = "";
            this.mainFolderTree.Items.Clear();
            this.lstFolderContents.ItemsSource = null;
            Browser = new BundleBrowser();
            Browser.OnWorkingDirectoryUpdated += this.WorkingDirectorySet;
            Browser.OnFolderSelected += this.FolderSelected;
            prgDialog = new ProgressDialog();
            prgDialog.Owner = this;
            prgDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            
            Browser.LoadDatabase(path, (str) => prgDialog.lblDesc.Content = str);
            prgDialog.ShowDialog();
        }

        public Point? start;
        public void tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            while (source is ContentElement)
                source = LogicalTreeHelper.GetParent(source);

            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            var lbi = source as TreeViewItem;

            if (lbi != null)
                this.start = e.GetPosition(null);
        }

        private void tree_MouseMove(object sender, MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            if (this.start == null)
                return;

            Vector diff = (Vector)(this.start - mpos);

            if (sender is ListBox)
            {
                if (((ListBox)sender).SelectedItem == null)
                    return;
            }
            else if (sender is TreeView)
            {
                if (((TreeView)sender).SelectedItem == null)
                    return;
            }
            else
                return;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                Console.WriteLine("Started Drag-Drop Operation");
                Dictionary<string, FileEntry> fileList = new Dictionary<string, FileEntry>();

                if (sender is TreeView)
                {
                    FolderStruct folder = (FolderStruct)((TreeViewItem)((TreeView)sender).SelectedItem).Tag;
                    folder.GetSubFileEntries(fileList, folder.Name);

                }
                else if (sender is ListView)
                {
                    if (((ListBox)sender).SelectedItem != null)
                    {
                        foreach (object selected_item in ((ListBox)sender).SelectedItems)
                        {
                            if (selected_item is FileEntry)
                            {
                                FileEntry file = selected_item as FileEntry;
                                fileList.Add(System.IO.Path.GetFileName(file.Path), file);
                            }
                            else if (selected_item is FolderStruct)
                            {
                                FolderStruct folder = selected_item as FolderStruct;
                                folder.GetSubFileEntries(fileList, folder.Name == "assets" ? "" : folder.Name);
                            }
                        }
                    }
                }

                this.InitiateDragDrop(fileList);
                this.start = null;
            }
        }

        public void InitiateDragDrop(Dictionary<string, FileEntry> fileList)
        {
            VirtualFileDataObject virtualFileDataObject = new VirtualFileDataObject();
            List<VirtualFileDataObject.FileDescriptor> files = new List<VirtualFileDataObject.FileDescriptor>();
            foreach (KeyValuePair<string, FileEntry> entry in fileList)
            {

                VirtualFileDataObject.FileDescriptor fileDescriptor = new VirtualFileDataObject.FileDescriptor()
                {
                    Name = entry.Key,
                    StreamContents = () =>
                    {
                        MemoryStream stream = new MemoryStream();
                        BundleEntryHolder maxBundleEntry = entry.Value.MaxBundleEntry();
                        Console.WriteLine("Extracted {0} from package: {1}", entry.Key, maxBundleEntry.Package);
                        entry.Value.FileEntryWriteStream(stream, maxBundleEntry);
                        return stream;
                    }
                };
                files.Add(fileDescriptor);
            }

            virtualFileDataObject.SetData(files);

            var effect = VirtualFileDataObject.DoDragDrop(this.mainFolderTree, virtualFileDataObject, DragDropEffects.Copy);
            if (effect == DragDropEffects.None)
            {
                virtualFileDataObject = null;
            }
            Console.WriteLine("Finished Drag-Drop operation setup");
        }

        private void OnPreviewLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            while (source is ContentElement)
                source = LogicalTreeHelper.GetParent(source);

            while (source != null && !(source is GridViewRowPresenter))
                source = VisualTreeHelper.GetParent(source);

            if (source == null)
                return;

            object content = ((GridViewRowPresenter)source).Content;

            if (content != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.start = e.GetPosition(null);

                if (((ListView)sender).SelectedItems.Contains(content))
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        this.start = e.GetPosition(null);
                        e.Handled = true;
                    }

                    if (e.ClickCount >= 2)
                    {
                        if (content is FolderStruct)
                            this.brdExplorer.Path = (content as FolderStruct).Path;
                        else if (content is FileEntry)
                        {
                            if (((IdstringItem)cmbPackages.SelectedItem).UnHashed == "(Show All)")
                                ((FileEntry)content).OpenViewer();
                            else
                                ((FileEntry)content).OpenViewer(((FileEntry)content).bundleEntries.Find(entry => entry.PackageName.Equals(((IdstringItem)cmbPackages.SelectedItem).UnHashed)));
                        }
                    }
                }
            }
        }

        void listViewItem_Selected(object sender, RoutedEventArgs e)
        {
            this.lstFolderContents.ReleaseMouseCapture();
            
        }
        private void listViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem listViewItem = sender as ListViewItem;
            if (listViewItem.IsSelected == true)
            {
                // Unselecting the item in the Preview event
                // "tricks" the ListView into selecting it again
                listViewItem.IsSelected = false;
            }
        }

        private void lstFolderContents_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.start = null;
        }

        private void brdExplorer_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            BreadcrumbBar brd = sender as BreadcrumbBar;
            this.Browser.SelectedFolder = (FolderStruct)brd.SelectedBreadcrumb.Tag;
        }

        private void lstFolderContents_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                List<string> path = this.brdExplorer.Path.Split(char.Parse(@"\")).ToList();

                if (path.Count > 1)
                    path.RemoveAt(path.Count - 1);

                string new_path = "";
                foreach (string path_part in path)
                {
                    new_path += new_path == "" ? path_part : @"\" + path_part;
                }

                this.brdExplorer.Path = new_path;
            }
        }

        private void lstFolderContents_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                foreach (object select in (sender as ListView).SelectedItems)
                {
                    if (select is FolderStruct)
                        this.brdExplorer.Path = (select as FolderStruct).Path;
                    else if (select is FileEntry)
                    {
                        if (((string)cmbPackages.SelectedItem) == "(Show All)")
                            ((FileEntry)select).OpenViewer();
                        else
                            ((FileEntry)select).OpenViewer(((FileEntry)select).bundleEntries.Find(entry => entry.PackageName.Equals((string)cmbPackages.SelectedItem)));
                    }
                }
            }
        }

        private void ShowTreeItemsOfPackage(string package, dynamic item = null)
        {
            foreach (var treeItem in item.Items)
            {
                if (((FolderStruct)treeItem.Tag).ContainsAnyBundleEntries(package))
                {
                    if (treeItem is TreeViewItem)
                        treeItem.IsExpanded = false;

                    treeItem.Visibility = Visibility.Visible;
                    this.ShowTreeItemsOfPackage(package, treeItem);
                }
                else
                {
                    if (treeItem is TreeViewItem)
                    {
                        treeItem.IsExpanded = false;
                        treeItem.Visibility = Visibility.Collapsed;
                    }
                    else
                        treeItem.IsButtonVisible = false;
                }
            }
        }

        private void UnhideAllTreeItems(dynamic item)
        {
            foreach (var treeItem in item.Items)
            {
                if (treeItem is TreeViewItem)
                    treeItem.IsExpanded = false;
                else
                    treeItem.IsButtonVisible = true;

                treeItem.Visibility = Visibility.Visible;
                this.UnhideAllTreeItems(treeItem);
            }
        }

        private void cmbPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //I'm not a fan of this hard-coded shit
            //But I'm not going to do anything about it right now
            if (((IdstringItem)e.AddedItems[0]).UnHashed != "(Show All)")
            {
                this.ShowTreeItemsOfPackage(((IdstringItem)e.AddedItems[0]).UnHashed, AssetsTree);
                //this.ShowTreeItemsOfPackage(((IdstringItem)e.AddedItems[0]).UnHashed, AssetsBread);
            }
            else
            {
                this.UnhideAllTreeItems(AssetsTree);
                //this.UnhideAllTreeItems(AssetsBread);
            }
            AssetsTree.IsSelected = true;
            this.brdExplorer.Path = "";
            this.Browser.SelectedFolder = AssetsTree.Tag as FolderStruct;
        }

        private void btnInspect_Click(object sender, RoutedEventArgs e)
        {
            string ID = ((IdstringItem)this.cmbPackages.SelectedItem).Hashed;
                
            PackageInspector inspector = new PackageInspector(ID, this.Browser.PackageHeaders[ID]);
            inspector.Show();
        }
    }

    [ValueConversion(typeof(IdstringItem), typeof(bool))]
    public class PackageCustomConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            IdstringItem packText = (IdstringItem)value;
            return packText.UnHashed != "(Show All)" && !String.IsNullOrWhiteSpace(packText.UnHashed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
