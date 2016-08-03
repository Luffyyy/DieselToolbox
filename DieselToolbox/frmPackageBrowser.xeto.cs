using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using System.IO;
using DieselEngineFormats.Bundle;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace DieselToolbox
{
	public class frmPackageBrowser : Form
	{
		TreeView treeMain;
		GridView grdFolder;
		TableLayout tblMain;
		Splitter spltMain;
		ButtonMenuItem pckList;
        ButtonMenuItem btnInspectPackage;
        ButtonMenuItem btnScripts;

        ProgressDialog prgDialog;
		CheckMenuItem cbtnLocalHashlists;
		CheckMenuItem cbtnExtractFullFileStructure;

		private PackageBrowser Browser;

        private PointF DragStartLocation;
        private bool DraggingDrop = false;

		public frmPackageBrowser ()
		{
			XamlReader.Load(this);
            
			/*spltMain.Panel1 = treeMain = new TreeView { Width = 200 };
			((TreeView)spltMain.Panel1).SelectionChanged += OnTreeFolderSelected;
			((TreeView)spltMain.Panel1).Expanded += OnTreeItemExpanded;
			((TreeView)spltMain.Panel1).Collapsed += OnTreeItemCollapsed;*/
            //spltMain.Panel2 = grdFolder = new GridView { AllowMultipleSelection = true };
            this.KeyDown += (sender, e) => {
                if (e.Key.Equals(Keys.Backspace) && this.Browser != null && this.Browser.SelectedFolder != null && ((IChild)this.Browser.SelectedFolder).Parent != null)
                    this.Browser.SelectedFolder = ((IChild)this.Browser.SelectedFolder).Parent;
            };
            /*grdFolder.MouseUp += ViewDragMouseUp;
            
            grdFolder.MouseMove += ViewMouseMove;

            grdFolder.CellDoubleClick += GrdFolder_CellDoubleClick;*/
            grdFolder.CellClick += ViewDragMouseClick;

            /*treeMain.MouseUp += ViewDragMouseUp;
            treeMain.MouseMove += ViewMouseMove;*/
            //treeMain.MouseDown += ViewDragMouseClick;
            this.Closed += (sender, e) => { Application.Instance.Quit(); };
            treeMain.NodeMouseClick += ViewDragMouseClick;
			List<MenuItem> context_items = new List<MenuItem> ();

			ButtonMenuItem viewItem = new ButtonMenuItem{ Text = "View" };
			viewItem.Click += (object sender, EventArgs e) => {
				FileEntry childItem;
				if ((childItem = grdFolder.SelectedItem as FileEntry) != null)
					Browser.TempFileManager.ViewFile(childItem);
			};
			context_items.Add (viewItem);

			ButtonMenuItem propItem = new ButtonMenuItem{ Text = "Properties" };
			propItem.Click += PropItem_Click;
			context_items.Add (propItem);

			grdFolder.ContextMenu = new ContextMenu(context_items);

			grdFolder.Columns.Add (new GridColumn{
				DataCell = new ImageTextCell{
                    ImageBinding = Binding.Property<IViewable, Image>(r => r.Icon),
					TextBinding = Binding.Property<IViewable, string>(r => r.Name)
				},
				HeaderText = "Name"
			});

			grdFolder.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
                    Binding = Binding.Property<IViewable, string>(r => r.Type)
				},
				HeaderText = "Type"
			});

			grdFolder.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
                    Binding = Binding.Property<IViewable, string>(r => r.Size)
				},
				HeaderText = "Size"
			});
            
            /*grdFolder.KeyDown += (sender, e) => {
                if (e.Key.Equals(Keys.Enter)) {
					Browser.SelectedFolder = (IParent)grdFolder.SelectedItem;
                }
            };*/

            btnInspectPackage.Click += (sender, e) => {
                Console.WriteLine("Inspect Package {0}", this._focused_package?.ToString());
            };

            cbtnLocalHashlists.Checked = StaticData.Settings.Data.StoreLocalHashlists;
            cbtnExtractFullFileStructure.Checked = StaticData.Settings.Data.ExtractFullFileStructure;

			cbtnLocalHashlists.CheckedChanged += (object sender, EventArgs e) => {
				StaticData.Settings.Data.StoreLocalHashlists = cbtnLocalHashlists.Checked;
				StaticData.Settings.Save();
			};
            cbtnExtractFullFileStructure.CheckedChanged += (object sender, EventArgs e) => {
                StaticData.Settings.Data.ExtractFullFileStructure = ((CheckMenuItem)sender).Checked;
                StaticData.Settings.Save();
            };

            this.Closed += (sender, e) =>
            {
                this.Browser.Dispose();
            };

            this.PopulateScripts();
        }

        void ViewMouseMove(object sender, MouseEventArgs e)
        {
            dynamic sent = sender as dynamic;
            PointF diff = (DragStartLocation - e.Location);
            if (e.Buttons == MouseButtons.Primary && sent.SelectedItem != null && DraggingDrop
                && Math.Abs(diff.X) > Definitions.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > Definitions.MinimumVerticalDragDistance)
            {
                e.Handled = true;
                DragDropController dragdrop = new DragDropController
                {
                    OutputFullPaths = StaticData.Settings.Data.ExtractFullFileStructure
                };
                object item;
                if (sent.SelectedItem is TreeItem)
                    item = ((TreeItem)sent.SelectedItem).Tag;
                else
                    item = sent.SelectedItem;

                if (item is IParent)
                    dragdrop.DoDragDrop((IParent)item);
                else if (item is IChild)
                    dragdrop.DoDragDrop((IChild)item);

            }
        }

        void ViewDragMouseClick(object sender, EventArgs e)
        {
            if (Mouse.Buttons == MouseButtons.Primary)
            {
                DragStartLocation = Mouse.Position;
                DraggingDrop = true;
            }
        }

        void ViewDragMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Buttons == MouseButtons.Primary)
                DraggingDrop = false;
        }

        void btnInspectPackage_Click(object sender, EventArgs e)
        {
            if (_focused_package != null)
            {
                PackageHeader package = Browser.PackageHeaders[_focused_package];
                new PackageInspector(package, Browser.BundleDB).Show();
            }
            else
                MessageBox.Show("You must select a package to inspect!", "Further Action Required!", MessageBoxType.Information);
        }

        void PropItem_Click (object sender, EventArgs e)
		{
			IViewable childItem;
			if ((childItem = grdFolder.SelectedItem as IViewable) != null) {
				PropertiesDialog prop = new PropertiesDialog(childItem);
				prop.Show();
			}
		}

		void GrdFolder_CellDoubleClick (object sender, GridViewCellEventArgs e)
		{
			if (e.Item != null) {
				if (e.Item is IParent)
					Browser.SelectedFolder = (IParent)e.Item;
				else if (e.Item is FileEntry)
					Browser.TempFileManager.ViewFile((FileEntry)e.Item);
			}
		}
			

		void OnTreeItemCollapsed (object sender, TreeViewItemEventArgs e)
		{
			((TreeItem)e.Item).Image = Definitions.FolderIcon ["closed"];
            treeMain.RefreshItem((TreeItem)e.Item);
        }

		void OnTreeItemExpanded (object sender, TreeViewItemEventArgs e)
		{
			((TreeItem)e.Item).Image = Definitions.FolderIcon ["open"];
            treeMain.RefreshItem((TreeItem)e.Item);
        }

        public void LoadDatabaseClicked(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.MultiSelect = false;
			dlg.Filters.Add ( new FileDialogFilter("BLB Database", ".blb"));
			dlg.CheckFileExists = true;
			DialogResult select = dlg.ShowDialog(this);
			if (select == DialogResult.Ok)
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
			this.Title = String.Format("{0} - Package Browser", path);
			treeMain.DataStore = null;
			treeMain.RefreshData ();
            _focused_package = null;
            pckList.Items.Clear();

            this.grdFolder.DataStore = null;

			Browser = new PackageBrowser();
			Browser.OnWorkingDirectoryUpdated += this.WorkingDirectorySet;
			Browser.OnFolderSelected += this.FolderSelected;
			prgDialog = new ProgressDialog();

    		Browser.LoadDatabase(path, (str) => prgDialog.lblProgressString.Text = str);

			prgDialog.ShowModalAsync(this);
		}

        public void PopulateScripts()
        {
            foreach (KeyValuePair<string, dynamic> script in ScriptActions.Scripts)
            {
                ButtonMenuItem btn = new ButtonMenuItem { Text = script.Value.title, ID = script.Key, Tag = script.Value };
                btn.Click += ScriptBTN_Click;

                btnScripts.Items.Add(btn);
            }
        }

        public void ScriptBTN_Click(object sender, EventArgs e)
        {
            if (this.Browser == null)
                return;

            ButtonMenuItem btn = sender as ButtonMenuItem;
            this.ExecuteScript(btn.ID, btn.Tag as dynamic);
        }

        public void ExecuteScript(string key, dynamic script)
        {
            Thread thr = new Thread(() =>
            {
                try
                {
                    script.execute(this.Browser);
                    Console.WriteLine("Finished script {0}", key);
                }
                catch(Exception exc)
                {
                    Console.WriteLine("Error occurred in script with key '{0}'", key);
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }
            })
            { IsBackground = true };
            
            thr.Start();
        }

		TreeItem AssetsTree;

		private void WorkingDirectorySet(object sender, EventArgs e)
		{
			prgDialog.lblProgressString.Text = "Finishing Processes";

			this.SetupTreeItems ();

            SortedDictionary<Idstring, PackageHeader> packages = Browser.PackageHeaders;
			AddPackagesToMenu (packages);

			prgDialog.Close ();
		}

		string show_all_radio = "(Show All)";
		private void AddPackagesToMenu(SortedDictionary<Idstring, PackageHeader> pckIds)
		{
			RadioMenuItem prevItem = new RadioMenuItem{
				Tag = new Idstring(show_all_radio, true),
				Text = show_all_radio
			};
			prevItem.CheckedChanged += OnPackageChanged;
			pckList.Items.Add (prevItem);
			foreach (KeyValuePair<Idstring, PackageHeader> ids in pckIds) {
				RadioMenuItem item = new RadioMenuItem(prevItem) {
					Tag = ids.Value,
					Text = ids.Key.ToString(),
				};
				item.CheckedChanged += OnPackageChanged;
				pckList.Items.Add (item);
				prevItem = item;
			}
		}

		private Idstring _focused_package = null;
		private void OnPackageChanged(object sender, EventArgs e)
		{
			RadioMenuItem item;
			if ((item = (RadioMenuItem)sender).Checked) {
				if ((_focused_package = ((PackageHeader)item.Tag).Name).UnHashed == show_all_radio)
					_focused_package = null;

				this.SetupTreeItems ();

				this.Browser.SelectedFolder = AssetsTree.Tag as IParent;
			}
		}

		private void SetupTreeItems()
		{
			AssetsTree = new TreeItem()
			{
				Text = "assets",
				Image = Definitions.FolderIcon["open"],
				Tag = Browser.Root
			};
            
            //AssetsTree.Expanded = true;
			AssetsTree.Expanded = true;

			Browser.Root.AddToTree(AssetsTree, _focused_package);

			TreeItemCollection treeItemColl = new TreeItemCollection ();
            treeItemColl.Add (AssetsTree);

			this.treeMain.DataStore = treeItemColl;
		}

		private void FolderSelected(object sender, EventArgs e)
		{
			PackageBrowser brows = sender as PackageBrowser;

            this.grdFolder.DataStore = (brows.SelectedFolder).ChildObjects(_focused_package);
		}

		void OnTreeFolderSelected(object sender, EventArgs e)
		{
			TreeItem ctrl;
			if ((ctrl = ((TreeItem)(((TreeView)sender).SelectedItem))) != null)
				this.Browser.SelectedFolder = (IParent)(ctrl.Tag);
		}
	}
}