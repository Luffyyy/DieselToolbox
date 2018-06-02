using DieselEngineFormats.Bundle;
using Eto.Drawing;
using Eto.Forms;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DieselEngineFormats.Utils;

namespace DieselToolbox
{
    public class FileEntry : IViewable, IChild
    {
        private uint _size;
        private string _name, _fullpath;
        private PackageFileEntry _max_entry = null;

        public Idstring _path;

        public Idstring _language;

        public Idstring _extension;

        public string Path
        {
            get
            {
                if (this._fullpath == null)
                {
                    this._fullpath = this._path.ToString();

                    if (this._language != null)
                    {
                        this._fullpath += "." + this._language.ToString();
                    }

                    this._fullpath += "." + this._extension.ToString();
                }
                return this._fullpath;
            }
        }

        public string Name
        {
            get
            {
                if (this._name == null)
                {
                    this._name = System.IO.Path.GetFileName(this._path.ToString());
                    if (this._language != null)
                    {
                        this._name += "." + this._language.ToString();
                    }

                    this._name += "." + this._extension.ToString();
                }

                return this._name;
            }
        }

        public List<PackageFileEntry> BundleEntries = new List<PackageFileEntry>();

        public DatabaseEntry DBEntry { get; set; }

        public IParent Parent { get; set; }

        public PackageBrowser ParentBrowser { get; set; }

        public uint avgSize { get; set; }

        public bool IsSelected { get; set; }

        public Icon Icon
        {
            get
            {
                Icon bmp;

                if (Definitions.FileIcons.ContainsKey(this._extension.ToString()))
                    bmp = Definitions.FileIcons[this._extension.ToString()];
                else if ((Definitions.RawTextExtension.Contains(this._extension.ToString()) || Definitions.ScriptDataExtensions.Contains(this._extension.ToString())))
                    bmp = Definitions.FileIcons["text"];
                else
                    bmp = Definitions.FileIcons["default"];

                return bmp;
            }
        }

        public string Size
        {
            get
            {
                if (this._size == 0 && this.BundleEntries.Count > 0)
                {
                    this._size = 0;
                    foreach (PackageFileEntry be in this.BundleEntries)
                        this._size += (uint)be.Length;

                    this._size = (uint)(this._size / Math.Max(this.BundleEntries.Count, 1));
                }
                string str_size;
                if (this._size < 1024)
                    str_size = this._size.ToString() + " B";
                else
                    str_size = string.Format("{0:n0}", this._size / 1024) + " KB";

                return str_size;
            }
        }

        public string Type
        {
            get
            {
                return this._extension.ToString();
            }
        }

        public string TempPath { get; set; }

        public PackageFileEntry TempEntry { get; set; }

        public FileEntry() { }

        public FileEntry(DatabaseEntry dbEntry, PackageDatabase db) { this.SetDBEntry(dbEntry, db); }

        public void AddBundleEntry(PackageFileEntry entry)
        {
            this.BundleEntries.Add(entry);
            this._max_entry = null;
        }

        public object FileData(PackageFileEntry be = null, FormatConverter exporter = null)
        {
            if (exporter == null)
                return this.FileStream(be);
            else
            {
                MemoryStream stream = this.FileStream(be);
                return stream == null ? null : exporter.Export(this.FileStream(be));
            }
        }

        private byte[] FileEntryBytes(PackageFileEntry entry)
        {
            if (entry == null)
                return null;

            string bundle_path;
            if (!File.Exists(bundle_path = System.IO.Path.Combine(this.ParentBrowser.WorkingDirectory, entry.Parent.Name.HashedString + ".bundle")))
            {
                Console.WriteLine("Bundle: {0}, does not exist", bundle_path);
                return null;
            }

            try
            {
                using (FileStream fs = new FileStream(bundle_path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (entry.Length != 0)
                        {
                            fs.Position = entry.Address;
                            return br.ReadBytes((int)(entry.Length == -1 ? fs.Length - fs.Position : entry.Length));
                        }
                        else
                            return new byte[0];
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
            }

            return null;
        }

        public MemoryStream FileStream(PackageFileEntry entry = null)
        {
            entry = entry ?? this.MaxBundleEntry();

            byte[] bytes = this.FileEntryBytes(entry);
            if (bytes == null)
                return null;

            MemoryStream stream = new MemoryStream(bytes);
            stream.Position = 0;
            return stream;
        }

        public byte[] FileBytes(PackageFileEntry entry = null)
        {
            entry = entry ?? this.MaxBundleEntry();

            return this.FileEntryBytes(entry);
        }

        public void SetDBEntry(DatabaseEntry ne, PackageDatabase db)
        {
            this.DBEntry = ne;
            //if (!ne.Path.HasUnHashed)
            //    Console.WriteLine("No unhashed string for " + ne.Path.Hashed);

            General.GetFilepath(ne, out this._path, out this._language, out this._extension, db);
        }

        public PackageFileEntry MaxBundleEntry()
        {
            if (this.BundleEntries.Count == 0)
                return null;

            if (this._max_entry == null)
            {
                this._max_entry = null;
                foreach (PackageFileEntry entry in this.BundleEntries)
                {
                    if (this._max_entry == null)
                    {
                        this._max_entry = entry;
                        continue;
                    }

                    if (entry.Length > this._max_entry.Length)
                        this._max_entry = entry;
                }

            }

            return this._max_entry;
        }
    }
}
