using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;

namespace DieselToolbox
{
    public class PackageBrowser : IDisposable
    {
        public EventHandler OnWorkingDirectoryUpdated;

        public EventHandler OnFolderSelected;

        public SortedDictionary<Idstring, PackageHeader> PackageHeaders;

		UITimer LoadTimer { get; set; }

        public IParent Root { get; set; }

        public Dictionary<Tuple<Idstring, Idstring, Idstring>, FileEntry> RawFiles = new Dictionary<Tuple<Idstring, Idstring, Idstring>, FileEntry>();

        public string WorkingDirectory { get; set; }

        public FileViewerManager TempFileManager { get; set; }

        private IParent _selectedFolder;

		public IParent SelectedFolder
        {
            get
            {
                return this._selectedFolder;
            }
            set
            {
                this._selectedFolder = value;
                this.OnFolderSelected(this, null);
            }
        }

		public PackageDatabase BundleDB { get; set; }

        private Action<string> progressStringCallback { get; set; }

        private string CurrentProgressString { get; set; }

        public PackageBrowser()
        {
            this.TempFileManager = new FileViewerManager();
        }

        public void LoadDatabase(string path, Action<string> progressString = null)
        {
            this.progressStringCallback = progressString;

            this.WorkingDirectory = Path.GetDirectoryName(path);

            this.LoadTimer = new UITimer{
				Interval = 0.01
			};
            this.LoadTimer.Elapsed += this.LoadPackagesUpdate;
            this.LoadTimer.Start();

            Thread loadThread = new Thread(() => this.LoadPackages(path));
            loadThread.IsBackground = true;
            loadThread.Start();
        }

        bool finishedLoad = false;
        public void LoadPackagesUpdate(object sender, EventArgs e)
        {
            this.progressStringCallback?.Invoke(this.CurrentProgressString);

            if (this.finishedLoad)
            {
                this.finishedLoad = false;
                this.OnWorkingDirectoryUpdated?.Invoke(this, null);

                this.LoadTimer.Stop();
                this.LoadTimer = null;
                
            }
        }

        public void LoadPackages(string filename)
        {
            this.CurrentProgressString = "Beginning Operation";

            this.PackageHeaders = new SortedDictionary<Idstring, PackageHeader>();

            this.CurrentProgressString = "Loading Existing Hashes";

            App.Instance.LoadHashlists();

            this.CurrentProgressString = "Loading Database";
            //Load Bundle Database
            this.BundleDB = new PackageDatabase(filename);

            this.CurrentProgressString = "Loading Hashlist";
            General.LoadHashlist(this.WorkingDirectory, this.BundleDB);

            this.CurrentProgressString = "Registering File Entries";
			Dictionary<uint, FileEntry> fileEntries = this.DatabaseEntryToFileEntry(this.BundleDB.GetDatabaseEntries());

            this.CurrentProgressString = "Loading Packages";

            List<string> files = Directory.EnumerateFiles(this.WorkingDirectory, "*.bundle").ToList();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (file.EndsWith("_h.bundle"))
                    continue;

			    PackageHeader bundle = new PackageHeader();
                if (!bundle.Load(file))
                    continue;

                this.CurrentProgressString = String.Format("Loading Package {0}/{1}", i, files.Count);
				this.AddBundleEntriesToFileEntries (fileEntries, bundle.Entries);
                this.PackageHeaders.Add(bundle.Name, bundle);
            }

            this.CurrentProgressString = "Registring Folder Layout";
            this.Root = new FolderItem(fileEntries) { Path = "assets", Name = "assets" };
            foreach(FileEntry entry in fileEntries.Values)
                this.RawFiles.Add(new Tuple<Idstring, Idstring, Idstring> (entry._path, entry._language, entry._extension), entry);

            this.finishedLoad = true;
            HashIndex.Clear();
            GC.Collect();
            this.CurrentProgressString = "Finishing";
        }

		public Dictionary<uint, FileEntry> DatabaseEntryToFileEntry(List<DatabaseEntry> entries)
        {
			Dictionary<uint, FileEntry> fileEntries = new Dictionary<uint, FileEntry>();
            foreach (DatabaseEntry ne in entries)
            {
				FileEntry fe = new FileEntry(ne, this.BundleDB) {ParentBrowser = this};
				fileEntries.Add(ne.ID, fe);
            }
            return fileEntries;
        }

		private void AddBundleEntriesToFileEntries(Dictionary<uint, FileEntry> fileEntries, List<PackageFileEntry> bes)
		{
            foreach (PackageFileEntry be in bes)
				if (fileEntries.ContainsKey (be.ID))
					fileEntries[be.ID].AddBundleEntry(be);
		}

        public void Dispose()
        {
            this.TempFileManager.Dispose();
        }
    }
}
