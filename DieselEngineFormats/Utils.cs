// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="">
//   
// </copyright>
// <summary>
//   Utils.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DieselEngineFormats.Utils
{
    using DieselEngineFormats.Bundle;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// The hash 64.
    /// </summary>
    public class Hash64
    {
		private static void mix64(ref ulong a, ref ulong b, ref ulong c)
		{
			a -= b; a -= c; a ^= (c >> 43);
			b -= c; b -= a; b ^= (a << 9);
			c -= a; c -= b; c ^= (b >> 8);
			a -= b; a -= c; a ^= (c >> 38);
			b -= c; b -= a; b ^= (a << 23);
			c -= a; c -= b; c ^= (b >> 5);
			a -= b; a -= c; a ^= (c >> 35);
			b -= c; b -= a; b ^= (a << 49);
			c -= a; c -= b; c ^= (b >> 11);
			a -= b; a -= c; a ^= (c >> 12);
			b -= c; b -= a; b ^= (a << 18);
			c -= a; c -= b; c ^= (b >> 22);
		}

		#region Public Methods and Operators

		/// <summary>
		/// The hash.
		/// </summary>
		/// <param name="k">
		/// The k.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		/// <param name="level">
		/// The level.
		/// </param>
		/// <returns>
		/// The <see cref="ulong"/>.
		/// </returns>
		public static ulong Hash (byte[] k, ulong length, ulong level)
		{			
			ulong len = length;
			ulong a = level, b = level;
			ulong c = 0x9e3779b97f4a7c13L;

			int len_x = 0;
			while (len >= 24) {
				a += (k[len_x + 0] + (((ulong)k[len_x + 1]) << 8) + (((ulong)k[len_x + 2]) << 16) + (((ulong)k[len_x + 3]) << 24)
					+ (((ulong)k[len_x + 4]) << 32) + (((ulong)k[len_x + 5]) << 40) + (((ulong)k[len_x + 6]) << 48) + (((ulong)k[len_x + 7]) << 56));
				b += (k[len_x + 8] + (((ulong)k[len_x + 9]) << 8) + (((ulong)k[len_x + 10]) << 16) + (((ulong)k[len_x + 11]) << 24)
					+ (((ulong)k[len_x + 12]) << 32) + (((ulong)k[len_x + 13]) << 40) + (((ulong)k[len_x + 14]) << 48) + (((ulong)k[len_x + 15]) << 56));
				c += (k[len_x + 16] + (((ulong)k[len_x + 17]) << 8) + (((ulong)k[len_x + 18]) << 16) + (((ulong)k[len_x + 19]) << 24)
					+ (((ulong)k[len_x + 20]) << 32) + (((ulong)k[len_x + 21]) << 40) + (((ulong)k[len_x + 22]) << 48) + (((ulong)k[len_x + 23]) << 56));
				mix64(ref a, ref b, ref c);
				len_x += 24; len -= 24;
			}

			c += length;

			if (len <= 23) {
				while (len > 0) {

					switch (len) {              // all the case statements fall through
					case 23:
						c += ((ulong)k[len_x + 22] << 56);
						break;
					case 22:
						c += ((ulong)k[len_x + 21] << 48);
						break;
					case 21:
						c += ((ulong)k[len_x + 20] << 40);
						break;
					case 20:
						c += ((ulong)k[len_x + 19] << 32);
						break;
					case 19:
						c += ((ulong)k[len_x + 18] << 24);
						break;
					case 18:
						c += ((ulong)k[len_x + 17] << 16);
						break;
					case 17:
						c += ((ulong)k[len_x + 16] << 8);
						break;
						/* the first byte of c is reserved for the length */
					case 16:
						b += ((ulong)k[len_x + 15] << 56);
						break;
					case 15:
						b += ((ulong)k[len_x + 14] << 48);
						break;
					case 14:
						b += ((ulong)k[len_x + 13] << 40);
						break;
					case 13:
						b += ((ulong)k[len_x + 12] << 32);
						break;
					case 12:
						b += ((ulong)k[len_x + 11] << 24);
						break;
					case 11:
						b += ((ulong)k[len_x + 10] << 16);
						break;
					case 10:
						b += ((ulong)k[len_x + 9] << 8);
						break;
					case  9:
						b += ((ulong)k[len_x + 8]);
						break;
					case  8:
						a += ((ulong)k[len_x + 7] << 56);
						break;
					case  7:
						a += ((ulong)k[len_x + 6] << 48);
						break;
					case  6:
						a += ((ulong)k[len_x + 5] << 40);
						break;
					case  5:
						a += ((ulong)k[len_x + 4] << 32);
						break;
					case  4:
						a += ((ulong)k[len_x + 3] << 24);
						break;
					case  3:
						a += ((ulong)k[len_x + 2] << 16);
						break;
					case  2:
						a += ((ulong)k[len_x + 1] << 8);
						break;
					case  1:
						a += ((ulong)k[len_x + 0]);
						break;
						/* case 0: nothing left to add */
					}
					len--;
				}
			}
			mix64 (ref a, ref b, ref c);
			/*-------------------------------------------- report the result */
			return c;
		}

        /// <summary>
        /// The hash string.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <returns>
        /// The <see cref="ulong"/>.
        /// </returns>
        public static ulong HashString(string input, ulong level = 0)
        {
            return Hash(Encoding.UTF8.GetBytes(input), (ulong)Encoding.UTF8.GetByteCount(input), level);
        }

        /*public static uint WWiseHash(string input)
        {
            uint hashed = 2166136261;
            
            foreach (char pData in input)
            {
                hashed *= 16777619;
                hashed ^= pData;
            }
            
            return hashed;
        }*/

        #endregion
    }

    public static class General
    {
        public static string HashlistFile = "hashlist";

        public static string HashlistVersion(string hashlistPath)
        {
            if (File.Exists(hashlistPath))
            {
                using (FileStream fs = new FileStream(hashlistPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader st = new StreamReader(fs))
                    {
                        //Read first line, remove first (//) comment characters
                        return st.ReadLine().Remove(0, 2);
                    }
                }
            }
            else
                return "0";
        }

        public static void LoadHashlist(string workingPath, PackageDatabase bundleDB)
        {
            string hashlistPath = Path.Combine(workingPath, HashlistFile);

            if (File.Exists(hashlistPath))
            {
				string gameverPath;
				if (File.Exists(gameverPath = Path.GetFullPath(Path.Combine(workingPath, "../game.ver"))))
                {
					string ver = File.ReadAllText(gameverPath);
                    HashIndex.version = ver;
                    if (!ver.Equals(HashlistVersion(hashlistPath)))
                    {
                        File.Delete(hashlistPath);
                    }
                }
                else
                {
                    Console.WriteLine("game.ver could not be found, the Hashlist will not automatically update without this!");
                }

                if (File.Exists(hashlistPath))
                {
					HashIndex.Load(hashlistPath);
                }
            }

            if (!File.Exists(hashlistPath))
            {
                GenerateHashlist(workingPath, bundleDB);
            }
        }

        public static void GenerateHashlist(string workingPath, PackageDatabase bundleDB)
        {
            List<string> headers = Directory.EnumerateFiles(workingPath, "all_*_h.bundle").ToList();

            foreach (string file in (headers.Count == 0 ? Directory.EnumerateFiles(workingPath, "all_*.bundle") : headers))
            {
                string bundle_file = file.Replace("_h", "");
                if (File.Exists(bundle_file))
                {
					PackageHeader bundle = new PackageHeader(file);
                    foreach (PackageFileEntry be in bundle.Entries)
                    {
                        DatabaseEntry ne = bundleDB.EntryFromID(be.ID);
                        if (ne == null)
                            continue;

                        if (ne._path == 0x9234DD22C60D71B8 && ne._extension == 0x9234DD22C60D71B8)
                        {
                            GenerateHashlist(workingPath, Path.Combine(workingPath, bundle_file), be);
                            return;
                        }
                    }

                }
            }
        }

        public static void GenerateHashlist(string workingPath, string file, PackageFileEntry be)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] data;
                    StringBuilder sb = new StringBuilder();
                    string[] idstring_data;
                    HashSet<string> new_paths = new HashSet<string>();

                    fs.Position = be.Address;
                    if (be.Length == -1)
                        data = br.ReadBytes((int)(fs.Length - fs.Position));
                    else
                        data = br.ReadBytes((int)be.Length);

                    foreach (byte read in data)
                        sb.Append((char)read);

                    idstring_data = sb.ToString().Split('\0');
                    sb.Clear();

                    foreach (string idstring in idstring_data)
                    {
                            new_paths.Add(idstring);
                    }

                    new_paths.Add("idstring_lookup");
                    new_paths.Add("existing_banks");
                    new_paths.Add("engine-package");

                    HashIndex.Clear();

					HashIndex.Load(ref new_paths);

					HashIndex.GenerateHashList(Path.Combine(workingPath, HashlistFile));

                    new_paths.Clear();
                }
            }
        }

        public static Idstring UnHashString(string ID)
        {
			Idstring str;
			TryUnHashString(ID, out str);
			return str;
        }

		public static bool TryUnHashString(string ID, out Idstring pck)
		{
			ulong pkgname_hash;
			if (!ulong.TryParse(ID, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out pkgname_hash))
			{
				Console.WriteLine("Failed converting package name: " + ID);
                pck = null;
				return false;
			}

			return TryUnHash (pkgname_hash, out pck);
		}

		public static Idstring UnHash(ulong id)
		{
			Idstring str;
			TryUnHash (id, out str);
			return str;
		}

        public static ulong SwapEdianness(ulong id)
        {
            return id = ((0x00000000000000FF) & (id >> 56)
                | (0x000000000000FF00) & (id >> 40)
                | (0x0000000000FF0000) & (id >> 24)
                | (0x00000000FF000000) & (id >> 8)
                | (0x000000FF00000000) & (id << 8)
                | (0x0000FF0000000000) & (id << 24)
                | (0x00FF000000000000) & (id << 40)
                | (0xFF00000000000000) & (id << 56));
        }

		public static bool TryUnHash(ulong id, out Idstring pck)
		{
            id = SwapEdianness(id);

            pck = HashIndex.Get(id);

			return true;
		}

		public static Idstring BundleNameToPackageID(string bundle_id)
		{
			if (bundle_id.StartsWith ("all_")) {
				return new Idstring (bundle_id, true);
			}
			return UnHashString(bundle_id);
		}

		public static string GetFullFilepath(DatabaseEntry dbEntry, PackageDatabase BundleDB = null)
		{
			if (dbEntry == null)
				return "";

			string file = "";

			Idstring path, lang, extension;

			GetFilepath (dbEntry, out path, out lang, out extension, BundleDB);

			file += path.ToString();

			file += lang == null ? "" : "." + lang.ToString();

			file += "." + extension.ToString();

			return file;
		}

		public static void GetFilepath(DatabaseEntry dbEntry,  out Idstring path, out Idstring language, out Idstring extension, PackageDatabase BundleDB = null)
		{
			path = dbEntry.Path;

			if (dbEntry.Language != 0) {
                if (BundleDB == null)
                    language = new Idstring(dbEntry.Language);
                else
                    language = BundleDB.LanguageFromID(dbEntry.Language)?.Name ?? new Idstring(dbEntry.Language.ToString(), true);
            } else
				language = null;

			extension = dbEntry.Extension;
		}

        // From http://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-windows-or-linux-without-using-a-con
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}