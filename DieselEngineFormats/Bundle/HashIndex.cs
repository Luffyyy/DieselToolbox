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
        private string UniqueUnhashed;
        private readonly int[] _UnHashedParts;

        public int[] UnHashedParts { get { return this._UnHashedParts; } }
        
        public string UnHashed
        {
            get
            {
                if ((this._UnHashedParts == null || this._UnHashedParts.Length == 0) && this.UniqueUnhashed == null)
                {
                    return null;//"ERROR: " + String.Format("{0:x}", HashedString);
                }

                string built_string = this.UniqueUnhashed;
                if (this._UnHashedParts != null)
                {
                    built_string = HashIndex.LookupString(this._UnHashedParts[0]);
                    for (int i = 1; i < this._UnHashedParts.Length; i++)
                    {
                        built_string += "/" + HashIndex.LookupString(this._UnHashedParts[i]);
                    }
                }
                return built_string;
            }
        }

        public bool HasUnHashed { get; set; }

        public bool Same { get; set; }

        public bool IsPath { get { return (this._UnHashedParts != null && this._UnHashedParts.Length > 1) || this.UnHashed == "existing_banks" || this.UnHashed == "idstring_lookup"; } }

        public string HashedString
        {
            get
            {
                if (this.Same)
                    return this.UnHashed;

                string _HashedString = String.Format("{0:x}", this.Hashed);
                if (_HashedString.Length != 16)
                {
                    _HashedString.Reverse();
                    for (int i = 0; i < 16 - _HashedString.Length; i++)
                        _HashedString += "0";
                    _HashedString.Reverse();
                }

                return _HashedString;
            }
        }

        public ulong _hashed = 0;

        public ulong Hashed
        {
            get
            {
                if (this._hashed == 0)
                    this._hashed = Hash64.HashString(this.UnHashed);

                return this._hashed;
                //return Hash64.HashString(this.UnHashed);
            }
        }

        public Idstring(string str, bool same = false)
        {
            this.Same = same;

            this.HasUnHashed = true;
            string[] parts = str.Split('/');
            int[] hash_parts = new int[parts.Length];
            if (parts.Length != 1)
            {
                lock (HashIndex.StringLookup)
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string part = parts[i];
                        if (!HashIndex.StringLookup.ContainsKey(part.GetHashCode()))
                            HashIndex.StringLookup.Add(part.GetHashCode(), part);

                        hash_parts[i] = part.GetHashCode();
                    }
                }
                this._UnHashedParts = hash_parts;
            }
            else
            {
                this.UniqueUnhashed = str;
            }
        }

        public Idstring(ulong hashed)
        {
            this._hashed = hashed;
            this.HasUnHashed = false;
        }

        public void SwapEdianness()
        {
            this._hashed = General.SwapEdianness(this.Hashed);
        }

        public string Tag { get; set; }

        public override string ToString()
        {
            return this.HasUnHashed ? this.UnHashed : this.HashedString;
        }

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo((obj as Idstring)?.ToString() ?? "");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return this.Hashed.GetHashCode();
        }
    }

    public static class HashIndex
    {
        public enum HashType
        {
            Path,
            Object,
            Ext = 4,
            Others = 8
        }

        private static Dictionary<ulong, Idstring> used_hashes = new Dictionary<ulong, Idstring>();
        private static Dictionary<ulong, Idstring> hashes = new Dictionary<ulong, Idstring>();
        public static Dictionary<ulong, Idstring> temp = new Dictionary<ulong, Idstring>();
        public static Dictionary<int, string> StringLookup = new Dictionary<int, string>();

        public static string version;

        public static Idstring Get(string hash)
        {
            Idstring ids;
            AddHash(hash, out ids);
            return ids;
        }

        public static Idstring Get(ulong hash)
        {
            if (hashes.ContainsKey(hash))
            {
                if (!used_hashes.ContainsKey(hash))
                    used_hashes.Add(hash, hashes[hash]);

                return hashes[hash];
            }
            else if (used_hashes.ContainsKey(hash))
                return used_hashes[hash];
            else
                return new Idstring(hash);
        }

        public static void Load(ref HashSet<string> new_hashes)
        {
            foreach (string path in new_hashes)
            {
                Idstring ids = new Idstring(path);
                CheckCollision(hashes, ids.Hashed, ids);
                hashes[ids.Hashed] = ids;
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
            hashes.Clear();
            temp.Clear();
        }

        public static void GenerateHashList(string HashlistFile, string tag = null)
        {
            StreamWriter fs = new StreamWriter(HashlistFile, false);

            if (version != null)
                fs.WriteLine("//" + version);

            foreach (KeyValuePair<ulong, Idstring> pair in hashes)
            {
                if (tag == null || (pair.Value.Tag == tag))
                    fs.WriteLine(pair.Value.UnHashed);
            }

            foreach (KeyValuePair<ulong, Idstring> pair in temp)
            {
                if (tag == null || (pair.Value.Tag == tag))
                    fs.WriteLine(pair.Value.UnHashed);
            }

            fs.Close();
        }

        public static Idstring CreateHash(string hash, string tag = null)
        {
            if (TypeOfHash(hash) == HashType.Path)
                hash = hash.ToLower();

            return new Idstring(hash) { Tag = tag };
        }

        public static HashType TypeOfHash(string hash)
        {
            if (hash.Contains("/") || hash.Equals("idstring_lookup") || hash.Equals("existing_banks"))
                return HashType.Path;

            if (hash.StartsWith("g_") && hash.StartsWith("c_") && hash.StartsWith("rp_") && hash.StartsWith("s_"))
                return HashType.Object;

            return HashType.Others;
        }

        public static bool AddHash(string hash, out Idstring ids, string tag = null, bool temp = false)
        {
            if (TypeOfHash(hash) == HashType.Path && hash.Contains(":"))
                hash = hash.Substring(0, hash.IndexOf(':'));

            ids = CreateHash(hash, tag);

            return AddHash(ids, temp);
        }

        public static bool HasHash(ulong hash)
        {
            return hashes.ContainsKey(hash) || temp.ContainsKey(hash);
        }

        public static bool AddHash(Idstring ids, bool temp = false)
        {
            Dictionary<ulong, Idstring> hash_tbl = temp ? HashIndex.temp : hashes;

            lock (hash_tbl)
            {
                if (!HasHash(ids.Hashed))
                {
                    hash_tbl.Add(ids.Hashed, ids);
                    return true;
                }
            }

            return false;
        }

        public static string LookupString(int hashcode)
        {
            return StringLookup.ContainsKey(hashcode) ? StringLookup[hashcode] : null;
        }

        public static bool Load(string HashlistFile, HashType? Types = null, bool temp = false)
        {
            try
            {
                string tag = Path.GetFileName(HashlistFile);
                System.Threading.Tasks.Parallel.ForEach(File.ReadLines(HashlistFile), hash =>
                //foreach (string hash in File.ReadLines(HashlistFile))
                {
                    if (String.IsNullOrEmpty(hash) || hash.StartsWith("//") || (Types != null && !((HashType)Types).HasFlag(TypeOfHash(hash)))) //Check for empty or comment
                        return; //continue;

                    Idstring ids;
                    AddHash(hash, out ids, tag, temp);
                });
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
