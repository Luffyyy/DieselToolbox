using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;

namespace DieselToolbox
{
    public class PackageInspector : Form
    {
        TreeView treeEntries;
        TreeItem treeBundleEntries;
        TreeItem treeReferenceEntries;

        TextBox txtPath;
        TextBox txtLang;
        TextBox txtType;
        TextBox txtID;
        TextBox txtLangID;
        TextBox txtAddress;
        TextBox txtLength;

        private BundleDatabase lookup_db;

        public PackageInspector()
        {
            XamlReader.Load(this);
            TreeItemCollection treeItems = new TreeItemCollection();
            treeItems.Add(treeBundleEntries = new TreeItem { Text="File Entries" });
            treeItems.Add(treeReferenceEntries = new TreeItem { Text= "Footer Entries" });

            this.treeEntries.DataStore = treeItems;
        }

        public PackageInspector(PackageHeader header, BundleDatabase db) : this()
        {
            this.BuildEntries(header, db);
        }

        public void BuildEntries(PackageHeader header, BundleDatabase db)
        {
            this.treeBundleEntries.Children.Clear();
            this.treeReferenceEntries.Children.Clear();
            this.ClearEntryFields();
            this.Title = String.Format("{0} - Bundle Inspector", header.Name.ToString());

            lookup_db = db;
            foreach (BundleFileEntry entry in header.Entries)
            {
                DatabaseEntry db_ent = db.EntryFromID(entry.ID);

                string path = db_ent != null ? General.GetFullFilepath(db_ent, db) : entry.ID.ToString();

                this.treeBundleEntries.Children.Add(
                    new TreeItem { Text = path, Tag = entry }   
                );
            }

            foreach(BundleFooterEntry entry in header.References)
            {
                this.treeReferenceEntries.Children.Add(
                    new TreeItem { Text = String.Format("{0}.{1}", entry.Path.ToString(), entry.Extension.ToString()), Tag = entry }
                );
            }
        }

        public void treeEntries_SelectionChanged(object sender, EventArgs e)
        {
            TreeView tree = sender as TreeView;
            object tag;
            if ((tag = (tree.SelectedItem as TreeItem)?.Tag) != null)
            {
                if (tag is BundleFileEntry)
                    this.SetInspectedEntry((BundleFileEntry)tag);
                else if (tag is BundleFooterEntry)
                    this.SetInspectedEntry((BundleFooterEntry)tag);
            }
        }

        public void ClearEntryFields()
        {
            this.txtPath.Text = "";
            this.txtLang.Text = "";
            this.txtType.Text = "";
            this.txtID.Text = "";
            this.txtLangID.Text = "";
            this.txtAddress.Text = "";
            this.txtLength.Text = "";
        }

        public void SetInspectedEntry(BundleFileEntry entry)
        {
            this.ClearEntryFields();
            DatabaseEntry db_ent = lookup_db.EntryFromID(entry.ID);
            this.txtPath.Text = db_ent?.Path?.ToString();
            this.txtLang.Text = (db_ent?.Language ?? 0) != 0 ? lookup_db.LanguageFromID((uint)db_ent?.Language).Name.ToString() : ""; 
            this.txtType.Text = db_ent?.Extension?.ToString();
            this.txtID.Text = entry.ID.ToString();
            this.txtLangID.Text = (db_ent?.Language ?? 0) != 0 ? db_ent?.Language.ToString() : "";
            this.txtAddress.Text = entry.Address.ToString();
            this.txtLength.Text = entry.Length.ToString();
        }

        public void SetInspectedEntry(BundleFooterEntry entry)
        {
            this.ClearEntryFields();
            this.txtPath.Text = entry.Path.ToString();
            this.txtType.Text = entry.Extension.ToString();
        }
    }
}
