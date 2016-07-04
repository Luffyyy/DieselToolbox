using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace DieselEngineFormats.ScriptData {
	
	public class CustomXMLNode : ScriptDataNode
	{
		public CustomXMLNode() : base() { }

		public CustomXMLNode(string meta, string data, object index, int indent = 0) : base(meta, data, index, indent) { }

		public CustomXMLNode(string meta, float[] data, object index, int indent = 0) : base(meta, data, index, indent) { }

		public CustomXMLNode(string meta, float data, object index, int indent = 0) : base(meta, data, index, indent) { }

		public CustomXMLNode(string meta, bool data, object index, int indent = 0) : base(meta, data, index, indent) { }

		public CustomXMLNode(string meta, Dictionary<string, object> data, object index, int indent = 0) : base(meta, data, index, indent) { }

		public CustomXMLNode(StreamReader data) : base(data) { }

		public string ToString(int i = 0, bool escape = false)
		{
			StringBuilder indentation = new StringBuilder();
			for (int j = 0; j < this.indent; j++)
				indentation.Append("\t");

			StringBuilder sb = new StringBuilder();
            //Remove space characters as they are invalid in xml
			sb.Append(indentation.ToString() + "<" + this.meta.Replace(" ", "-"));

			if (!index.Equals(-1))
			{
				//sb.Append(" index=\"" + this.index + "\"");
			}

			foreach (KeyValuePair<string, object> kvp in this.attributes)
			{
                if (kvp.Value is float[])
                {
                    sb.Append(" " + kvp.Key + "=\"");

                    foreach (float f in (kvp.Value as float[]))
                        sb.Append(f + " ");

                    sb.Remove(sb.Length - 1, 1);

                    sb.Append("\"");

                }
                else if (kvp.Value is bool)
                    sb.Append(" " + kvp.Key + "=\"" + ((bool)kvp.Value ? "true" : "false") + "\"");
                else
                {
                    sb.Append(" " + kvp.Key + "=\"" + (kvp.Value is string && escape ? SecurityElement.Escape(kvp.Value as string) : kvp.Value) + "\"");
                }
			}

			if (this.children.Count > 0)
			{
				sb.Append(">\r\n");
				//foreach (ScriptDataNode child in this.children)
				for (int j = 0; j < this.children.Count; j++)
					sb.Append((this.children[j] as CustomXMLNode).ToString(j, escape));

                //Remove space characters as they are invalid in xml
                sb.Append(indentation.ToString() + "</" + this.meta.Replace(" ", "-") + ">\r\n");
			}
			else
			{
				if (!String.IsNullOrWhiteSpace(this.data))
				{
					if (this.index is int && (int)this.index != i + 1)
						sb.Append(" index=\"" + this.index + "\"");
                    sb.Append(" value=\"" + (escape ? SecurityElement.Escape(this.data) : this.data) + "\"/>\r\n");
				}
				else
				{
					sb.Append("/>\r\n");
				}
			}

			return sb.ToString();
		}

		public override string ToString()
		{
			return this.ToString(0);
		}

		public override void FromString(StreamReader data)
		{
			bool closed = false;

			string line = data.ReadLine();
			int line_pos = 0;

			if (String.IsNullOrWhiteSpace(line))
				return;

			for (; line_pos < line.Length; line_pos++)
			{
				if (line[line_pos] == '<')
				{
					line_pos++;
					break;
				}
			}

			if (!closed && line_pos < line.Length && line[line_pos] == '/')
			{
				closed = true;
				line_pos++;
			}

			StringBuilder meta = new StringBuilder();

			for (; line_pos < line.Length; line_pos++)
			{
				if (line[line_pos] != ' ' && line[line_pos] != '=' && line[line_pos] != '/' && line[line_pos] != '>')
				{
					meta.Append(line[line_pos]);
				}
				else
				{
					line_pos++;
					break;
				}
			}

			this.meta = meta.ToString();

			if (line_pos < line.Length)
			{
				StringBuilder attrib_name = new StringBuilder();
				StringBuilder attrib_data = new StringBuilder();

				for (; line_pos < line.Length; line_pos++)
				{
					if (line[line_pos] == '/' || line[line_pos] == '>')
					{
						break;
					}

					if (line[line_pos] == ' ')
					{
						continue;
					}

					for (; line_pos < line.Length; line_pos++)
					{
						if (line[line_pos] != '=')
						{
							attrib_name.Append(line[line_pos]);
						}
						else
						{
							line_pos++;
							break;
						}
					}

					if (line[line_pos] == '"')
						line_pos++;

					for (; line_pos < line.Length; line_pos++)
					{
						if (line[line_pos] != '"')
						{
							attrib_data.Append(line[line_pos]);
						}
						else
						{
							break;
						}
					}

					Boolean data_bool;
					if (Boolean.TryParse(attrib_data.ToString(), out data_bool))
					{
						this.attributes.Add(attrib_name.ToString(), data_bool);
						attrib_name.Clear();
						attrib_data.Clear();
						continue;
					}

					float data_float;
					if (float.TryParse(attrib_data.ToString(), out data_float))
					{
						this.attributes.Add(attrib_name.ToString(), data_float);
						attrib_name.Clear();
						attrib_data.Clear();
						continue;
					}

					List<float> data_floats = new List<float>();
					bool isFloatArray = true;
					if (attrib_data.ToString().Split(' ').Length > 1)
					{
						string[] splits = attrib_data.ToString().Split(' ');

						foreach (String spl in splits)
						{
							float test_out;
							if (float.TryParse(spl, out test_out))
							{
								data_floats.Add(test_out);
							}
							else
							{
								isFloatArray = false;
							}

						}

						if (isFloatArray)
						{
							this.attributes.Add(attrib_name.ToString(), data_floats.ToArray());
							attrib_name.Clear();
							attrib_data.Clear();
							continue;
						}
					}

					this.attributes.Add(attrib_name.ToString(), attrib_data.ToString());
					attrib_name.Clear();
					attrib_data.Clear();

				}
			}

			if (!closed && line_pos < line.Length && line[line_pos] == '/')
			{
				closed = true;
				line_pos++;
			}

			if (!closed)
			{
				CustomXMLNode child;
				while (!((child = new CustomXMLNode(data)).meta).Equals(this.meta))
					this.children.Add(child);
			}
		}
	}
}
