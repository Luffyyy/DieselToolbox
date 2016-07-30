using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DieselEngineFormats.Utils;

namespace DieselEngineFormats.Bundle
{
    public class Idstring : IComparable, ICloneable
    {
        private readonly string[] _UnHashedParts;

        public string[] UnHashedParts { get { return _UnHashedParts ?? new[] { HashedString }; } }

        public string UnHashed
        {
            get
            {
                if (_UnHashedParts == null || _UnHashedParts.Length == 0)
                {
                    return null;//"ERROR: " + String.Format("{0:x}", HashedString);
                }

                string built_string = _UnHashedParts[0];
                for (int i = 1; i < _UnHashedParts.Length; i++)
                {
                    built_string += "/" + _UnHashedParts[i];
                }
                return built_string;
            }
        }

        public bool HasUnHashed { get; set; }

        public bool Same { get; set; }

        public bool IsPath { get { return _UnHashedParts.Length > 1 || this.UnHashed == "existing_banks" || this.UnHashed == "idstring_lookup"; } }

        public string HashedString
        {
            get
            {
                if (Same)
                    return UnHashed;

                string _HashedString = String.Format("{0:x}", this.Hashed);
                if (_HashedString.Length != 16)
                {
                    string preStr = "";
                    for (int i = 0; i < 16 - _HashedString.Length; i++)
                        preStr += "0";
                    _HashedString = preStr + _HashedString;
                }

                return _HashedString;
            }
        }

        //public ulong _hashed = 0;

        public ulong Hashed
        {
            get
            {
                /*if (_hashed == 0)
                    _hashed = Hash64.HashString(this.UnHashed);

                return _hashed;*/
                return Hash64.HashString(this.UnHashed);
            }
        }

        public Idstring(string str, Dictionary<string, string> strings = null, bool same = false)
        {
            this.Same = same;

            HasUnHashed = true;
            string[] parts = str.Split('/');
            if (strings != null)
            {
                lock (strings)
                {
                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        string part = parts[i];
                        if (!strings.ContainsKey(part))
                            strings.Add(part, part);

                        parts[i] = strings[part];
                    }
                }
            }

            this._UnHashedParts = parts;
        }

        public Idstring(ulong hashed)
        {
            //_hashed = hashed;
            HasUnHashed = false;
        }

        public void SwapEdianness()
        {
            //_hashed = General.SwapEdianness(this.Hashed);
        }

        public string Tag { get; set; }

        public override string ToString()
        {
            return this.HasUnHashed ? UnHashed : HashedString;
        }

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo((obj as Idstring)?.ToString() ?? "");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public static class HashIndex
    {
        private static Dictionary<ulong, Idstring> paths = new Dictionary<ulong, Idstring>();
        private static Dictionary<ulong, Idstring> others = new Dictionary<ulong, Idstring>();
        private static Dictionary<string, string> strings = new Dictionary<string, string>();

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
            return GetOther(hash);
        }

        public static Idstring GetOther(ulong hash)
        {
            if (others.ContainsKey(hash))
                return others[hash];
            else
                return new Idstring(hash);
        }

        public static Idstring GetAny(ulong hash)
        {
            if (paths.ContainsKey(hash))
                return paths[hash];
            else if (others.ContainsKey(hash))
                return others[hash];
            else
                return new Idstring(hash);
        }

        public static void Load(ref HashSet<string> new_paths, ref HashSet<string> new_others)
        {
            foreach (string path in new_paths)
            {
                Idstring ids = new Idstring(path, strings);
                CheckCollision(paths, ids.Hashed, ids);
                paths[ids.Hashed] = ids;
            }

            foreach (string other in new_others)
            {
                Idstring ids = new Idstring(other, strings);
                CheckCollision(others, ids.Hashed, ids);
                others[ids.Hashed] = ids;
            }
        }

        private static void CheckCollision(Dictionary<ulong, Idstring> item, ulong hash, Idstring value)
        {
            if (item.ContainsKey(hash) && (item[hash] != value))
            {
                Console.WriteLine("Hash collision: {0:x} : {1} == {2}", hash, item[hash], value);
            }
        }

        public static void Clear()
        {
            paths.Clear();
            others.Clear();
        }

        public static void GenerateHashList(string HashlistFile, string tag = null)
        {
            StreamWriter fs = new StreamWriter(HashlistFile, false);

            if (version != null)
                fs.WriteLine("//" + version);

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

        public static Idstring CreateHash(string hash, string tag = null)
        {
            if ((hash.Contains("/") || hash.Equals("idstring_lookup") || hash.Equals("existing_banks")))
            {
                hash = hash.ToLower();
            }

            return new Idstring(hash, strings) { Tag = tag };
        }

        public static bool AddHash(string hash, string tag = null)
        {
            Idstring ids = CreateHash(hash, tag);

            return AddHash(ids);
        }

        public static bool AddHash(Idstring ids)
        {
            Dictionary<ulong, Idstring> hash_tbl = ids.IsPath ? paths : others;

            lock (hash_tbl)
            {
                if (!hash_tbl.ContainsKey(ids.Hashed))
                {
                    hash_tbl.Add(ids.Hashed, ids);
                    return true;
                }
            }

            return false;
        }

        public static bool Load(string HashlistFile)
        {
            try
            {
                string tag = Path.GetFileName(HashlistFile);
                System.Threading.Tasks.Parallel.ForEach(File.ReadLines(HashlistFile), hash =>
                //foreach (string hash in File.ReadLines(HashlistFile))
                {
                    if (String.IsNullOrEmpty(hash) || hash.StartsWith("//")) //Check for empty or comment
                        return; //continue;

                    AddHash(hash, tag);
                });

                GC.Collect();
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
