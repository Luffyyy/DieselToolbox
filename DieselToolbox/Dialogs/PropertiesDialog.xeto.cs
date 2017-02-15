using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using DieselEngineFormats.Bundle;

namespace DieselToolbox
{
	public class PropertiesDialog : Form
	{
		public TableLayout tblDetails;
        public TextBox txtFullPath;
        public TextBox txtName;
		public TextBox txtType;
		public TextBox txtAvgSize;
		public TableRow tblRowEntries;
		public TabPage tabEntries;
		public GridView grdEntries;

		public PropertiesDialog ()
		{
			XamlReader.Load (this);

			this.grdEntries.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
					Binding = Binding.Property<PackageFileEntry, Idstring>(r => r.PackageName).Convert(val => val?.ToString()),
					
				},
				HeaderText = "Package"
			});

			this.grdEntries.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
					Binding = Binding.Property<PackageFileEntry, Idstring>(r => r.PackageName).Convert(val => val?.HashedString)
				},
				HeaderText = "Package (Hashed)"
			});

			this.grdEntries.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
					Binding = Binding.Property<PackageFileEntry, uint>(r => r.Address).Convert(val => val.ToString())
				},
				HeaderText = "Addess"
			});

			this.grdEntries.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
					Binding = Binding.Property<PackageFileEntry, int>(r => r.Length).Convert(val => val.ToString())
				},
				HeaderText = "Length"
			});

			this.grdEntries.Columns.Add (new GridColumn{
				DataCell = new TextBoxCell{
					Binding = Binding.Property<PackageFileEntry, string>(r => r.FileSize)
				},
				HeaderText = "Size"
			});
		}

		public PropertiesDialog (IViewable item) : this()
		{
			this.DisplayInformation(item);
		}

		public void DisplayInformation(IViewable item)
		{
			this.Title = String.Format ("{0}, Properties", item.Name);

            this.txtFullPath.Text = item.Path;

            this.txtName.Text = item.Name;
			this.txtType.Text = item.Type;

			this.txtAvgSize.Text = item.Size;

			if (item is FileEntry)
				this.grdEntries.DataStore = ((FileEntry)item).BundleEntries;
			else
				this.tabEntries.Visible = false;
		}
	}
}