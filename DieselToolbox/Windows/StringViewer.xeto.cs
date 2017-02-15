#pragma warning disable CS0649

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using DieselEngineFormats;
using DieselEngineFormats.Bundle;
using System.IO;

namespace DieselToolbox
{
	public class StringViewer : Form
	{
		GridView grdStrings;
		TextBox txtID;
		TextBox txtText;

		private StringsFile _strings;

		public StringsFile Strings
		{
			get { return this._strings; }
			set
			{
                this._strings = value;
				this.grdStrings.DataStore = value.LocalizationStrings;
			}
		}

		public StringViewer ()
		{
			XamlReader.Load (this);
            //this.InitScripts ();

            this.grdStrings.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{ Binding = Binding.Delegate<StringEntry, string>(r => (r.ID.HasUnHashed ? "" : "0x") + r.ID.ToString())
				},
				HeaderText = "ID"
			});

            this.grdStrings.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{ Binding = Binding.Property<StringEntry, string>(r => r.Text)
				},
				HeaderText = "Text"
			});
		}

		public StringViewer(StringsFile strFile) : this()
		{
			this.Strings = strFile;
		}

        public StringViewer(string file) : this()
        {
            this.Strings = new StringsFile(file);
        }

        public StringViewer(Stream str) :this()
        {
            this.Strings = new StringsFile(str);
        }

		/*public void InitScripts()
		{
			if (ScriptDefs.Exports.ContainsKey("strings"))
			{
				List<dynamic> export = ScriptDefs.Exports["strings"];

				foreach (dynamic dyn in export)
				{
					MenuItem item = new MenuItem
					{
						Header = dyn.label,
						Tag = dyn,
					};
					item.Click += exportClick;
					this.menuExport.Items.Add(item);
				}
			}

			if (ScriptDefs.Imports.ContainsKey("strings"))
			{
				List<dynamic> import = ScriptDefs.Imports["strings"];

				foreach (dynamic dyn in import)
				{
					MenuItem item = new MenuItem
					{
						Header = dyn.label,
						Tag = dyn,
					};
					item.Click += importClick;
					this.menuImport.Items.Add(item);
				}
			}
		}

		private void exportClick(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;

			dynamic dyn = item.Tag as dynamic;

			SaveFileDialog dlg = new SaveFileDialog { 
				ValidateNames = true,
				Filter = dyn.filter
			};
			bool res = (bool)dlg.ShowDialog();

			if (res)
			{
				dyn.execute(dlg.FileName, this.Strings);
			}

		}

		private void importClick(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;

			dynamic dyn = item.Tag as dynamic;

			if (dyn.needHash && this.Hashes == null)
			{
				MessageBox.Show("A HashList is required to import this type, Please select a Bundle Database", "Extra Action needed!", MessageBoxButton.OK);

				OpenFileDialog dbdlg = new OpenFileDialog {
					Multiselect = false,
					Filter = "Bundle Database|*.blb",
					CheckPathExists = true,
					CheckFileExists = true
				};

				bool dbres = (bool)dbdlg.ShowDialog();

				if (dbres)
				{
					this.UpdateHashes(dbdlg.FileName);
				}
			}

			OpenFileDialog dlg = new OpenFileDialog {
				Multiselect = false,
				Filter = dyn.filter,
				CheckPathExists = true,
				CheckFileExists = true
			};

			bool res = (bool)dlg.ShowDialog();

			if (res)
			{
				this.Strings = (StringsFile)dyn.execute(dlg.FileName, this.Hashes);                
			}
		}*/

		private void lstStrings_SelectionChanged(object sender, EventArgs e)
		{
			StringEntry strEntry = this.grdStrings.SelectedItem as StringEntry;

			if (strEntry != null)
			{
				this.txtID.Text = (strEntry.ID.HasUnHashed ? "" : "0x") + strEntry.ID.ToString();
				this.txtText.Text = strEntry.Text;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			StringEntry strEntry = this.grdStrings.SelectedItem as StringEntry;

			if (strEntry != null)
			{
				strEntry.Text = this.txtText.Text;

				this.grdStrings.ReloadData(this.grdStrings.SelectedRows);

				if (!this.Strings.ModifiedStrings.Contains(strEntry))
					this.Strings.ModifiedStrings.Add(strEntry);
			}
		}
	}
}