using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace DieselEngineFormats.Font
{
    public class FontKerning
    {
        public int char1, char2;
        public byte u1, u2, u3, u4;

        public FontKerning(BinaryReader instream)
        {
            this.char1 = instream.ReadInt32();
            this.char2 = instream.ReadInt32();
            this.u1 = instream.ReadByte();
            this.u2 = instream.ReadByte();
            this.u3 = instream.ReadByte();
            this.u4 = instream.ReadByte();
        }

        public void WriteStream(BinaryWriter bw)
        {
            bw.Write(this.char1);
            bw.Write(this.char2);
            bw.Write(this.u1);
            bw.Write(this.u2);
            bw.Write(this.u3);
            bw.Write(this.u4);
        }
    }

    public class FontCharacter
    {

        public int ID { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public byte W { get; set; }

        public byte H { get; set; }

        public byte XAdvance { get; set; }

        public sbyte XOffset { get; set; }

        public sbyte YOffset { get; set; }

        public int ASCIICode { get; set; }

        public char Character { get { return (char)this.ASCIICode; } set { this.ASCIICode = (int)value; } }

        public object Tag { get; set; }

        public FontCharacter() {}

        public FontCharacter(BinaryReader instream)
        {
            instream.ReadByte();
            this.W = instream.ReadByte();
            this.H = instream.ReadByte();
            this.XAdvance = instream.ReadByte();
            this.XOffset = instream.ReadSByte();
            this.YOffset = instream.ReadSByte();
            this.X = instream.ReadInt16();
            this.Y = instream.ReadInt16();
        }

        public void WriteMainSection(BinaryWriter bw)
        {
            bw.Write((byte)0);
            bw.Write(this.W);
            bw.Write(this.H);
            bw.Write(this.XAdvance);
            bw.Write(this.XOffset);
            bw.Write(this.YOffset);
            bw.Write(this.X);
            bw.Write(this.Y);
        }

        public void WriteCodeIDSection(BinaryWriter bw)
        {
            bw.Write(this.ASCIICode);
            bw.Write(this.ID);
        }
    }

    public class DieselFont
    {
        public List<FontKerning> kerning = new List<FontKerning>();

        public bool HasKerning { get { return this.kerning.Count > 0; } }

        public int TextureWidth, TextureHeight;

        public string Name { get; set; }

        private const int EndTag = 925913978;

        public int u1, u2, u3, u5, LineHeight, u7, Info_Size;

        public short u4, Common_Base;

        private List<FontCharacter> _characters = new List<FontCharacter>();

        public List<FontCharacter> Characters { 
            get 
            {
                this._characters.Sort(delegate(FontCharacter x, FontCharacter y)
                {
                    return x.ASCIICode.CompareTo(y.ASCIICode);
                });
                return this._characters; 
            }
            set 
            {
                this._characters = value;
            }
        }

        public DieselFont() {}

        public DieselFont(XmlReader xmlReader)
        {
            this.ReadBMFontXmlFile(xmlReader);
        }

        public DieselFont(string filepath)
        {
            using (FileStream str = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(str))
                {
                    try
                    {
                        this.ReadDieselFile(br);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }
                }
            }
        }

        public DieselFont(Stream str)
        {
            using (BinaryReader br = new BinaryReader(str))
            {
                try
                {
                    this.ReadDieselFile(br);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        public DieselFont(BinaryReader br)
        {
            try
            {
                this.ReadDieselFile(br);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        
        #region BMFont
        public void ReadBMFontXmlFile(XmlReader readXML)
        {
            while (readXML.Read())
            {
                if (readXML.NodeType == XmlNodeType.Element)
                {
                    switch(readXML.Name)
                    {
                        case "info":
                            this.Name = readXML.GetAttribute("face");
                            this.Info_Size = int.Parse(readXML.GetAttribute("size"));
                            break;
                        case "common":
                            this.LineHeight = int.Parse(readXML.GetAttribute("lineHeight"));
                            this.Common_Base = short.Parse(readXML.GetAttribute("base"));
                            this.TextureWidth = int.Parse(readXML.GetAttribute("scaleW"));
                            this.TextureHeight = int.Parse(readXML.GetAttribute("scaleH"));
                            break;
                        case "char":
                            FontCharacter fontChar = new FontCharacter{
                                ASCIICode = int.Parse(readXML.GetAttribute("id")),
                                ID = this.Characters.Count,
                                X = short.Parse(readXML.GetAttribute("x")),
                                Y = short.Parse(readXML.GetAttribute("y")),
                                W = byte.Parse(readXML.GetAttribute("width")),
                                H = byte.Parse(readXML.GetAttribute("height")),
                                XOffset = sbyte.Parse(readXML.GetAttribute("xoffset")),
                                YOffset = sbyte.Parse(readXML.GetAttribute("yoffset")),
                                XAdvance = byte.Parse(readXML.GetAttribute("xadvance")),
                            };
                            this.Characters.Add(fontChar);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Diesel
        public void ReadDieselFile(BinaryReader br)
        {
            br.BaseStream.Position = 0;

            int charCount = br.ReadInt32();//Count
            br.ReadInt32();//Count

            int fontRectPos = br.ReadInt32();

            this.u1 = br.ReadInt32();//Unknown
            this.u2 = br.ReadInt32();//Unknown

            br.ReadInt32();//Count
            br.ReadInt32();//Count

            int asciiCharsPos = br.ReadInt32();

            br.ReadInt32();//Unknown, Same as L52
            this.u3 = br.ReadInt32();//Unknown
            this.u4 = br.ReadInt16();//Unknown Short at end seems to be base
            this.Common_Base = br.ReadInt16();

            int kernings = br.ReadInt32();
            br.ReadInt32();

            int kerningPos = br.ReadInt32();

            bool hasKerning = kerningPos != 0;

            br.ReadInt32();//Unknown, same as L52
            this.u5 = br.ReadInt32();//Unknown
            br.ReadInt32();//Unknown

            int endPos = br.ReadInt32();

            int kernCount = (endPos - kerningPos) / 12;

            this.LineHeight = br.ReadInt32(); //Line Height

            this.TextureWidth = br.ReadInt32();

            this.TextureHeight = br.ReadInt32();

            this.u7 = br.ReadInt32(); //Sometimes the same as LineHeight

            this.Info_Size = br.ReadInt32(); //base?

            if (br.BaseStream.Position != fontRectPos)
            {
                Console.WriteLine("Position is not correct for Font Texture Rectangle by " + (br.BaseStream.Position - fontRectPos));
                br.BaseStream.Position = fontRectPos;
            }

            for (int i = 0; i < charCount; i++)
            {
                this._characters.Add(new FontCharacter(br));
            }

            if ((charCount % 2) != 0)
                br.ReadInt16();

            if (br.BaseStream.Position != asciiCharsPos)
            {
                Console.WriteLine("Position is not correct for ASCII Characters by " + (br.BaseStream.Position - asciiCharsPos));
                br.BaseStream.Position = asciiCharsPos;
            }

            for (int i = 0; i < charCount; i++)
            {
                int ASCIICode = br.ReadInt32();
                int ID = br.ReadInt32();
                this._characters[ID].ID = ID;
                this._characters[ID].ASCIICode = ASCIICode;
            }

            if (hasKerning)
            {
                for (int i = 0; i < kernCount; i++)
                {
                    this.kerning.Add(new FontKerning(br));
                }
            }

            if (br.BaseStream.Position != endPos)
            {
                Console.WriteLine("End Position isn't correct");
                br.BaseStream.Position = endPos;
            }

            this.Name = "";

            char ch;
            while ((int)(ch = br.ReadChar()) != 0)
                this.Name += ch;

            br.ReadBytes((int)((br.BaseStream.Length - 4) - br.BaseStream.Position));
            br.ReadInt32(); //End Tag
        }

        public void WriteDieselData(BinaryWriter bw)
        {
            int charCount = this._characters.Count;
            bw.Write(charCount);
            bw.Write(charCount);

            //font rect pos
            bw.Write(92);

            bw.Write(this.u1);
            bw.Write(this.u2);

            bw.Write(charCount);
            bw.Write(charCount);

            //asciiID pos
            bw.Write(92 + (charCount * 10) + ((charCount % 2) != 0 ? 2 : 0));

            bw.Write(this.u1);
            bw.Write(this.u3);
            bw.Write(this.u4);
            bw.Write(this.Common_Base);

            bw.Write(this.kerning.Count);
            bw.Write(this.kerning.Count);

            //Kerning pos
            bw.Write((this.HasKerning ? 92 + (charCount * 18) : 0) + ((charCount % 2) != 0 ? 2 : 0));

            bw.Write(this.u1);
            bw.Write(this.u5);
            bw.Write(this.u1);

            //end pos
            bw.Write(92 + (charCount * 18) + (this.kerning.Count * 12) + ((charCount % 2) != 0 ? 2 : 0));

            bw.Write(this.LineHeight);
            bw.Write(this.TextureWidth);
            bw.Write(this.TextureHeight);
            bw.Write(this.u7 != 0 ? this.LineHeight : this.u7);
            bw.Write(this.Info_Size);

            this._characters.Sort(delegate(FontCharacter x, FontCharacter y)
            {
                return x.ID.CompareTo(y.ID);
            });

            foreach(FontCharacter fontChar in this._characters)
            {
                fontChar.WriteMainSection(bw);
            }

            if ((charCount % 2) != 0)
                bw.Write((short)0);

            this._characters.Sort(delegate(FontCharacter x, FontCharacter y)
            {
                return x.ASCIICode.CompareTo(y.ASCIICode);
            });

            foreach (FontCharacter fontChar in this._characters)
            {
                fontChar.WriteCodeIDSection(bw);
            }

            foreach (FontKerning fontKern in this.kerning)
            {
                fontKern.WriteStream(bw);
            }
            
            foreach (char nameChar in this.Name)
            {
                bw.Write(nameChar);
            }
            bw.Write((byte)0);
            while (bw.BaseStream.Position % 4 != 0)
                bw.Write((byte)0);

            bw.Write(DieselFont.EndTag);
        }
        #endregion
    }
}
