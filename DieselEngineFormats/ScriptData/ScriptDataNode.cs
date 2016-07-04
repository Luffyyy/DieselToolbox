using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DieselEngineFormats.ScriptData
{
    public abstract class ScriptDataNode
    {
        public String meta;
        public List<ScriptDataNode> children = new List<ScriptDataNode>();
        public Dictionary<String, object> attributes = new Dictionary<string, object>();
        public int indent = 0;
        public object index = -1;
        public String data;

        public ScriptDataNode()
        {
        }

        public ScriptDataNode(string meta, string data, object index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data;
        }

        public ScriptDataNode(string meta, float[] data, object index, int indent = 0)
        {
            this.indent = indent;
            this.meta = meta;
            this.index = index;
            this.data = "";
            foreach(float val in data)
            {
                this.data += (this.data == "" ? "" : " ") + val.ToString();
            }
        }

        public ScriptDataNode(string meta, float data, object index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data.ToString();
        }

        public ScriptDataNode(string meta, bool data, object index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            this.meta = meta;
            this.data = data ? "true" : "false";
        }

        public ScriptDataNode(string meta, Dictionary<string, object> data, object index, int indent = 0)
        {
            this.indent = indent;
            this.index = index;
            
            if (data.ContainsKey("_meta"))
                this.meta = data["_meta"].ToString();
            else
                this.meta = meta;

            foreach(KeyValuePair<string, object> kvp in data)
            {
                if (kvp.Key.Equals("_meta"))
                    continue;

                int newindex;
                if (int.TryParse(kvp.Key, out newindex))
                {
                    if (kvp.Value is Dictionary<string, object>)
                        this.children.Add((ScriptDataNode)Activator.CreateInstance(this.GetType(), "table", kvp.Value as Dictionary<string, object>, newindex, this.indent + 1));
                    else if (kvp.Value is float)
                        this.children.Add((ScriptDataNode)Activator.CreateInstance(this.GetType(), "value_node", (float)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is string)
                        this.children.Add((ScriptDataNode)Activator.CreateInstance(this.GetType(), "value_node", (string)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is bool)
                        this.children.Add((ScriptDataNode)Activator.CreateInstance(this.GetType(), "value_node", (bool)kvp.Value, newindex, this.indent + 1));
                    else if (kvp.Value is float[])
                        this.children.Add((ScriptDataNode)Activator.CreateInstance(this.GetType(), "value_node", (float[])kvp.Value, newindex, this.indent + 1));
                    else
                        Console.WriteLine("No option for " + kvp.Value);
                }
                else
                {
                    if (kvp.Value is Dictionary<string, object>)
                    {
                        ScriptDataNode node = (ScriptDataNode)Activator.CreateInstance(this.GetType(), kvp.Key, kvp.Value as Dictionary<string, object>, kvp.Key, this.indent + 1);
                        string nodeString = node.ToString();
                        if (this.children.FindIndex(item => item.ToString().Equals(nodeString)) == -1)
                            this.children.Add(node);
                    }
                    else
                        this.attributes.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public ScriptDataNode(StreamReader data)
        {
            this.FromString(data);
        }

        public virtual void FromString(StreamReader data)
        {
            
        }

        public abstract override string ToString();

        public Dictionary<string, object> ToDieselScript()
        {
            Dictionary<string, object> toreturn = new Dictionary<string, object>();
            
            toreturn.Add("_meta", this.meta);

            foreach (var attr in this.attributes)
                toreturn.Add(attr.Key, attr.Value);

            int element_count = 1;
            int additional_count = 1;

            foreach (ScriptDataNode child in this.children)
            {
                String temp_meta = ( child.meta.ToLowerInvariant().Equals("value_node") ? (element_count++).ToString() : child.meta );

                if (toreturn.ContainsKey(temp_meta))
                    toreturn.Add((additional_count++).ToString(), child.ToDieselScript());
                else
                    toreturn.Add(temp_meta, child.ToDieselScript());
            }

            return toreturn;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is ScriptDataNode)
                return this.ToString().Equals((obj as ScriptDataNode).ToString());

            return false;
        }
    }
}
