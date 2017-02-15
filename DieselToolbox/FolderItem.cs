using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DieselEngineFormats;
using System.IO;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Threading;
using Eto.Drawing;
using System.Runtime.InteropServices;
using DieselEngineFormats.BNK;
using DieselEngineFormats.Font;
using DieselEngineFormats.Bundle;
using Eto.Forms;
using DieselEngineFormats.Utils;
using DieselEngineFormats.ScriptData;

namespace DieselToolbox
{
    public class FolderItem : IChild, IParent, IViewable
    {
        private SortedDictionary<string, IChild> _children;

        public IParent Parent { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public string Type
        {
            get { return Definitions.FolderTypeName; }
        }

        public Icon Icon
        {
            get
            {
                return Definitions.FolderIcon["closed"];
            }
        }

        public string Size
        {
            get
            {
                return "";
            }

        }

        public SortedDictionary<string, IChild> Children
        {
            get { return this._children; }
            set { this._children = value; }
        }

        public List<object> ChildObjects(Idstring pck = null)
        {
            List<object> objs = new List<object>();
            foreach (KeyValuePair<string, IChild> kvp in this.Children)
            {
                if (kvp.Value is IParent && (pck == null || ((IParent)kvp.Value).ContainsAnyBundleEntries(pck)))
                    objs.Add(kvp.Value);
            }

            foreach (KeyValuePair<string, IChild> kvp in this.Children)
            {
                if (kvp.Value is IChild && (!(kvp.Value is FileEntry) || pck == null || ((FileEntry)kvp.Value).BundleEntries.FindIndex(item => item.Parent.Name.Equals(pck)) != -1) && !(kvp.Value is IParent))
                    objs.Add(kvp.Value);
            }

            return objs;
        }

        uint folderLevel;

        public FolderItem(uint level = 0) { }

        public FolderItem(FileEntry entry, uint level = 0)
        {
            this.folderLevel = level;
            this._children = new SortedDictionary<string, IChild>();
            this.AddFileEntry(entry);
        }

        public FolderItem(Dictionary<uint, FileEntry> ents, uint level = 0)
        {
            this.folderLevel = level;
            this._children = new SortedDictionary<string, IChild>();

            foreach (KeyValuePair<uint, FileEntry> entry in ents)
            {
                this.AddFileEntry(entry.Value);
            }
        }

        public void AddFileEntry(FileEntry entry)
        {
            int[] path_parts = entry._path.UnHashedParts;
            if (path_parts != null && path_parts.Length > (this.folderLevel + 1))
            {
                string initial_folder = HashIndex.LookupString(path_parts[this.folderLevel]);
                if (!this._children.ContainsKey(initial_folder))
                {
                    FolderItem folder = new FolderItem(entry, this.folderLevel + 1)
                    {
                        Parent = this

                    };

                    folder.Path = "assets";
                    for (int i = 0; i <= this.folderLevel; i++)
                    {
                        System.IO.Path.Combine(folder.Path, HashIndex.LookupString(path_parts[i]));
                    }
                    folder.Name = initial_folder;
                    this._children.Add(initial_folder, folder);
                }
                else
                {
                    ((FolderItem)this._children[initial_folder]).AddFileEntry(entry);
                }
            }
            else
            {
                entry.Parent = this;
                this._children.Add(entry.Name, entry);
            }
        }

        public void AddToTree(TreeItem item, Idstring pck = null)
        {
            foreach (KeyValuePair<string, IChild> entry in this._children)
            {
                if (entry.Value is IParent)
                {
                    IParent _entry = entry.Value as IParent;

                    if (pck != null && !_entry.ContainsAnyBundleEntries(pck))
                        continue;

                    TreeItem treeItem = new TreeItem()
                    {
                        Text = entry.Key,
                        Image = this.Icon,
                        Tag = entry.Value
                    };
                    item.Children.Add(treeItem);
                    _entry.AddToTree(treeItem, pck);
                }
            }
        }

        /*public void AddToTree(TreeViewItem item, string package)
        {
            foreach (KeyValuePair<string, FolderStruct> entry in this.folders)
            {
                if (!entry.Value.ContainsAnyBundleEntries() && !entry.Value.ContainsAnyBundleEntries(package))
                    continue;

                TreeViewItem treeItem = new TreeViewItem()
                {
                    Header = entry.Key,
                    Tag = entry.Value,
                    HeaderTemplate = item.HeaderTemplate
                };
                item.Items.Add(treeItem);
                entry.Value.AddToTree(treeItem);
            }
        }*/

        /*public void AddToBreadcrumb(BreadcrumbItem brdItem)
        {
            foreach (KeyValuePair<string, FolderStruct> entry in this.folders)
            {
                if (!entry.Value.ContainsAnyBundleEntries())
                    continue;

                BreadcrumbItem subBrdItem = new BreadcrumbItem()
                {
                    Header = entry.Key,
                    Tag = entry.Value,
                    Image = (ImageSource)App.Current.FindResource("Icon_FolderClosed")
                };
                brdItem.Items.Add(subBrdItem);
                entry.Value.AddToBreadcrumb(subBrdItem);
            }
        }*/

        public bool ContainsAnyBundleEntries(Idstring package = null)
        {
            foreach (KeyValuePair<string, IChild> entry in this._children)
            {
                if (entry.Value is IParent)
                {
                    IParent _entry = entry.Value as IParent;
                    if (_entry.ContainsAnyBundleEntries(package))
                    {
                        return true;
                    }
                }
                else if (entry.Value is FileEntry)
                {
                    FileEntry _entry = entry.Value as FileEntry;
                    if (_entry.BundleEntries.Count != 0 && (package != null ? _entry.BundleEntries.FindIndex(ent => ent.Parent.Name.Equals(package)) != -1 : true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void GetSubFileEntries(Dictionary<string, FileEntry> fileList)
        {
            foreach (KeyValuePair<string, IChild> pairEntry in this._children)
            {
                if (pairEntry.Value is FileEntry)
                {
                    FileEntry _entry = pairEntry.Value as FileEntry;
                    if (_entry.BundleEntries.Count == 0)
                        continue;

                    fileList.Add(_entry.Path, _entry);

                }
                else if (pairEntry.Value is IParent)
                {
                    IParent _entry = pairEntry.Value as IParent;
                    _entry.GetSubFileEntries(fileList);
                }
            }
        }

        public int GetTotalSize()
        {
            Dictionary<string, FileEntry> files = new Dictionary<string, FileEntry>();
            this.GetSubFileEntries(files);

            int totalSize = 0;
            foreach (KeyValuePair<string, FileEntry> pair in files)
            {
                totalSize += pair.Value.MaxBundleEntry().Length;
            }

            return totalSize;
        }
    }
}
