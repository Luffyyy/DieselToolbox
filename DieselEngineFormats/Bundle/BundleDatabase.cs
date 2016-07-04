// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameIndex.cs" company="Zwagoth">
//   This code is released into the public domain by Zwagoth.
// </copyright>
// <summary>
//   The name entry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DieselEngineFormats.Bundle
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     The name index.
    /// </summary>
    public class BundleDatabase
    {
        #region Fields

        /// <summary>
        ///     The id 2 name.
        /// </summary>
        private Dictionary<uint, DatabaseEntry> _entries = new Dictionary<uint, DatabaseEntry>();

        public Dictionary<uint, DatabaseEntry> Entries
        {
            get
            {
                return _entries;
            }
            set
            {
                _entries = value;
            }
        }

        /// <summary>
        ///     The defined languages.
        /// </summary>
        private Dictionary<uint, LanguageEntry> _languages = new Dictionary<uint, LanguageEntry>();

        public Dictionary<uint, LanguageEntry> Languages
        {
            get
            {
                return _languages;
            }
            set
            {
                _languages = value;
            }
        }

        #endregion

        #region Public Methods and Operators

		public BundleDatabase () { }

		public BundleDatabase (string path)
		{
			this.Load (path);	
		}

        /// <summary>
        /// The add to languages.
        /// </summary>
        /// <param name="hash">
        /// The hash of the language.
        /// </param>
        /// <param name="representation">
        /// The representation of the language.
        /// </param>
        public void AddLang(ulong hash, uint representation, uint unk)
        {
            var b = new LanguageEntry { _name = hash, ID = representation, Unknown = unk };

            this.Languages[representation] = b;
        }

        /// <summary>
        /// The id 2 language.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="LanguageEntry"/>.
        /// </returns>
        public LanguageEntry LanguageFromID(uint id)
        {
            if (this.Languages.ContainsKey(id))
            {
                return this.Languages[id];
            }

            return null;
        }

        /// <summary>
        /// The language hash 2 id.
        /// </summary>
        /// <param name="lang_hash">
        /// The hash.
        /// </param>
        /// <returns>
        /// The matched ID.
        /// </returns>
        public uint IDFromLanguageHash(ulong lang_hash)
        {
            foreach (var kvpair in this.Languages)
            {
                LanguageEntry entry = kvpair.Value;
                if (entry._name == lang_hash)
                    return kvpair.Key;
            }

            return 0;
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="language">
        /// The language.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        public void AddEntry(ulong extension, ulong path, uint language, uint id)
        {
            var b = new DatabaseEntry { _extension = extension, _path = path, Language = language, ID = id };
            this.Entries[id] = b;
        }

        public void AddEntry(DatabaseEntry entry)
        {
            this.Entries[entry.ID] = entry;
        }

        /// <summary>
        ///     The get id2Name Entries.
        /// </summary>
        public List<DatabaseEntry> GetDatabaseEntries()
        {
            List<DatabaseEntry> entries = new List<DatabaseEntry>();
            entries.AddRange(_entries.Values);
            return entries;
        }

        /// <summary>
        ///     The clear.
        /// </summary>
        public void Clear()
        {
            this.Entries.Clear();
            this.Languages.Clear();
        }

        /// <summary>
        /// The entry 2 id.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="extension">
        /// The extension.
        /// </param>
        /// <param name="language">
        /// The language.
        /// </param>
        /// <param name="checkLanguage">
        /// The check_language.
        /// </param>
        /// <returns>
        /// The <see cref="HashSet"/>.
        /// </returns>
        public HashSet<uint> IDFromEntry(ulong path, ulong extension, uint language, bool checkLanguage = false)
        {
            var foundItems = new HashSet<uint>();
            foreach (var kvpair in this.Entries)
            {
                DatabaseEntry entry = kvpair.Value;
                if (entry._path== path && entry._extension == extension)
                {
                    if (checkLanguage)
                    {
                        if (entry.Language == language)
                        {
                            foundItems.Add(kvpair.Key);
                        }

                        continue;
                    }

                    foundItems.Add(kvpair.Key);
                }
            }

            return foundItems;
        }

        /// <summary>
        /// The id 2 name.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="DatabaseEntry"/>.
        /// </returns>
        public DatabaseEntry EntryFromID(uint id)
        {
            if (this.Entries.ContainsKey(id))
            {
                return this.Entries[id];
            }

            return null;
        }

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Load(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fs))
                {
                    bool linux = false;
                    /*#if LINUX
						reader.BaseStream.Position += 8;
					#else*/
                    reader.BaseStream.Position += 4;
                    //#endif                
                    uint lang_count = reader.ReadUInt32();
                    if (lang_count == 0)
                    {
                        linux = true;
                        lang_count = reader.ReadUInt32();
                    }

                    reader.BaseStream.Position += 4;

                    uint lang_offset;

                    if (linux)
                    {
                        lang_offset = (uint)reader.ReadUInt64();
                        reader.BaseStream.Position += 24;
                    }
                    else {
                        lang_offset = reader.ReadUInt32();
                        reader.BaseStream.Position += 12;
                    }

                    uint file_entries_count = reader.ReadUInt32();
                    reader.BaseStream.Position += 4;

                    uint file_entries_offset;
                    if (linux)
                        file_entries_offset = (uint)reader.ReadUInt64();
                    else
                        file_entries_offset = reader.ReadUInt32();

                    //Languages
                    fs.Position = lang_offset;
                    try
                    {
                        for (int i = 0; i < lang_offset; ++i)
                        {
                            ulong language_hash = reader.ReadUInt64();
                            uint language_representation = reader.ReadUInt32();
                            uint language_unknown = reader.ReadUInt32();

                            this.AddLang(language_hash, language_representation, language_unknown);
                        }
                    }
                    catch (Exception exc)
                    {
						Console.WriteLine (exc.Message);
						Console.WriteLine (exc.StackTrace);
                        return false;
                    }

                    //File entries
                    fs.Position = file_entries_offset;
                    try
                    {
                        for (int i = 0; i < file_entries_count; ++i)
                        {
                            ulong ext = reader.ReadUInt64();
                            ulong fpath = reader.ReadUInt64();
                            uint language = reader.ReadUInt32();
                            uint u1 = reader.ReadUInt32();
                            uint id = reader.ReadUInt32();
                            uint u2 = reader.ReadUInt32();
                            this.AddEntry(ext, fpath, language, id);
                        }
                    }
                    catch (Exception exc)
                    {
						Console.WriteLine (exc.Message);
						Console.WriteLine (exc.StackTrace);
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}