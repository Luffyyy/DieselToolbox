using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DieselToolbox
{
    /// <summary>
    /// Interaction logic for PackageInspector.xaml
    /// </summary>
    public partial class PackageInspector : Window
    {
        private BundleHeader _bundle;

        public BundleHeader Bundle
        {
            get
            {
                return _bundle;
            }
            set
            {
                _bundle = value;
                this.RefreshBundle();
            }
        }

        public PackageInspector(string hashedName, BundleHeader bundle)
        {
            InitializeComponent();
            string unhashed = hashedName;
            if (!unhashed.StartsWith("all_"))
                unhashed = GeneralUtils.UnHashString(unhashed, StaticData.Hashes);

            this.Title = String.Format("Package Inspector: {0} ({1})", unhashed, hashedName);
            this.Bundle = bundle;
        }

        public void RefreshBundle()
        {
            this.treePackageInfo.Items.Clear();

            if (this.Bundle.References.Count > 0)
            {
                TreeViewItem entries = new TreeViewItem
                {
                    Header = "Entries",
                };

                foreach (BundleEntry entry in this.Bundle.Entries)
                {
                    DatabaseEntry dbEntry = StaticData.BundleDB.EntryFromID(entry.Id);
                    string path;
                    
                    if (dbEntry.Language == 0)
                        path = String.Format("{0}.{1}", StaticData.Hashes.GetPath(dbEntry.Path), StaticData.Hashes.GetExtension(dbEntry.Extension));
                    else
                        path = String.Format("{0}.{1}.{2}", StaticData.Hashes.GetPath(dbEntry.Path), StaticData.Hashes.GetAny(StaticData.BundleDB.LanguageFromID(dbEntry.Language).Hash), StaticData.Hashes.GetExtension(dbEntry.Extension));

                    entries.Items.Add(new TreeViewItem
                        {
                            Header = path,
                            Tag = entry
                        });
                }

                this.treePackageInfo.Items.Add(entries);
                
            }

            if (this.Bundle.References.Count > 0)
            {
                TreeViewItem refEntries = new TreeViewItem
                {
                    Header = "References",
                };

                foreach (ReferenceEntry entry in this.Bundle.References)
                {
                    refEntries.Items.Add(new TreeViewItem
                    {
                        Header = String.Format("{0}.{1}", StaticData.Hashes.GetPath(entry.Path), StaticData.Hashes.GetExtension(entry.Extension)),
                        Tag = entry
                    });
                }

                this.treePackageInfo.Items.Add(refEntries);
            }
        }

        private void treePackageInfo_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
                return;

            TreeViewItem item = e.NewValue as TreeViewItem;

            if (item.Tag is ReferenceEntry)
            {
                ReferenceEntry entry = (ReferenceEntry)item.Tag;

                this.txtPath.Text = StaticData.Hashes.GetPath(entry.Path);
                this.txtType.Text = StaticData.Hashes.GetExtension(entry.Extension);

                this.txtPath.IsEnabled = true;
                this.txtLang.IsEnabled = false;
                this.txtType.IsEnabled = true;
                this.txtAddress.IsEnabled = false;
                this.txtID.IsEnabled = false;
                this.txtLangID.IsEnabled = false;
                this.txtLength.IsEnabled = false;
            }
            else if (item.Tag is BundleEntry)
            {
                this.txtPath.IsEnabled = true;
                this.txtLang.IsEnabled = true;
                this.txtType.IsEnabled = true;
                this.txtAddress.IsEnabled = true;
                this.txtID.IsEnabled = true;
                this.txtLangID.IsEnabled = true;
                this.txtLength.IsEnabled = true;

                BundleEntry entry = (BundleEntry)item.Tag;
                DatabaseEntry dbEntry = StaticData.BundleDB.EntryFromID(entry.Id);

                this.txtPath.Text = StaticData.Hashes.GetPath(dbEntry.Path);
                this.txtLang.Text = dbEntry.Language == 0 ? "" : StaticData.Hashes.GetAny(StaticData.BundleDB.LanguageFromID(dbEntry.Language).Hash);
                this.txtType.Text = StaticData.Hashes.GetExtension(dbEntry.Extension);

                this.txtAddress.Text = entry.Address.ToString();
                this.txtID.Text = entry.Id.ToString();
                this.txtLangID.Text = dbEntry.Language.ToString();
                this.txtLength.Text = entry.Length.ToString();
            }
            else
            {
                this.txtPath.Text = "";
                this.txtPath.IsEnabled = false;

                this.txtLang.Text = "";
                this.txtLang.IsEnabled = false;

                this.txtLangID.Text = "";
                this.txtLangID.IsEnabled = false;
                
                this.txtType.Text = "";
                this.txtType.IsEnabled = false;
                this.txtAddress.Text = "";
                this.txtAddress.IsEnabled = false;
                this.txtID.Text = "";
                this.txtID.IsEnabled = false;
                this.txtLength.Text = "";
                this.txtLength.IsEnabled = false;
            }
        }
    }
}
