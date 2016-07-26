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
        private string _size;
        private string _fullpath;
        private BundleFileEntry _max_entry = null;
        private dynamic _temp_exported;

        public Idstring _path;

        public Idstring _language;

        public Idstring _extension;

        public string Path
        {
            get
            {
                if (_fullpath == null)
                {
                    _fullpath = this._path.ToString();

                    if (this._language != null)
                    {
                        _fullpath += "." + this._language.ToString();
                    }

                    _fullpath += "." + this._extension.ToString();
                }

                return _fullpath;
            }
        }

        public string Name
        {
            get
            {

                return System.IO.Path.GetFileName(this.Path);
            }
        }

        public List<BundleFileEntry> BundleEntries = new List<BundleFileEntry>();

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
                if (_size == null)
                {
                    int currentSize = 0;
                    foreach (BundleFileEntry be in this.BundleEntries)
                        currentSize += be.Length;

                    currentSize = currentSize / Math.Max(this.BundleEntries.Count, 1);

                    if (currentSize < 1024)
                        _size = currentSize.ToString() + " B";
                    else
                        _size = string.Format("{0:n0}", currentSize / 1024) + " KB";
                }

                return _size;
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

        public BundleFileEntry TempEntry { get; set; }

        public FileEntry() { }

        public FileEntry(DatabaseEntry dbEntry, BundleDatabase db) { this.SetDBEntry(dbEntry, db); }

        public void AddBundleEntry(BundleFileEntry entry)
        {
            this.BundleEntries.Add(entry);
            _max_entry = null;
        }

        public dynamic FileData(BundleFileEntry be = null, dynamic exporter = null)
        {
            if (exporter == null)
            {
                return this.FileBytes(be);
            }
            else
            {
                MemoryStream stream = this.FileStream(be);

                if (exporter != null && !((exporter.GetType().GetMethod("export") != null) || exporter.export != null))
                    throw new InvalidDataException("Inputted exporter does not contain a method definition for export!");

                if (stream == null)
                    return null;

                return exporter.export(stream);
            }
        }

        private byte[] FileEntryBytes(BundleFileEntry entry)
        {
            if (entry == null)
                return null;

            string bundle_path;
            if (!File.Exists(bundle_path = System.IO.Path.Combine(this.ParentBrowser.WorkingDirectory, entry.Parent.Name.HashedString + ".bundle")))
            {
                Console.WriteLine("Bundle: {0}, does not exist", bundle_path);
                return null;
            }

            byte[] data = null;
            try
            {
                using (FileStream fs = new FileStream(bundle_path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (entry.Length != 0)
                        {
                            fs.Position = entry.Address;
                            data = br.ReadBytes((int)(entry.Length == -1 ? fs.Length - fs.Position : entry.Length));
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
            }

            return data;
        }

        public MemoryStream FileStream(BundleFileEntry entry = null)
        {
            entry = entry ?? this.MaxBundleEntry();

            byte[] bytes = this.FileEntryBytes(entry);
            if (bytes == null)
                return null;

            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return stream;
        }

        public byte[] FileBytes(BundleFileEntry entry = null)
        {
            entry = entry ?? this.MaxBundleEntry();

            return this.FileEntryBytes(entry);
        }

        public void SetDBEntry(DatabaseEntry ne, BundleDatabase db)
        {
            DBEntry = ne;
            if (!ne.Path.HasUnHashed)
                Console.WriteLine("No unhashed string for " + ne.Path.Hashed);

            General.GetFilepath(ne, out _path, out _language, out _extension, db);
        }

        public BundleFileEntry MaxBundleEntry()
        {
            if (this.BundleEntries.Count == 0)
                return null;

            if (_max_entry == null)
            {
                _max_entry = null;
                foreach (BundleFileEntry entry in this.BundleEntries)
                {
                    if (_max_entry == null)
                    {
                        _max_entry = entry;
                        continue;
                    }

                    if (entry.Length > _max_entry.Length)
                        _max_entry = entry;
                }

            }

            return _max_entry;
        }
    }
}
