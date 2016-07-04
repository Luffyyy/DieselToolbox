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

        public List<FileEntry> RawFiles = new List<FileEntry>();

        public string WorkingDirectory { get; set; }

        public FileViewerManager TempFileManager { get; set; }

        private IParent _selectedFolder;

		public IParent SelectedFolder
        {
            get
            {
                return _selectedFolder;
            }
            set
            {
                _selectedFolder = value;
                this.OnFolderSelected(this, null);
            }
        }

		public BundleDatabase BundleDB { get; set; }

        private Action<string> progressStringCallback { get; set; }

        private string CurrentProgressString { get; set; }

        public PackageBrowser()
        {
            TempFileManager = new FileViewerManager();
        }

        public void LoadDatabase(string path, Action<string> progressString = null)
        {
            progressStringCallback = progressString;

            this.WorkingDirectory = Path.GetDirectoryName(path);

			LoadTimer = new UITimer{
				Interval = 0.01
			};
			LoadTimer.Elapsed += this.LoadPackagesUpdate;
            LoadTimer.Start();

            Thread loadThread = new Thread(() => this.LoadPackages(path));
            loadThread.IsBackground = true;
            loadThread.Start();
        }

        bool finishedLoad = false;
        public void LoadPackagesUpdate(object sender, EventArgs e)
        {
            progressStringCallback?.Invoke(CurrentProgressString);

            if (finishedLoad)
            {
				finishedLoad = false;
                this.OnWorkingDirectoryUpdated?.Invoke(this, null);

                LoadTimer.Stop();
                LoadTimer = null;
                
            }
        }

        public void LoadPackages(string filename)
        {
            CurrentProgressString = "Beginning Operation";

            PackageHeaders = new SortedDictionary<Idstring, PackageHeader>();

            CurrentProgressString = "Loading Database";
            //Load Bundle Database
			BundleDB = new BundleDatabase(filename);

            CurrentProgressString = "Loading Hashlist";
            General.LoadHashlist(this.WorkingDirectory, BundleDB);

            CurrentProgressString = "Registering File Entries";
			Dictionary<uint, FileEntry> fileEntries = this.DatabaseEntryToFileEntry(BundleDB.GetDatabaseEntries());

            CurrentProgressString = "Loading Package Headers";
            List<string> files = Directory.EnumerateFiles(this.WorkingDirectory, "*_h.bundle").ToList();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string bundle_file = file.Replace("_h.bundle", "");
                if (File.Exists(bundle_file + ".bundle"))
                {
                    CurrentProgressString = String.Format("Loading Package {0}/{1}", i, files.Count);
					PackageHeader bundle = new PackageHeader(bundle_file);
					this.AddBundleEntriesToFileEntries (fileEntries, bundle.Entries);
                    PackageHeaders.Add(bundle.Name, bundle);
                }
            }

			CurrentProgressString = "Registring Folder Layout";
			Root = new FolderItem(fileEntries) { Path = "assets" };
            RawFiles = fileEntries.Values.ToList();
            finishedLoad = true;
            CurrentProgressString = "Finishing";
        }

		public Dictionary<uint, FileEntry> DatabaseEntryToFileEntry(List<DatabaseEntry> entries)
        {
			Dictionary<uint, FileEntry> fileEntries = new Dictionary<uint, FileEntry>();
            foreach (DatabaseEntry ne in entries)
            {
				FileEntry fe = new FileEntry(ne, BundleDB) {ParentBrowser = this};
				fileEntries.Add(ne.ID, fe);
            }
            return fileEntries;
        }

		private void AddBundleEntriesToFileEntries(Dictionary<uint, FileEntry> fileEntries, List<BundleFileEntry> bes)
		{
			foreach (BundleFileEntry be in bes) {
				if (fileEntries.ContainsKey (be.ID)) {
					fileEntries[be.ID].AddBundleEntry(be);
				}
			}
		}

        public void Dispose()
        {
            this.TempFileManager.Dispose();
        }
    }
}
