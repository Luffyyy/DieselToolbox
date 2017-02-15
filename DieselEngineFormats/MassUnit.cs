using DieselEngineFormats.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DieselEngineFormats
{
    public class Vector3
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public Vector3(float? x = null, float? y = null, float? z = null)
        {
            this.X = x ?? 0f;
            this.Y = y ?? 0f;
            this.Z = z ?? 0f;
        }

    }

    public struct UnitPosition
    {
        public Vector3 Position { get; set; }

        public float[] Rotation { get; set; }

        public UnitPosition(BinaryReader br)
        {
            this.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            this.Rotation = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
        }
    }

    public struct MassUnitHeader
    {
        public Idstring Unit { get; set; }

        public uint Offset { get; set; }

        public uint InstanceCount { get; set; }

        public MassUnitHeader(BinaryReader br)
        {
            ulong UnitPathHash = br.ReadUInt64();
            this.Unit = HashIndex.Get(UnitPathHash);
            br.ReadSingle(); // Unknown.
            this.InstanceCount = br.ReadUInt32();
            br.ReadUInt32(); // Unknown
            this.Offset = br.ReadUInt32();
            br.BaseStream.Seek(8, SeekOrigin.Current);
        }
    }
    
    public class MassUnit
    {
        public Dictionary<Idstring, List<UnitPosition>> Instances { get; set; }

        public MassUnit()
        {
            this.Instances = new Dictionary<Idstring, List<UnitPosition>>();
        }

        public MassUnit(string filePath) : this()
        {
            this.Load(filePath);
        }

        public MassUnit(Stream fileStream) : this()
        {
            using (BinaryReader br = new BinaryReader(fileStream))
                this.ReadFile(br);
        }

        private void ReadFile(BinaryReader br)
        {
            uint unitCount = br.ReadUInt32();
            br.ReadUInt32(); // Unknown
            uint unitsOffset = br.ReadUInt32();
            br.BaseStream.Seek((long)unitsOffset, SeekOrigin.Begin);
            var headers = new List<MassUnitHeader>();
            for (int i = 0; i < unitCount; ++i)
            {
                headers.Add(new MassUnitHeader(br));
            }

            foreach (var header in headers)
            {
                var instances = new List<UnitPosition>();
                br.BaseStream.Seek(header.Offset, SeekOrigin.Begin);
                for (int i = 0; i < header.InstanceCount; ++i)
                {
                    instances.Add(new UnitPosition(br));
                }
                this.Instances[header.Unit] = instances;
            }
        }

        public void Load(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                    this.ReadFile(br);
            }
        }
    }
}
