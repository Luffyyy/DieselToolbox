// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BundleHeader.cs" company="Zwagoth">
//   This code is released into the public domain by Zwagoth.
// </copyright>
// <summary>
//   The bundle entry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using DieselEngineFormats.Utils;
using System.Globalization;


namespace DieselEngineFormats.Bundle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    public class BundleFooterEntry
    {
        public Idstring Extension { get; set; }

        public Idstring Path { get; set; }

		public PackageHeader Parent { get; set; }

        public BundleFooterEntry(BinaryReader br)
        {
            Extension = HashIndex.GetExtension(br.ReadUInt64());
            Path = HashIndex.GetPath(br.ReadUInt64());
        }
    }
    

    /// <summary>
    ///     The bundle header.
    /// </summary>
    /// 
    public class PackageHeader
    {
        #region Fields

        /// <summary>
        ///     The has length field.
        /// </summary>
        public bool HasLengthField = false;

        private BundleHeaderConfig _config = Defs.PD2Config;

        public BundleHeaderConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
            }
        }

        /// <summary>
        /// The _entries.
        /// </summary>
        private List<BundleFileEntry> _entries = new List<BundleFileEntry>();

        /// <summary>
        /// The _references.
        /// </summary>
        private List<BundleFooterEntry> _references = new List<BundleFooterEntry>();

		private Idstring _name;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets list of bundle file entries
        /// </summary>
        public List<BundleFileEntry> Entries
        {
            get
            {
                return this._entries;
            }
        }

        /// <summary>
        ///     Gets list of reference entries
        /// </summary>
        public List<BundleFooterEntry> References
        {
            get
            {
                return this._references;
            }
        }

        /// <summary>
        ///     Gets or sets
        /// </summary>
        public byte[] Footer { get; set; }

        /// <summary>
        ///     The header.
        /// </summary>
        public List<uint> Header { get; set; }

        public Idstring Name { 
			get { return _name; } 
			set { _name = value; } 
		}

        #endregion

        #region Public Methods and Operators

		public PackageHeader () { }

		public PackageHeader (string bundleId)
		{
			this.Load(bundleId);
		}

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="bundleId">
        /// The bundle id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Load(string bundle_path)
        {
            if (!File.Exists(bundle_path))
            {
                Console.WriteLine("Bundle header file does not exist.");
                return false;
            }
			this._name = (Idstring)General.BundleNameToPackageID (Path.GetFileNameWithoutExtension (bundle_path).Replace("_h", "")).Clone();
            this._name.SwapEdianness();

            try
            {
                using (var fs = new FileStream(bundle_path, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        bool confSet = false;

                        this.Header = new List<uint>{
                            br.ReadUInt32(), //ref offset
							br.ReadUInt32(), //tag / count
							br.ReadUInt32(), //linux:tag / count
 							br.ReadUInt32(), // offset / count
						};
                        if (Header[1] != Header[2]) this.Header.Add(br.ReadUInt32());

                        uint refOffset = this.Header[0];

                        uint itemCount = 0, offset = 0;

                        for (int i = 1; i < (Header.Count - 1); i++)
                        {
                            if (Header[i] == Header[i + 1])
                            {
                                itemCount = Header[i];
                                if (Header.Count <= i + 2)
                                {
                                    Header.Add(br.ReadUInt32());
                                }
                                offset = Header[i + 2];
                                if (i != 1)
                                {
                                    if (Header[1] == 0)
                                    {
                                        offset += 4;
                                    }
                                    else
                                        this.HasLengthField = true;
                                }

                                break;
                            }
                        }

                        if (br.BaseStream.Position < offset)
                        {
                            uint amount = ((offset - (uint)br.BaseStream.Position) / 4);
                            for (uint i = 0; i < amount; i++)
                                this.Header.Add(br.ReadUInt32());
                        }

                        if (offset == 0)
                            offset = refOffset - 4;

						br.BaseStream.Position = offset;

                        uint entryTag = br.ReadUInt32();

                        if (!confSet)
                        {
                            List<BundleHeaderConfig> configs = Defs.ConfigLookup.FindAll(conf => this.HasLengthField ? conf.EntryStartLengthTag.Equals(entryTag) : conf.EntryStartTag.Equals(entryTag));
                            if (configs.Count == 1)
                            {
                                this.Config = configs[0]; 
                                
                                confSet = true;
                            }
                        }

                        for (int i = 0; i < itemCount; ++i)
                        {
                            var be = new BundleFileEntry(br, this.HasLengthField) { Parent = this };

                            this._entries.Add(be);

                            if (this.HasLengthField || i <= 0)
                                continue;

                            BundleFileEntry pbe = this._entries[i - 1];
							pbe.Length = (int)(be.Address - pbe.Address);
                        }

                        if (itemCount > 0 && !this.HasLengthField)
                        {
							string bundleFile;
							if (!File.Exists(bundleFile = bundle_path.Replace("_h", "")))
                                this._entries[this._entries.Count - 1].Length = -1;
                            else
                            {
                                if (bundleFile == bundle_path)
                                    this._entries[this._entries.Count - 1].Length = (int)(((uint)fs.Length) - this._entries[this._entries.Count - 1].Address);
                                else
                                {
                                    long length = new System.IO.FileInfo(bundleFile).Length;
                                    this._entries[this._entries.Count - 1].Length = (int)(((uint)length) - this._entries[this._entries.Count - 1].Address);
                                }
                            }
                        }

                        //Footer breakdown
                        /*
                         * uint32 - tag
                         * uint32 - section size
                         * uint32 - count
                         * uint32 - unknown
                         * uint32 - unknown
                         * uint32 - tag?
                         * foreach (count):
                         *  uint64 - hash (extension)
                         *  uint64 - hash (path)
                         * uint32 - end?
                         * uint32 (0) - end
                        */
                        uint sectag = br.ReadUInt32();

                        if (sectag.Equals(this.Config.ReferenceStartTag))
                            this.ReadFooter(br);

                        //this.Footer = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                        //uint val = Convert.ToUInt32(this.Footer[0]);

                        //if (confSet)
                            //Console.WriteLine("Bundle Config detected!");
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);

                return false;
            }

            return true;
        }

        public void ReadFooter(BinaryReader br)
        {
            //Footer breakdown
            /*
             * uint32 - tag
             * uint32 - section size
             * uint32 - count
             * uint32 - unknown
             * uint32 - unknown
             * uint32 - tag?
             * foreach (count):
             *  uint64 - hash (extension)
             *  uint64 - hash (path)
             * uint32 - end?
             * uint32 (0) - end
            */
            uint size = br.ReadUInt32();
            uint count = br.ReadUInt32();
            uint alloc_size = br.ReadUInt32();
            uint entry_size = br.ReadUInt32();
            uint tag = br.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                this._references.Add(new BundleFooterEntry(br));
            }

        }

        /// <summary>
        /// Write the header contents to the BinaryWriter 'writer', with the default/detected config
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void Write(BinaryWriter writer)
        {
            this.Write(writer, this.Config);
        }

        /// <summary>
        /// Write the header contents to the BinaryWriter 'writer', with the passed 'config'
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="config">
        /// The config for writing the Header
        /// </param>
        public void Write(BinaryWriter writer, BundleHeaderConfig config)
        {
            writer.Write((uint)((this.HasLengthField ? 7 + this.Entries.Count * 3 : 5 + this.Entries.Count * 2) * 4));

            if (this.HasLengthField)
                writer.Write(config.HeaderStartLengthTag);
            else if (config.HeaderStartEmptyTag)
                writer.Write((uint)0);

            writer.Write(this.Entries.Count);
            writer.Write(this.Entries.Count);

            writer.Write(this.HasLengthField ? 24 : 16);

            if (this.HasLengthField)
            {
                writer.Write(config.HeaderEndLengthTag);
                writer.Write(config.EntryStartLengthTag);
            }
            else
                writer.Write(config.EntryStartTag);

            foreach (BundleFileEntry entry in this.Entries)
            {
                entry.WriteEntry(writer, this.HasLengthField);
            }

            writer.Write(this.Footer);

            if (config.EmptyEndInt && this.Footer[this.Footer.Length - 1] != 0)
                writer.Write((uint)0);
        }

        /// <summary>
        ///     Sort list of bundle file entries by Id
        /// </summary>
        public void SortEntriesId()
        {
            int oldcount = _entries.Count();
            _entries = _entries.OrderBy(o => o.ID).ToList();
            if (oldcount != _entries.Count())
                Console.WriteLine();
        }

        /// <summary>
        ///     Sort list of bundle file entries by Address
        /// </summary>
        public void SortEntriesAddress()
        {
            int oldcount = _entries.Count();
            _entries = _entries.OrderBy(o => o.Length).ToList(); // Order by length, so 0 is always first
            _entries = _entries.OrderBy(o => o.Address).ToList(); //Order by address
            if (oldcount != _entries.Count())
                Console.WriteLine();
        }

        #endregion
    }

    public struct BundleHeaderConfig
    {
        public bool HeaderStartEmptyTag { get; set; }

        public uint HeaderStartLengthTag { get; set; }

        public uint HeaderEndLengthTag { get; set; }

        public uint EntryStartLengthTag { get; set; }

        public uint EntryStartTag { get; set; }

        public uint ReferenceStartTag { get; set; }

        public uint EndTag { get; set; }

        public bool EmptyEndInt { get; set; }
    }
}