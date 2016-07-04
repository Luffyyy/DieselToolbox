using DieselEngineFormats.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieselEngineFormats
{
    public class StringEntry
    {
        public uint StringPosition { get; set; }

        public Idstring ID { get; set; }

        public string Text { get; set; }

        public StringEntry(BinaryReader br)
        {
            br.ReadUInt64();
            ulong HashedID = br.ReadUInt64();
            br.ReadUInt32();
            StringPosition = br.ReadUInt32();

            ID = HashIndex.GetAny(HashedID) ?? new Idstring(HashedID);
        }
    }

    public class StringsFile
    {
        public List<StringEntry> LocalizationStrings = new List<StringEntry>();

        public List<StringEntry> ModifiedStrings = new List<StringEntry>();

        public StringsFile(string filepath)
        {
            using (FileStream str = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(str))
                {
                    try
                    {
                        this.Load(br);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }
                }
            }
        }

        public StringsFile(Stream str)
        {
            using (BinaryReader br = new BinaryReader(str))
            {
                try
                {
                    this.Load(br);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        public StringsFile(BinaryReader br)
        {
            try
            {
                this.Load(br);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public void Load(BinaryReader br)
        {
            br.BaseStream.Position = 0;

            br.ReadUInt32(); //Empty 4 bytes
            uint stringCount = br.ReadUInt32();
            br.ReadUInt32(); // String Count
            br.ReadUInt32(); //Unknown
            br.ReadUInt32(); //Unknown
            br.ReadUInt32(); //Unknown
            br.ReadUInt32(); //Unknown
            br.ReadUInt32(); //Unknown

            for (uint i = 0; i < stringCount; i++)
            {
                StringEntry strEntry = new StringEntry(br);

                if (strEntry.ID.HasUnHashed)
                    LocalizationStrings.Add(strEntry);
            }

            for (int i = 0; i < LocalizationStrings.Count; i++ )
            {
                StringEntry strEntry = LocalizationStrings[i];

                br.BaseStream.Position = strEntry.StringPosition;

                strEntry.Text = "";
                char ch;
                while ((int)(ch = br.ReadChar()) != 0)
                    strEntry.Text += ch;
            }
        }
    }
}
