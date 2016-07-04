using DieselEngineFormats.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DieselEngineFormats
{
    public struct UnitPosition
    {
        public float[] Position;

        public float[] Rotation;
    }

    public struct MassUnitHeader
    {
        public ulong UnitPathHash;

        public Idstring UnitPath;

        public uint Offset;

        public uint InstanceCount;
    }
    
    public class MassUnit
    {
        public Dictionary<Idstring, List<UnitPosition>> Instances = new Dictionary<Idstring, List<UnitPosition>>();

        public MassUnit(string filePath)
        {
            this.Load(filePath);
        }

        public MassUnit(Stream fileStream)
        {
            using (BinaryReader br = new BinaryReader(fileStream))
                this.ReadFile(br);
        }

        private void ReadFile(BinaryReader br)
        {
            var unitCount = br.ReadUInt32();
            br.ReadUInt32(); // Unknown purpose.
            var unitsOffset = br.ReadUInt32();
            br.BaseStream.Seek((long)unitsOffset, SeekOrigin.Begin);
            var headers = new List<MassUnitHeader>();
            for (int i = 0; i < unitCount; ++i)
            {
                var header = new MassUnitHeader();
                header.UnitPathHash = br.ReadUInt64();
				header.UnitPath = HashIndex.GetPath(header.UnitPathHash);
                br.ReadSingle(); // Unknown.
                header.InstanceCount = br.ReadUInt32();
                br.ReadUInt32(); // Unknown
                header.Offset = br.ReadUInt32();
                br.BaseStream.Seek(8, SeekOrigin.Current);
                headers.Add(header);
            }

            foreach (var header in headers)
            {
                if (header.UnitPath == null)
                {
                    Console.WriteLine("Massunit with id of {0:x} has no known path. Ignoring unit.", header.UnitPathHash);
                    continue;
                }
                var instances = new List<UnitPosition>();
                br.BaseStream.Seek(header.Offset, SeekOrigin.Begin);
                for (int i = 0; i < header.InstanceCount; ++i)
                {
                    var instance = new UnitPosition();
                    instance.Position = new float[3];
                    instance.Rotation = new float[4];

                    instance.Position[0] = br.ReadSingle();
                    instance.Position[1] = br.ReadSingle();
                    instance.Position[2] = br.ReadSingle();

                    instance.Rotation[0] = br.ReadSingle();
                    instance.Rotation[1] = br.ReadSingle();
                    instance.Rotation[2] = br.ReadSingle();
                    instance.Rotation[3] = br.ReadSingle();
                    instances.Add(instance);
                }
                this.Instances[header.UnitPath] = instances;
            }
        }

        public void Load(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    this.ReadFile(br);
                }
            }
        }
    }
}
