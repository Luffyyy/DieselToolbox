namespace DieselEngineFormats.Bundle
{
    using System.IO;

    /// <summary>
    /// The bundle entry.
    /// </summary>
    public class PackageFileEntry
    {
        #region Fields

        /// <summary>
        /// Gets or sets the added of the bundle entry in the bundle file. Offset starts at beginning of file.
        /// </summary>
        public uint Address { get; set; }

        /// <summary>
        /// Gets or sets the entries name index id;
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Gets or sets the entries file length.
        /// </summary>
        public int Length { get; set; }

		public string FileSize{
			get {
				long bytesNo = this.Length;

				if (bytesNo < 1024)
					return bytesNo.ToString () + " B";
				else
					return string.Format ("{0:n0}", bytesNo / 1024) + " KB";
			}
		}

		public PackageHeader Parent { get; set; }

		public Idstring PackageName {get { return this.Parent?.Name; }}

        #endregion

        #region Public Methods and Operators

        public PackageFileEntry(BinaryReader br, bool has_length = false)
        {
            this.ReadEntry(br, has_length);
        }

        /// <summary>
        /// The read entry.
        /// </summary>
        /// <param name="br">
        /// The br.
        /// </param>
        /// <param name="readLength">
        /// The read length.
        /// </param>
        public void ReadEntry(BinaryReader br, bool readLength = false)
        {
            this.ID = br.ReadUInt32();
            this.Address = br.ReadUInt32();
            if (readLength)
            {
                this.Length = br.ReadInt32();
            }
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("BundleEntry(id: {0} address: {1} length: {2})", this.ID, this.Address, this.Length);
        }

        /// <summary>
        /// Write a bundle entry to a output stream.
        /// </summary>
        /// <param name="writer">
        /// Binary writer to write the entry to.
        /// </param>
        /// <param name="writeLength">
        /// If this entry should include a length field.
        /// </param>
        public void WriteEntry(BinaryWriter writer, bool writeLength = false)
        {
            writer.Write(this.ID);
            writer.Write(this.Address);
            if (writeLength)
            {
                writer.Write(this.Length);
            }
        }

        #endregion
    }
}
