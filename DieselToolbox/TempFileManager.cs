using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DieselToolbox
{
    public class FileViewerManager : IDisposable
    {
        public class TempFile : IDisposable
        {
            public TempFile(FileEntry entry, PackageFileEntry be = null, FormatConverter exporter = null)
            {
                this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Definitions.TempDir, $"{entry._path.HashedString}.{entry._extension.ToString()}");

                if (exporter != null && exporter.Extension != null)
                    this.Path += "." + exporter.Extension;

                object file_data = entry.FileData(be, exporter);

                if (file_data is byte[])
                {
                    File.WriteAllBytes(this.Path, (byte[])file_data);
                }
                else if (file_data is Stream)
                {
                    using (FileStream file_stream = File.Create(this.Path))
                        ((Stream)file_data).CopyTo(file_stream);

                    ((Stream)file_data).Close();
                }
                else if (file_data is string)
                    File.WriteAllText(this.Path, (string)file_data);
                else if (file_data is string[])
                    File.WriteAllLines(this.Path, (string[])file_data);

                this.Entry = be;
                if(exporter != null)
                    this.ExporterKey = exporter.Key;
            }

            ~TempFile()
            {
                this.Dispose();
            }

            public string Path { get; set; }

            public Process RunProcess { get; set; }

            public PackageFileEntry Entry { get; set; }

            public string ExporterKey { get; set; }

            public bool Disposed { get; set; }

            public void Dispose()
            {
                if (this.Disposed)
                    return;

                try
                {

                    if (!(this.RunProcess?.HasExited ?? true))
                        this.RunProcess?.Kill();

                    if (File.Exists(this.Path))
                        File.Delete(this.Path);

                    Console.WriteLine("Deleted temp file {0}", this.Path);

                }
                catch (Exception exc){
                    Console.WriteLine(exc.Message);
                }

                this.Disposed = true;
            }
        }

        private Dictionary<FileEntry, TempFile> TempFiles = new Dictionary<FileEntry, TempFile>();

        private UITimer Timer;

        public bool Disposed { get; set; }

        public FileViewerManager()
        {
            this.Timer = new UITimer { Interval = 30 };
            this.Timer.Elapsed += this.Update;
            //Timer.Start();
            string temp_path;

            try
            {

                if (!Directory.Exists(temp_path = Path.Combine(Path.GetTempPath(), Definitions.TempDir)))
                    Directory.CreateDirectory(temp_path);
                else
                {
                    foreach (string file in Directory.GetFiles(temp_path))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        ~FileViewerManager()
        {
            this.Dispose();
        }

        private bool IsFileAvailable(string path)
        {
            try
            {
                using (FileStream str = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return str.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool ShouldDeleteFile(TempFile file)
        {
            return file.RunProcess != null && file.RunProcess.HasExited && IsFileAvailable(file.Path);
        }

        private void Update(object sender, EventArgs e)
        {
            if (this.TempFiles.Count == 0)
                return;

            List<FileEntry> to_delete = new List<FileEntry>();
            foreach (KeyValuePair<FileEntry, TempFile> temp in this.TempFiles)
            {
                if (this.IsFileAvailable(temp.Value.Path))
                    to_delete.Add(temp.Key);
            }

            foreach (FileEntry ent in to_delete)
            {
                this.DeleteTempFile(ent);
            }
        }

        public void ViewFile(FileEntry entry, PackageFileEntry be = null)
        {
            try
            {
                Console.WriteLine(entry.BundleEntries.Count);
                if (entry.BundleEntries.Count == 0)
                    return;

                string typ = Definitions.TypeFromExtension(entry._extension.ToString());
                FormatConverter exporter = null;

                if (ScriptActions.Converters.ContainsKey(typ))
                {
                  //  if(ScriptActions.Converters[typ].Count > 1)
                   // {
                        SaveOptionsDialog dlg = new SaveOptionsDialog(typ);
                        DialogResult dlgres = dlg.ShowModal();

                        if (dlgres == DialogResult.Cancel)
                            return;

                        exporter = dlg.SelectedExporter;
                  //  }
                }

                //Thread thread = new Thread(() =>
                //{
                TempFile temp = this.GetTempFile(entry, be, exporter);
                //{
                GC.Collect();
                ProcessStartInfo pi = new ProcessStartInfo(temp.Path);
                
                pi.UseShellExecute = true;
                if (General.IsLinux)
                {
                    pi.Arguments = temp.Path;
                    pi.FileName = "xdg-open";
                }
                else
                    pi.FileName = temp.Path;

                Process proc = Process.Start(pi);
                //temp.RunProcess = proc;
                if (!this.TempFiles.ContainsKey(entry))
                    this.TempFiles.Add(entry, temp);
                        /*if (proc == null)//seconds -> milliseconds
                            Thread.Sleep(20 * 1000);
                        proc?.WaitForExit();
                        while((!(proc?.HasExited ?? true)))
                        { }

                        if (General.IsLinux && (proc?.ExitCode == 3 || proc?.ExitCode == 4))
                            Console.WriteLine("No default file association for filetype {0}", Path.GetExtension(temp.Path));

                        while (!this.IsFileAvailable(temp.Path))
                        {
                            Console.WriteLine("Waiting on file");
                        }
                    }
                    this.TempFiles.Remove(entry);*/
                //});
                //thread.IsBackground = true;
                //thread.Start();

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
            }
        }

        public TempFile CreateTempFile(FileEntry entry, PackageFileEntry be = null, FormatConverter exporter = null)
        {
            if (this.TempFiles.ContainsKey(entry))
                this.DeleteTempFile(entry);

            TempFile temp = new TempFile(entry, be, exporter);

            return temp;
        }

        public TempFile GetTempFile(FileEntry file, PackageFileEntry entry = null, FormatConverter exporter = null)
        {
            TempFile path;
            if (!this.TempFiles.ContainsKey(file) || this.TempFiles[file].Disposed || !File.Exists(this.TempFiles[file].Path) || this.TempFiles[file].ExporterKey != exporter.Key || this.TempFiles[file].Entry != entry)
            {
                if (this.TempFiles.ContainsKey(file))
                    this.DeleteTempFile(file);

                path = this.CreateTempFile(file, entry, exporter);
            }
            else
                path = this.TempFiles[file];

            return path;
        }

        public void DeleteTempFile(FileEntry entry)
        {
            if (!this.TempFiles.ContainsKey(entry))
                return;

            TempFile temp_file = this.TempFiles[entry];

            temp_file.Dispose();

            this.TempFiles.Remove(entry);
        }

        public void DeleteAllTempFiles()
        {
            List<TempFile> key_list = this.TempFiles.Values.ToList();
            for (int i = 0; i < this.TempFiles.Count; i++)
            {
                key_list[i].Dispose();
            }

            this.TempFiles = new Dictionary<FileEntry, TempFile>();
        }

        public void Dispose()
        {
            if (this.Disposed)
                return;

            this.DeleteAllTempFiles();

            this.Disposed = true;
        }
    }
}
