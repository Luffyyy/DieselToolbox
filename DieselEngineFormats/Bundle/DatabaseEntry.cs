// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameEntry.cs" company="Zwagoth">
//   This code is released into the public domain by Zwagoth.
// </copyright>
// <summary>
//   The name entry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DieselEngineFormats.Bundle
{
    /// <summary>
    ///     The name entry.
    /// </summary>
    public class DatabaseEntry
    {
        #region Public Properties

        private Idstring _ids_extension;

        public ulong _extension;

        /// <summary>
        ///     Gets or sets name entry extension hash.
        /// </summary>
        public Idstring Extension
        {
            get
            {
                if (_ids_extension == null)
                    _ids_extension = HashIndex.GetExtension(_extension);

                return _ids_extension;
            }
        }

        /// <summary>
        ///     Gets or sets the name entry language ids.
        /// </summary>
        public uint Language { get; set; }

        private Idstring _ids_path;

        public ulong _path;

        /// <summary>
        ///     Gets or sets the name entry path hash.
        /// </summary>
        public Idstring Path { get {
                if (_ids_path == null)
                    _ids_path = HashIndex.GetPath(_path);

                return _ids_path;
            } }

        /// <summary>
        ///     Gets or sets the ID of the entry
        /// </summary>
        public uint ID { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Return a string representation of a NameEntry.
        /// </summary>
        /// <returns>
        ///     The string representation of a name entry.
        /// </returns>
        public override string ToString()
        {
            return this.Path.HashedString + '.' + this.Language.ToString("x") + '.' + this.Extension.HashedString;
        }

        #endregion
    }
}