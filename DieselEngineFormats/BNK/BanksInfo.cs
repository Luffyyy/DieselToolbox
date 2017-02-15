using DieselEngineFormats.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieselEngineFormats.BNK
{
    public class BanksInfo
    {
        public List<string> Soundbanks { get; set; } = new List<string>();

        public Dictionary<uint, Tuple<string, Idstring>> SoundLookups { get; set; } = new Dictionary<uint, Tuple<string, Idstring>>();

        public BanksInfo(string file)
        {
            using (FileStream str = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(str))
                {
                    this.ReadFile(br);
                }
            }

        }

        public BanksInfo(Stream str)
        {
            using (BinaryReader br = new BinaryReader(str))
            {
                this.ReadFile(br);
            }

        }

        protected Stack<long> savedPositions = new Stack<long>();

        protected BinaryReader br;

        private void SeekPop()
        {
            this.br.BaseStream.Seek(this.savedPositions.Pop(), SeekOrigin.Begin);
        }

        private void SeekPush()
        {
            this.savedPositions.Push(this.br.BaseStream.Position);
        }

        protected void ReadFile(BinaryReader _br)
        {
            br = _br;
            uint bnk_count = br.ReadUInt32();
            //Skip second count
            br.BaseStream.Position += 4;
            uint bnk_offset = br.ReadUInt32();
            uint section_pointer = br.ReadUInt32();
            uint unknown1 = br.ReadUInt32();

            uint sound_count = br.ReadUInt32();
            //Skip second count
            br.BaseStream.Position += 4;
            uint sound_offset = br.ReadUInt32();
            //Skips section pointer, unknown1, unknown2
            br.BaseStream.Position += 12;

            uint u_count = br.ReadUInt32();
            //Skip second count
            br.BaseStream.Position += 4;
            uint u_offset = br.ReadUInt32();

            br.BaseStream.Position = bnk_offset;

            for (int i = 0; i < bnk_count; i++)
            {
                br.BaseStream.Position += 4;
                uint position = br.ReadUInt32();
                this.SeekPush();
                br.BaseStream.Position = position;
                Soundbanks.Add(this.ReadString());
                this.SeekPop();
            }

            br.BaseStream.Position = sound_offset;

            Dictionary<ulong, uint> sound_lookups = new Dictionary<ulong, uint>();

            for (int i = 0; i < sound_count; i++)
            {
                uint id = (uint)br.ReadUInt64();
                ulong hash = br.ReadUInt64();
                if (sound_lookups.ContainsKey(hash))
                {
                    uint other_id = sound_lookups[hash];
                    continue;
                }

                sound_lookups.Add(hash, id);
            }

            br.BaseStream.Position = u_offset;

            for (int i = 0; i < u_count; i++)
            {
                ulong hash = br.ReadUInt64();
                br.BaseStream.Position += 4;
                uint string_pos = br.ReadUInt32();
                this.SeekPush();
                br.BaseStream.Position = string_pos;
                string str = this.ReadString();
                this.SeekPop();
                if (!sound_lookups.ContainsKey(hash))
                    continue;
                uint id = sound_lookups[hash];
                if (SoundLookups.ContainsKey(id))
                    continue;

                Idstring ids = HashIndex.Get(hash);

                SoundLookups.Add(id, new Tuple<string, Idstring>(id.ToString() != str ? str : null, ids));
            }
        }

        protected string ReadString()
        {
            string str = "";
            char c;
            while ((int)(c = br.ReadChar()) != 0)
            {
                str += c;
            }
            return str;
        }
    }
}
