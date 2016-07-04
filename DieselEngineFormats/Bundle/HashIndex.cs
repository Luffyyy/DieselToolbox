using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DieselEngineFormats.Utils;

namespace DieselEngineFormats.Bundle
{
    public class Idstring : IComparable
    {
        private string _UnHashed;

        public string UnHashed
        {
            get
            {
                if (_UnHashed == null)
                {
                    _UnHashed = "ERROR: " + String.Format("{0:x}", HashedString);
                }

                return _UnHashed;
            }
        }

        public bool HasUnHashed { get; set; }

        public bool Same { get; set; }

        private string _HashedString;

        public string HashedString
        {
            get
            {
                if (_HashedString == null)
                {
                    _HashedString = String.Format("{0:x}", this.Hashed);
                    if (_HashedString.Length != 16)
                    {
                        string preStr = "";
                        for (int i = 0; i < 16 - _HashedString.Length; i++)
                            preStr += "0";
                        _HashedString = preStr + _HashedString;
                    }
                }

                return _HashedString;
            }
        }

        private ulong _hashed = 0;

        public ulong Hashed
        {
            get
            {
                if (_hashed == 0)
                {
                    _hashed = Hash64.HashString(UnHashed);
                }
                return _hashed;
            }
        }

        public Idstring(string str, bool same = false)
        {
            if (same)
                _HashedString = str;

            this.Same = same;

            HasUnHashed = true;
            _UnHashed = str;
        }

        public Idstring(ulong hashed)
        {
            _hashed = hashed;
            HasUnHashed = false;
        }

        public void SwapEdianness()
        {
            _hashed = General.SwapEdianness(this.Hashed);

            if (!this.Same)
                this._HashedString = null;
        }

        public string Tag { get; set; }

        public override string ToString()
        {
            return this.HasUnHashed ? _UnHashed : HashedString;
        }

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo((obj as Idstring)?.ToString() ?? "");
        }
    }

    public static class HashIndex
    {
        private static Dictionary<ulong, Idstring> exts = new Dictionary<ulong, Idstring>();
		private static Dictionary<ulong, Idstring> paths = new Dictionary<ulong, Idstring>();
        //private Dictionary<uint, string> sounds = new Dictionary<uint, string>();
		private static Dictionary<ulong, Idstring> others = new Dictionary<ulong, Idstring>();

		public static string version;

        public static Idstring GetPath(ulong hash)
        {
            if (paths.ContainsKey(hash))
                return paths[hash];
            else
                return new Idstring(hash);
        }

        public static Idstring GetExtension(ulong hash)
        {
			if (exts.ContainsKey (hash))
				return exts [hash];
			else
				return new Idstring(hash);
        }

		public static Idstring GetOther(ulong hash)
        {
			if (others.ContainsKey (hash))
				return others [hash];
			else
                return new Idstring(hash);
        }

		public static Idstring GetAny(ulong hash)
        {
            if (paths.ContainsKey(hash))
                return paths[hash];
            else if (exts.ContainsKey(hash))
                return exts[hash];
            else if (others.ContainsKey(hash))
                return others[hash];
            else
                return new Idstring(hash);
        }

		public static void Load(ref HashSet<string> new_paths, ref HashSet<string> new_exts)
        {
            foreach (string path in new_paths)
            {
                Idstring ids = new Idstring(path);
                CheckCollision(paths, ids.Hashed, ids);
                paths[ids.Hashed] = ids;
            }

            foreach (string ext in new_exts)
            {
                Idstring ids = new Idstring(ext);
                CheckCollision(exts, ids.Hashed, ids);
                exts[ids.Hashed] = ids;
            }
        }

		private static void CheckCollision(Dictionary<ulong, Idstring> item, ulong hash, Idstring value)
        {
            if ( item.ContainsKey(hash) && (item[hash] != value) )
            {
                Console.WriteLine("Hash collision: {0:x} : {1} == {2}", hash, item[hash], value);
            }
        }

		public static void Clear()
        {
            exts.Clear();
            paths.Clear();
			others.Clear();
        }

		public static void GenerateHashList(string HashlistFile, string tag = null)
        {
            StreamWriter fs = new StreamWriter(HashlistFile, false);

            if (version != null)
                fs.WriteLine("//" + version);

            foreach (KeyValuePair<ulong, Idstring> pair in exts)
            {
                if (tag == null || (pair.Value.Tag == tag))
                    fs.WriteLine(pair.Value.UnHashed);
            }

            foreach (KeyValuePair<ulong, Idstring> pair in paths)
            {
                if (tag == null || (pair.Value.Tag == tag))
                    fs.WriteLine(pair.Value.UnHashed);
            }

            foreach (KeyValuePair<ulong, Idstring> pair in others)
            {
                if (tag == null || (pair.Value.Tag == tag))
                    fs.WriteLine(pair.Value.UnHashed);
            }

            fs.Close();
        }

        public static bool AddHash(string hash, string tag = null)
        {
            Idstring ids = new Idstring(hash) { Tag = tag };
            Dictionary<ulong, Idstring> hash_tbl = others;
            if ((hash.Contains("/") || hash.Contains("\\") || hash.Equals("idstring_lookup") || hash.Equals("existing_banks")) && !paths.ContainsKey(ids.Hashed))
            {
                hash_tbl = paths;
                hash = hash.ToLower();
            }
            else if (!hash.Contains("/") && !hash.Contains(".") && !hash.Contains(":") && !hash.Contains("\\") && !exts.ContainsKey(ids.Hashed))
                hash_tbl = exts;

            if (!hash_tbl.ContainsKey(ids.Hashed))
            {
                hash_tbl.Add(ids.Hashed, ids);
                return true;
            }

            return false;
        }

		public static void Load(ref HashSet<string> new_paths, ref HashSet<string> new_exts, ref HashSet<string> new_other)
        {
            foreach (string path in new_paths)
            {
                Idstring ids = new Idstring(path);
                CheckCollision(paths, ids.Hashed, ids);
                paths[ids.Hashed] = ids;
            }

            foreach (string ext in new_exts)
            {
                Idstring ids = new Idstring(ext);
                CheckCollision(exts, ids.Hashed, ids);
                exts[ids.Hashed] = ids;
            }

            foreach (string other in new_other)
            {
                Idstring ids = new Idstring(other);
                CheckCollision(exts, ids.Hashed, ids);
                others[ids.Hashed] = ids;
            }
        }

		public static bool Load(string HashlistFile)
        {
            try
            {
                string tag = Path.GetFileName(HashlistFile);

                foreach (string hash in File.ReadLines(HashlistFile))
                {
                    if (String.IsNullOrEmpty(hash) || hash.StartsWith("//")) //Check for empty or comment
                        continue;

                    Idstring ids = new Idstring(hash) { Tag = tag };

                    if ((hash.Contains("/") || hash.Contains("\\") || hash.Equals("idstring_lookup") || hash.Equals("existing_banks")) && !paths.ContainsKey(ids.Hashed))
                        paths.Add(ids.Hashed, ids);
                    else if (!hash.Contains("/") && !hash.Contains(".") && !hash.Contains(":") && !hash.Contains("\\") && !exts.ContainsKey(ids.Hashed))
                        exts.Add(ids.Hashed, ids);
                    else if (!others.ContainsKey(ids.Hashed))
                        others.Add(ids.Hashed, ids);

                    /*else if (hash.Contains("pln_"))
                    {
                        uint wwiseHashed = Hash64.WWiseHash(hash);
                        if (!sounds.ContainsKey(wwiseHashed))
                            sounds.Add(wwiseHashed, hash);
                    }*/
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            return true;
        }
    }
}
