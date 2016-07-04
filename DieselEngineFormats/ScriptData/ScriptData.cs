namespace DieselEngineFormats.ScriptData
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ScriptData
    {
        #region Fields

        public dynamic Root;
        private readonly BinaryReader br;
        private readonly FileStream fs_copy;
        private readonly Stack<long> savedPositions = new Stack<long>();
        private int floatOffset;
        private int idstringOffset;
        private string path;
        private int quaternionOffset;
        private int stringOffset;
        private int tableOffset;
        private int vectorOffset;
        private bool linux = false;

        //Exports
        private int table_count = 1;
        private MemoryStream table_index = new MemoryStream();
        private Dictionary<long, long> table_offsets_fix = new Dictionary<long, long>();
        private List<float> floats = new List<float>();
        private List<string> strings = new List<string>();
        private List<float[]> vectors = new List<float[]>();
        private List<float[]> quaternions = new List<float[]>();
        private List<ulong> idstrings = new List<ulong>();
        private List<Dictionary<string, object>> tables = new List<Dictionary<string, object>>();


        #endregion

        #region Constructors and Destructors

		public ScriptData() { }

        public ScriptData(string path)
        {
            this.path =  path;
            using (fs_copy = new FileStream(this.path, FileMode.Open, FileAccess.Read))
            {
                this.br = new BinaryReader(fs_copy);
                this.ReadHeader();
                this.Root = this.ParseItem();
                this.br.Close();
            }
        }

        public ScriptData( BinaryReader bReader )
        {
            this.br = bReader;
            this.ReadHeader();
            this.Root = this.ParseItem();
            this.br.Close();
        }

        #endregion

        #region Methods

        private dynamic ParseItem()
        {
            uint item_type = this.br.ReadUInt32();
            int value = (int)item_type & 0xFFFFFF;
            item_type = (item_type >> 24) & 0xFF;

            switch (item_type)
            {
                case 0: //Nil
                    return null;
                case 1: //False
                    return false;
                case 2: //True
                    return true;
                case 3: //Number
                    return this.ReadFloat(value);
                case 4: //String
                    return this.ReadString(value);
                case 5: //Vector
                    return this.ReadVector(value);
                case 6: //Quaternion
                    return this.ReadQuaternion(value);
                case 7: //IdString
                    return this.ReadIdString(value);
                case 8: //Table
                    return this.ReadTable(value);
                default:
                    return null;
            }
        }

        private float ReadFloat(int index)
        {
            float return_float;
            this.SeekPush();
            this.Seek(this.floatOffset + index * 4);
            return_float = this.br.ReadSingle();
            this.SeekPop();
            return return_float;
        }

        private void ReadHeader()
        {
            uint tag = this.br.ReadUInt32();

            if (tag == 568494624)
                linux = true;

            if (!linux)
			    this.Seek(12);
            else
                this.Seek(16);

            this.floatOffset = this.ReadOffset();

            if (!linux)
                this.Seek(28);
            else
                this.Seek(40);

            this.stringOffset = this.ReadOffset();

            if (!linux)
                this.Seek(44);
            else
                this.Seek(64);

            this.vectorOffset = this.ReadOffset();

            if (!linux)
                this.Seek(60);
            else
                this.Seek(88);

            this.quaternionOffset = this.ReadOffset();

            if (!linux)
                this.Seek(76);
            else
                this.Seek(112);

            this.idstringOffset = this.ReadOffset();

            if (!linux)
                this.Seek(92);
            else
                this.Seek(136);

            this.tableOffset = this.ReadOffset();

            if (!linux)
                this.Seek(100);
            else
                this.Seek(152);
        }

        private int ReadOffset()
        {
            if (linux)
                return (int)this.br.ReadInt64();
            else
                return this.br.ReadInt32();
        }


        private ulong ReadIdString(int index)
        {
            ulong return_idstring;
            this.SeekPush();
            this.Seek(this.idstringOffset + index * 8);
            return_idstring = this.br.ReadUInt64();
            this.SeekPop();
            return return_idstring;
        }

        private float[] ReadQuaternion(int index)
        {
            
            var return_quaternion = new float[4];
            this.SeekPush();
            this.Seek(this.quaternionOffset + index * 16);
            return_quaternion[0] = this.br.ReadSingle();
            return_quaternion[1] = this.br.ReadSingle();
            return_quaternion[2] = this.br.ReadSingle();
            return_quaternion[3] = this.br.ReadSingle();
            this.SeekPop();
            return return_quaternion;
        }

        private string ReadString(int index)
        {
            string return_string = "";
            this.SeekPush();

            this.Seek(this.stringOffset + (index * (linux ? 16 : 8)) + (linux ? 8 : 4));
            int real_offset = this.ReadOffset();

            this.Seek(real_offset);
            var inchar = (char)this.br.ReadByte();
            while (inchar != '\0')
            {
                return_string += inchar;
                inchar = (char)this.br.ReadByte();
            }
            this.SeekPop();
            if (return_string.Contains("metadata"))
                Console.WriteLine();
            return return_string;
        }

        private Dictionary<string, object> ReadTable(int index)
        {
            var return_table = new Dictionary<string, object>();
			this.SeekPush ();

            this.Seek(this.tableOffset + (index * (linux ? 32 : 20)));
            int metatable_offset = this.ReadOffset();

            int item_count = this.br.ReadInt32();
            this.br.ReadInt32(); // Unknown : count
            int items_offset = this.ReadOffset();

            if (metatable_offset >= 0)
                return_table["_meta"] = ReadString(metatable_offset);
            for (int current_item = 0; current_item < item_count; ++current_item)
            {
                this.Seek(items_offset + (current_item * 8));
                dynamic key_item = this.ParseItem().ToString();
                dynamic value_item = this.ParseItem();
                return_table[key_item] = value_item;
            }
            this.SeekPop();
            return return_table;
        }

        private float[] ReadVector(int index)
        {
            var return_vector = new float[3];
            this.SeekPush();
            this.Seek(this.vectorOffset + index * 12);
            return_vector[0] = this.br.ReadSingle();
            return_vector[1] = this.br.ReadSingle();
            return_vector[2] = this.br.ReadSingle();
            this.SeekPop();
            return return_vector;
        }

        private void Seek(int offset)
        {
            this.br.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        private void SeekAdvance(int offset)
        {
            this.br.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        private void SeekPop()
        {
            this.br.BaseStream.Seek(this.savedPositions.Pop(), SeekOrigin.Begin);
            if (this.savedPositions.Count == 0)
                Console.WriteLine();
        }

        private void SeekPush()
        {
            this.savedPositions.Push(this.br.BaseStream.Position);
        }

        private void WriteTable(ref Dictionary<string, object> data, List<string> path, ref BinaryWriter bw)
        {
            Dictionary<string, object> currentData = data;
            if (path.Count > 0)
            {
                foreach(string p in path)
                    currentData = (Dictionary<string, object>)(currentData[p]);
            }

            int meta = -1;
            if (currentData.ContainsKey("_meta"))
                meta = strings.LastIndexOf((string)currentData["_meta"]);

            bw.Write(meta);
            bw.Write(currentData.Count - (meta > -1 ? 1 : 0));
            bw.Write(currentData.Count - (meta > -1 ? 1 : 0));
            table_offsets_fix.Add(bw.BaseStream.Position, table_index.Length);
            bw.Write((uint)( table_index.Length ));
            bw.Write(2037412641);

            BinaryWriter bwindex = new BinaryWriter(table_index);
            bwindex.Seek(0, SeekOrigin.End);
            long bwi_pos = bwindex.BaseStream.Position;
            byte[] buffer = new byte[currentData.Count*8];
            bwindex.Write(buffer);
            bwindex.BaseStream.Position = bwi_pos;

            Dictionary<String, Dictionary<string, object>> dicts_to_work = new Dictionary<String, Dictionary<string, object>>();

            foreach (KeyValuePair<string, object> keypair in currentData)
            {
                if (!keypair.Key.Equals("_meta"))
                {
                    //key
                    float floatparse;
                    ulong ulongparse;
                    if (float.TryParse(keypair.Key, out floatparse))
                    {
                        bwindex.Write((ushort)floats.LastIndexOf(floatparse));
                        bwindex.Write((byte)0);
                        bwindex.Write((byte)getItemType(floatparse));
                    }
                    else if (ulong.TryParse(keypair.Key, out ulongparse))
                    {
                        bwindex.Write((ushort)idstrings.LastIndexOf(ulongparse));
                        bwindex.Write((byte)0);
                        bwindex.Write((byte)getItemType(ulongparse));
                    }
                    else
                    {
                        bwindex.Write((ushort)strings.LastIndexOf(keypair.Key));
                        bwindex.Write((byte)0);
                        bwindex.Write((byte)getItemType(keypair.Key));
                    }

                    ulong tryp;
                    //value
                    if (keypair.Value is Dictionary<string, object>)
                    {
                        int data_index = table_count++;
                        bwindex.Write((ushort)(data_index));

                        long save_pos = bwindex.BaseStream.Position;

                        List<string> currPath = new List<string>(path);
                        currPath.Add(keypair.Key);
                        WriteTable(ref data, currPath, ref bw);

                        bwindex.BaseStream.Position = save_pos;
                    }
                    else if (keypair.Value is double)
                    {
                        bwindex.Write((ushort)floats.LastIndexOf(Convert.ToSingle((double)keypair.Value)));
                    }
                    else if (keypair.Value is float)
                    {
                        bwindex.Write((ushort)floats.LastIndexOf(Convert.ToSingle(keypair.Value)));
                    }
                    else if (keypair.Value is string)
                    {
                        if (strings.Contains(keypair.Value as string))
                            bwindex.Write((ushort)strings.LastIndexOf((string)keypair.Value));
                        else
                        {
                            float floattest;
                            if (float.TryParse((string)keypair.Value, out floattest))
                            {
                                bwindex.Write((ushort)floats.LastIndexOf(floattest));
                            }
                        }
                    }
                    else if (keypair.Value is Single[])
                    {
                        float[] temp = (float[])keypair.Value;
                        if (temp.Length == 3)
                            bwindex.Write((ushort)listVectorLastIndex(ref vectors, ref temp));
                        else if (temp.Length == 4)
                            bwindex.Write((ushort)listQuaternionLastIndex(ref quaternions, ref temp));
                    }
                    else if (ulong.TryParse(keypair.Value.ToString(), out tryp))
                    {
                        bwindex.Write((ushort)idstrings.LastIndexOf(tryp));
                    }
                    else if (keypair.Value is bool)
                    {
                        bwindex.Write((ushort)(Convert.ToInt32((bool)keypair.Value) + 1));
                    }
                    else
                    {
                        bwindex.Write((ushort)(0));
                    }

                    bwindex.Write((byte)0);
                    bwindex.Write((byte)getItemType(keypair.Value));
                }
            }

            foreach (KeyValuePair<string, Dictionary<string, object>> keypair in dicts_to_work)
            {
                List<string> currPath = new List<string>(path);
                currPath.Add(keypair.Key);
                WriteTable(ref data, currPath, ref bw);
            }
        }

        public void export(Dictionary<string, object> data, String filename)
        {
            floats = new List<float>();
            strings = new List<string>();
            vectors = new List<float[]>();
            quaternions = new List<float[]>();
            idstrings = new List<ulong>();
            tables = new List<Dictionary<string, object>>();

            organize(ref data, ref floats, ref strings, ref vectors, ref quaternions, ref idstrings, ref tables);

            strings.Remove("_meta");

            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                BinaryWriter bw = new BinaryWriter(fs);

                uint floatOffset = 0;
                uint stringOffset = 0;
                uint vectorOffset = 0;
                uint quaternionOffset = 0;
                uint idstringOffset = 0;
                uint tableOffset = 0;

                //Header
                bw.Write(2037412641);
                bw.Write(floats.Count);
                bw.Write(floats.Count);
                bw.Write(floatOffset);

                bw.Write(2037412641);
                bw.Write(strings.Count);
                bw.Write(strings.Count);
                bw.Write(stringOffset);

                bw.Write(2037412641);
                bw.Write(vectors.Count);
                bw.Write(vectors.Count);
                bw.Write(vectorOffset);

                bw.Write(2037412641);
                bw.Write(quaternions.Count);
                bw.Write(quaternions.Count);
                bw.Write(quaternionOffset);

                bw.Write(2037412641);
                bw.Write(idstrings.Count);
                bw.Write(idstrings.Count);
                bw.Write(idstringOffset);

                bw.Write(2037412641);
                bw.Write(tables.Count+1);
                bw.Write(tables.Count+1);
                bw.Write(tableOffset);

                //Body

                bw.Write(2037412641);
                bw.Write((ushort)0);
                bw.Write((byte)0);
                bw.Write((byte)8);

                //Write floats
                if (floats.Count > 0)
                {
                    floatOffset = (uint)fs.Position;
                    foreach (float number in floats)
                        bw.Write(number);
                }

                //Write strings
                if (strings.Count > 0)
                {
                    stringOffset = (uint)fs.Position;
                    int soffset = strings.Count * 8;
                    int accumulatedstringlegth = 0;

                    foreach (string text in strings)
                    {
                        bw.Write(2037412641);
                        bw.Write((uint)(stringOffset + soffset + accumulatedstringlegth));

                        accumulatedstringlegth += text.Length + 1;

                    }

                    foreach (string text in strings)
                    {
                        foreach (char c in text)
                        {
                            bw.Write(c);
                        }
                        bw.Write((byte)0);
                    }

                    for (int x = 0; x < (fs.Position % 4); x++)
                        bw.Write((byte)0);
                }

                //Write vectors
                if (vectors.Count > 0)
                {
                    vectorOffset = (uint)fs.Position;
                    foreach (float[] vector in vectors)
                    {
                        bw.Write(vector[0]);
                        bw.Write(vector[1]);
                        bw.Write(vector[2]);
                    }
                }

                //Write quaternions
                if (quaternions.Count > 0)
                {
                    quaternionOffset = (uint)fs.Position;
                    foreach (float[] quaternion in quaternions)
                    {
                        bw.Write(quaternion[0]);
                        bw.Write(quaternion[1]);
                        bw.Write(quaternion[2]);
                        bw.Write(quaternion[3]);
                    }
                }

                //Write idstrings
                if (idstrings.Count > 0)
                {
                    idstringOffset = (uint)fs.Position;
                    foreach (ulong idstring in idstrings)
                        bw.Write(idstring);
                }

                //Write tables
                if (tables.Count > 0)
                {
                    tableOffset = (uint)fs.Position;

                    List<string> path = new List<string>();
                    WriteTable(ref data, path, ref bw);

                    long item_offset = bw.BaseStream.Position;
                    foreach(KeyValuePair<long, long> pos in table_offsets_fix)
                    {
                        bw.BaseStream.Position = pos.Key;
                        bw.Write((uint)(item_offset + pos.Value));
                    }
                    bw.BaseStream.Position = item_offset;
                    bw.Write(table_index.ToArray());
                }

                bw.Write(1836436848);

                fs.Seek(12, SeekOrigin.Begin);
                bw.Write(floatOffset);
                fs.Seek(28, SeekOrigin.Begin);
                bw.Write(stringOffset);
                fs.Seek(44, SeekOrigin.Begin);
                bw.Write(vectorOffset);
                fs.Seek(60, SeekOrigin.Begin);
                bw.Write(quaternionOffset);
                fs.Seek(76, SeekOrigin.Begin);
                bw.Write(idstringOffset);
                fs.Seek(92, SeekOrigin.Begin);
                bw.Write(tableOffset);

            }
        }

        private int getItemType(object input)
        {
            ulong tryp;
            if (input is Dictionary<string, object>)
                return 8;
            else if (input is double || input is float)
                return 3;
            else if (input is string)
            {
                if (strings.Contains(input as string))
                    return 4;
                else
                {
                    float floattest;
                    if (float.TryParse(input as string, out floattest))
                    {
                        return 3;
                    }
                }
                return 4;
            }
            else if (input is float[] || input is Single[])
            {
                float[] temp = (float[])input;
                if (temp.Length == 3)
                    return 5;
                else if (temp.Length == 4)
                    return 6;
                else
                    return -1;
            }
            else if (ulong.TryParse(input.ToString(), out tryp) || input is ulong)
            {
                return 7;
            }
            else if (input is bool)
                return Convert.ToInt32((bool)input) + 1;
            else
                return 0;
        }

        private bool listVectorContains(ref List<float[]> vectors, ref float[] data)
        {
            foreach (float[] vector in vectors)
            {
                if (vector[0] == data[0] && vector[1] == data[1] && vector[2] == data[2])
                    return true;
            }
            return false;
        }

        private int listVectorLastIndex(ref List<float[]> vectors, ref float[] data)
        {
            foreach (float[] vector in vectors)
            {
                if (vector[0] == data[0] && vector[1] == data[1] && vector[2] == data[2])
                    return vectors.LastIndexOf(vector);
            }
            return -1;
        }

        private bool listQuaternionContains(ref List<float[]> quaternions, ref float[] data)
        {
            foreach (float[] vector in quaternions)
            {
                if (vector[0] == data[0] && vector[1] == data[1] && vector[2] == data[2] && vector[3] == data[3])
                    return true;
            }
            return false;
        }

        private int listQuaternionLastIndex(ref List<float[]> quaternions, ref float[] data)
        {
            foreach (float[] vector in quaternions)
            {
                if (vector[0] == data[0] && vector[1] == data[1] && vector[2] == data[2] && vector[3] == data[3])
                    return quaternions.LastIndexOf(vector);
            }
            return -1;
        }



        private bool listContains(ref List<Dictionary<string, object>> tables, ref Dictionary<string, object> data)
        {
            foreach (var item in tables)
            {
                if (item.Count == data.Count)
                {
                    bool broken = false;
                    foreach (KeyValuePair<string, object> keypair in item)
                    {
                        
                        if (!data.ContainsKey(keypair.Key))
                        {
                            broken = true;
                            break;
                        }

                        if (!compareObjects(keypair.Value, data[keypair.Key]))
                        {
                            broken = true;
                            break;
                        }
                    }
                    if(!broken)
                        return true;
                }
            }

            return false;
        }


        private int listLastIndex(ref List<Dictionary<string, object>> tables, ref Dictionary<string, object> data)
        {
            foreach (Dictionary<string, object> item in tables)
            {
                if (item.Count == data.Count)
                {
                    bool broken = false;
                    foreach (KeyValuePair<string, object> keypair in item)
                    {
                        if (!data.ContainsKey(keypair.Key))
                        {
                            broken = true;
                            break;
                        }

                        if (!compareObjects(keypair.Value, data[keypair.Key]))
                        {
                            broken = true;
                            break;
                        }

                    }
                    if (!broken)
                        return tables.IndexOf(item);
                }
            }

            return -1;
        }


        private bool compareObjects(object first, object second)
        {

            if (first is Dictionary<string, object> && second is Dictionary<string, object>)
            {
                Dictionary<string, object> firsttemp = (Dictionary<string, object>)first;
                Dictionary<string, object> secondtemp = (Dictionary<string, object>)second;

                if (firsttemp.Count != secondtemp.Count)
                {
                    return false;
                }
                else
                {

                    foreach (KeyValuePair<string, object> keypair in firsttemp)
                    {

                        if (secondtemp.ContainsKey(keypair.Key))
                        {
                            if (!compareObjects(firsttemp[keypair.Key], secondtemp[keypair.Key]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }

                    }

                    return true;

                }
                
            }
            else if ((first is double && second is double) || (first is float && second is float))
            {
                if (Convert.ToSingle(first) == Convert.ToSingle(second))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ((first is string && second is string))
            {
                if (((string)first).Equals((string)second))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ((first is Single[] && second is Single[]) || (first is float[] && second is float[]))
            {
                float[] firsttemp = (float[])first;
                float[] secondtemp = (float[])second;

                if (firsttemp.Length == secondtemp.Length)
                {
                    for (int x = 0; x < firsttemp.Length; x++)
                    {
                        if (firsttemp[x] != secondtemp[x])
                            return false;
                    }
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else if ((first is ulong && second is ulong))
            {
                if((ulong)first == (ulong)second)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ((first is bool && second is bool))
            {
                if ((bool)first == (bool)second)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


        private void organize(ref Dictionary<string, object> data, ref List<float> floats, ref List<string> strings, ref List<float[]> vectors, ref List<float[]> quaternions, ref List<ulong> idstrings, ref List<Dictionary<string, object>> tables)
        {
            bool organizeTable = false;   
            
            foreach (KeyValuePair<string, object> item in data)
            {

                if (item.Value is Dictionary<string, object>)
                {
                    Dictionary<string, object> temp = (Dictionary<string, object>)(item.Value);
                    organize(ref temp, ref floats, ref strings, ref vectors, ref quaternions, ref idstrings, ref tables);
                    if (!listContains(ref tables, ref temp) || !organizeTable)
                        tables.Add((Dictionary<string, object>)item.Value);
                }
                else if (item.Value is double)
                {
                    if (!floats.Contains(Convert.ToSingle((double)item.Value)))
                        floats.Add(Convert.ToSingle((double)item.Value));
                }
                else if (item.Value is Single)
                {
                    if (!floats.Contains(Convert.ToSingle(item.Value)))
                        floats.Add(Convert.ToSingle(item.Value));
                }
                else if (item.Value is string)
                {
                    float floattest;
                    if (float.TryParse((string)item.Value, out floattest))
                    {
                        if (!floats.Contains(floattest))
                            floats.Add(floattest);
                    }
                    if (!strings.Contains((string)item.Value))
                        strings.Add((string)item.Value);
                }
                else if (item.Value is Single[])
                {
                    float[] temp = (float[])item.Value;
                    if (temp.Length == 3 && !listVectorContains(ref vectors, ref temp))
                        vectors.Add(temp);
                    else if (temp.Length == 4 && !listQuaternionContains(ref quaternions, ref temp))
                        quaternions.Add(temp);
                }
                else if (item.Value is UInt64)
                {
                    if (!idstrings.Contains((ulong)item.Value))
                        idstrings.Add((ulong)item.Value);
                }
                else if (item.Value is bool)
                {
                }
                else
                {
                    ulong tryp;
                    if (ulong.TryParse(item.Value.ToString(), out tryp))
                    {
                        if (!idstrings.Contains(tryp))
                            idstrings.Add(tryp);
                    }
                    else
                    {
                        Console.WriteLine(item.Value.GetType());
                    }
                }

                float floatparse;
                ulong ulongparse;
                if (float.TryParse(item.Key, out floatparse))
                {
                    if (!floats.Contains(floatparse))
                        floats.Add(floatparse);
                }
                else if (ulong.TryParse(item.Key, out ulongparse))
                {
                    if (!idstrings.Contains(ulongparse))
                        idstrings.Add(ulongparse);
                }
                else
                {
                    if (!strings.Contains(item.Key))
                        strings.Add(item.Key);
                }
            }
        }
        #endregion
    }
}