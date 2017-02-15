namespace DieselEngineFormats.Bundle
{
    /// <summary>
    ///     The Language entry.
    /// </summary>
    public class LanguageEntry
    {
        #region Public Properties

        private Idstring _ids_name;

        public ulong _name;

        /// <summary>
        ///     Gets or sets the language entry hash.
        /// </summary>
        public Idstring Name {
            get
            {
                if (this._ids_name == null)
                    this._ids_name = HashIndex.Get(this._name);

                return this._ids_name;
            }
        }

        /// <summary>
        ///     Gets or sets the language entry Unknown value.
        /// </summary>
        public ulong Unknown { get; set; }

        public uint ID { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Return a string representation of a LanguageEntry.
        /// </summary>
        /// <returns>
        ///     The string representation of a Language entry.
        /// </returns>
        public override string ToString()
        {
            return this.Name.HashedString.ToString() + '\t' + this.Unknown.ToString();
        }

        #endregion
    }
}