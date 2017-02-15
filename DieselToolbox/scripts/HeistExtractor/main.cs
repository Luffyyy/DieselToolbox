using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DieselToolbox;
using DieselEngineFormats;
using DieselEngineFormats.Bundle;
using DieselEngineFormats.ScriptData;
using System.Xml;

public class main
{
    public void register()
    {
        Console.WriteLine("Register HeistExtractor");
        ScriptActions.RegisterScript(new HeistExtractor());
    }
}

public class HeistExtractor
{
    private struct XMLTagLookup
    {
        public string node_name { get; set; }
        public string[] value { get; set; }
        public Func<string, string> Converter { get; set; }
        public Func<XmlNode, bool> ValidAttribute { get; set; }
        public bool key_is_ext { get; set; }
    }

    public string key = "heist_extractor";
    public string title = "Heist Extractor";

    private Dictionary<string, Action<FileEntry>> FileProcessors;
    private StreamWriter error_output;

    private string heist_world = "levels/bridge/world";

    public HeistExtractor()
    {
        this.FileProcessors = new Dictionary<string, Action<FileEntry>> {
            { "unit", ProcessUnit },
            { "object", ProcessObject },
            { "material_config", ProcessMaterialConfig },
            { "merged_font", ProcessMergedFont },
            { "gui", ProcessGUI },
            { "scene", ProcessScene },
            { "animation_def", ProcessAnimationDef },
            { "animation_state_machine", ProcessAnimationStateMachine },
            { "animation_subset", ProcessAnimationSubset },
            { "effect", ProcessEffect },
            { "continent", ProcessContinent },
            { "sequence_manager", ProcessSequenceManager },
            { "environment", ProcessEnvironment },
            { "mission", ProcessMission },
            { "dialog_index", ProcessDialogIndex }
        };
    }

    private PackageBrowser _browser;

    private string OutputPath = "D:/Test Output Green Bridge";

    private HashSet<Idstring> ExtractedPaths; 

    public void execute(PackageBrowser browser)
    {
        this.ExtractedPaths = new HashSet<Idstring>();

        this.error_output = new StreamWriter("./heist_extractor.log");

        this._browser = browser;
        System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
        clock.Start();
        this.error_output.Write("Heist Extractor executed" + "\n");
        this.error_output.Flush();
        Idstring ids = HashIndex.Get(this.heist_world);
        Idstring ids_ext = HashIndex.Get("world");
        var tids = new Tuple<Idstring, Idstring, Idstring>(ids, new Idstring(0), ids_ext);
        if (browser.RawFiles.ContainsKey(tids))
        {
            this.ProcessWorld(browser.RawFiles[tids]);
        }
        else
            Console.WriteLine("World File does not exist");

        //this.ProcessFolder(browser.Root);
        //Path.Combine(Definitions.HashDir, hashlist_tag)
        using (StreamWriter str = new StreamWriter(new FileStream(Path.Combine(this.OutputPath, "add.xml"), FileMode.Create, FileAccess.Write)))
        {
            str.Write("<table>\n");
            foreach(Idstring path in this.ExtractedPaths)
            {
                string[] split = path.ToString().Split('.');
                str.Write(String.Format("\t<{0} path=\"{1}\" force=\"true\"/>\n", split[1], split[0]));
            }

            str.Write("</table>\n");
        }

        clock.Stop();
        this.error_output.Write("Scrape operation took {0} seconds" + "\n", clock.Elapsed.TotalSeconds.ToString());
        this.error_output.Close();
    }

    /*private void ProcessFolder(IParent folder)
    {
        foreach (IChild child in folder.Children.Values)
        {
            if (child is FileEntry)
                this.ProcessFile(child as FileEntry);
            else if (child is IParent)
                this.ProcessFolder(child as IParent);
        }
    }*/

    private void WriteFile(FileEntry entry, byte[] byt = null)
    {
        Idstring ids = HashIndex.Get(entry.Path);
        if (entry.BundleEntries.Count == 0 || this.ExtractedPaths.Contains(ids))
            return;

        string path = Path.Combine(this.OutputPath, entry.Path);
        string folder = Path.GetDirectoryName(path);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        byte[] bytes = byt ?? entry.FileBytes() ?? new byte[0];

        File.WriteAllBytes(path, bytes);
        this.ExtractedPaths.Add(ids);
    }

    private void ProcessWorld(FileEntry file)
    {
        foreach(KeyValuePair<string, IChild> child in file.Parent.Children)
        {
            if (child.Value is FileEntry)
            {
                this.WriteFile(child.Value as FileEntry);
            }
        }

        this.WriteFile(file);
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="environment_values", value=new[]{"environment" }, Converter = (hash) => { return hash + ".environment"; } }
        });

        string continents_file = Path.Combine(Path.GetDirectoryName(file.Path), "continents").Replace("\\", "/");
        Idstring ids = HashIndex.Get(continents_file);
        var t_ids = new Tuple<Idstring, Idstring, Idstring>(ids, new Idstring(0), HashIndex.Get("continents"));
        if (this._browser.RawFiles.ContainsKey(t_ids))
        {
            FileEntry c_file = this._browser.RawFiles[t_ids];
            this.WriteFile(c_file);

            string xml = ScriptActions.GetConverter("scriptdata", "script_cxml").export(c_file.FileStream(), true);

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xml);
                foreach (XmlNode child in doc.ChildNodes[0])
                {
                    this.ProcessFile(Path.Combine(Path.GetDirectoryName(file.Path), string.Format("{0}/{0}.continent", child.Attributes.GetNamedItem("name").Value)).Replace("\\", "/"));
                }
            }
            catch (Exception exc)
            {
                this.error_output.Write("Exception occured on file: {0}\n", c_file.Path);
                if (xml != null)
                    this.error_output.Write(xml + "\n");
                this.error_output.Write(exc.Message + "\n");
                this.error_output.Write(exc.StackTrace + "\n");
                this.error_output.Flush();
                return;
            }
        }
        else
            this.error_output.Write("Continents file {0} does not exist!\n", continents_file);

        string mission_file = Path.Combine(Path.GetDirectoryName(file.Path), "mission").Replace("\\", "/");
        Idstring m_ids = HashIndex.Get(mission_file);
        var t_m_ids = new Tuple<Idstring, Idstring, Idstring>(m_ids, new Idstring(0), HashIndex.Get("mission"));
        if (this._browser.RawFiles.ContainsKey(t_m_ids))
        {
            FileEntry m_file = this._browser.RawFiles[t_m_ids];
            this.WriteFile(m_file);

            string xml = ScriptActions.GetConverter("scriptdata", "script_cxml").export(m_file.FileStream(), true);

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xml);
                foreach (XmlNode child in doc.ChildNodes[0])
                {
                    this.ProcessFile(Path.Combine(Path.GetDirectoryName(file.Path), string.Format("{0}.mission", child.Attributes.GetNamedItem("file").Value)).Replace("\\", "/"));
                }
            }
            catch (Exception exc)
            {
                this.error_output.Write("Exception occured on file: {0}\n", m_file.Path);
                if (xml != null)
                    this.error_output.Write(xml + "\n");
                this.error_output.Write(exc.Message + "\n");
                this.error_output.Write(exc.StackTrace + "\n");
                this.error_output.Flush();
                return;
            }
        }
        else
            this.error_output.Write("Mission file {0} does not exist!\n", continents_file);

        this.error_output.Flush();
    }

    private void ProcessFile(string path)
    {
        Idstring p_ids = HashIndex.Get(Path.GetFileNameWithoutExtension(path));
        var t_ids = new Tuple<Idstring, Idstring, Idstring>(p_ids, new Idstring(0), HashIndex.Get(Path.GetExtension(path)));
        if (!this._browser.RawFiles.ContainsKey(t_ids))
        {
            this.error_output.Write(string.Format("File with path {0} does not exist!\n", path));
            this.error_output.Flush();
            return;
        }
        FileEntry file = this._browser.RawFiles[t_ids];

        if (file.BundleEntries.Count == 0 || this.ExtractedPaths.Contains(p_ids))
            return;

        try
        {
            if (Path.GetExtension(path) == ".object")
            {
                string model_file = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)).Replace("\\", "/");
                Idstring m_ids = HashIndex.Get(model_file);
                //error_output.WriteLine(string.Format("Attempt to ouput model file {0}", model_file));
                var t_m_ids = new Tuple<Idstring, Idstring, Idstring>(m_ids, new Idstring(0), HashIndex.Get("model"));
                if (this._browser.RawFiles.ContainsKey(t_m_ids))
                    this.WriteFile(this._browser.RawFiles[t_m_ids]);

                string cooked_physics = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)).Replace("\\", "/");
                Idstring c_ids = HashIndex.Get(cooked_physics);
                var t_c_ids = new Tuple<Idstring, Idstring, Idstring>(c_ids, new Idstring(0), HashIndex.Get("cooked_physics"));
                //error_output.WriteLine(string.Format("Attempt to ouput cooked_physics file {0}", cooked_physics));
                if (this._browser.RawFiles.ContainsKey(t_c_ids))
                    this.WriteFile(this._browser.RawFiles[t_c_ids]);
            }

            if (this.FileProcessors.ContainsKey(file._extension.ToString()))
                this.FileProcessors[file._extension.ToString()].Invoke(file);
            else
                this.WriteFile(file);

        }
        catch (Exception exc)
        {
            this.error_output.Write("Exception occured on file: {0}\n", file.Path);
            this.error_output.Write(exc.Message + "\n");
            this.error_output.Write(exc.StackTrace + "\n");
            this.error_output.Flush();
        }
    }

    private void ProcessXML(FileEntry file, List<XMLTagLookup> attribute_tags, string xml = null)
    {
        XmlDocument doc = new XmlDocument();

        string xml_doc = xml;
        if (xml == null)
        {
            MemoryStream str = file.FileStream();
            this.WriteFile(file, str.ToArray());
            using (var reader = new StreamReader(str))
                xml_doc = reader.ReadToEnd();

            if (str != null)
                str.Close();
        }

        try
        {
            doc.LoadXml(xml_doc);
        }
        catch (Exception exc)
        {
            this.error_output.Write("Exception occured on file: {0}\n", file.Path);
            if (xml != null)
                this.error_output.Write(xml + "\n");
            this.error_output.Write(exc.Message + "\n");
            this.error_output.Write(exc.StackTrace + "\n");
            this.error_output.Flush();
            return;
        }
        foreach (XMLTagLookup tag in attribute_tags)
        {
            XmlNodeList depends = doc.GetElementsByTagName(tag.node_name);
            foreach (XmlNode sub_depends in depends)
            {
                if (sub_depends.Attributes.Count == 0)
                    continue;

                if (tag.ValidAttribute != null && !tag.ValidAttribute.Invoke(sub_depends))
                    return;

                foreach (string val in tag.value ?? new string[] { null })
                {

                    string hash;
                    if (val == null)
                    {
                        XmlAttribute att = sub_depends.Attributes[0];
                        hash = att != null ? att.Value : null;
                        if (tag.key_is_ext)
                            hash += "." + att.Name;
                    }
                    else
                    {
                        XmlNode named_item = sub_depends.Attributes.GetNamedItem(val);
                        hash = named_item != null ? named_item.Value : null;
                    }
                    hash = System.Web.HttpUtility.HtmlDecode(hash);
                    if (!String.IsNullOrWhiteSpace(hash))
                    {
                        if (tag.Converter != null)
                            hash = tag.Converter.Invoke(hash);

                        if (!String.IsNullOrWhiteSpace(hash))
                            this.ProcessFile(hash);
                    }
                }
            }
        }
        /*foreach (XmlNode node in doc.ChildNodes)
        {
            Console.WriteLine(node.Name);
            if (node.Name == "anim_state_machine" || node.Name == "object")
                this.AddHashFromFirstAttribute(node);

        }*/
    }


    private void ProcessUnit(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="depends_on", value=null, key_is_ext = true },
            new XMLTagLookup { node_name= "object", value= new[]{ "file" }, Converter = (hash) => { return hash + ".object"; } },
            new XMLTagLookup { node_name="anim_state_machine", value=null, Converter = (hash) => { return hash + ".anim_state_machine"; } },
            new XMLTagLookup { node_name="network", value=new[]{"remote_unit" }, Converter = (hash) => { return hash + ".unit"; } }
        });
    }

    private void ProcessObject(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="diesel", value=new[]{"materials" }, Converter = (hash) => { return hash + ".material_config"; } },
            new XMLTagLookup { node_name="sequence_manager", value=new[]{"file" }, Converter = (hash) => { return hash + ".sequence_manager"; } },
            new XMLTagLookup { node_name="animation_def", value=new[]{"name" }, Converter = (hash) => { return hash + ".animation_def"; } },
            new XMLTagLookup { node_name="effect_spawner", value=new[]{"effect" }, Converter = (hash) => { return hash + ".effect"; } },
            //new XMLTagLookup { node_name="object", value=new[]{"name" } }
        });
    }

    private void ProcessMaterialConfig(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="diffuse_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="diffuse_layer0_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="diffuse_layer1_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="bump_normal_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="self_illumination_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="reflection_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } },
            new XMLTagLookup { node_name="opacity_texture", value=new[]{"file" }, Converter = (hash) => { return hash + ".texture"; } }
        });
    }

    private void ProcessMergedFont(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="font", value=new[]{"name" }, Converter = (hash) => { return hash + ".font"; } }
        });
    }

    private void ProcessGUI(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="preload", value=new[]{"texture" }, Converter = (hash) => { return hash + ".texture"; } }
        });
    }

    private void ProcessScene(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="load_scene", value=new[]{"file" }, Converter = (hash) => { return hash + ".scene"; } }
        });
    }

    private void ProcessAnimationDef(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="subset", value=new[]{"file" }, Converter = (hash) => { return hash + ".animation_subset"; } }
        });
    }

    private void ProcessAnimationStateMachine(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="states", value=new[]{"file" }, Converter = (hash) => { return hash + ".animations_states"; } }
        });
    }

    private void ProcessAnimationSubset(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="anim", value=new[]{"file" }, Converter = (hash) => { return hash + ".anim"; } }
        });
    }

    private void ProcessEffect(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="effect_spawn", value=new[]{"effect" }, Converter = (hash) => { return hash + ".effect"; } },
            new XMLTagLookup { node_name="billboard", value=new[]{"texture" }, Converter = (hash) => { return hash + ".texture"; } }
        });
    }

    private void ProcessScriptData(FileEntry file, List<XMLTagLookup> lookup)
    {
        /*ScriptData script = new ScriptData(new BinaryReader(file.FileStream()));
        if (script.Root is Dictionary<string, object>)
        {

        }*/
        MemoryStream str = file.FileStream();
        this.WriteFile(file, str.ToArray());
        string xml = ScriptActions.GetConverter("scriptdata", "script_cxml").export(str, true);
        this.ProcessXML(file, lookup, xml);
    }

    private void ProcessContinent(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="unit_data", value=new[]{"name" }, Converter = (hash) => { return hash + ".unit"; } },
            new XMLTagLookup { node_name = "editable_gui", value=new[]{"font" }, Converter = (hash) => { return hash + ".font"; } }
        });
    }

    private void ProcessSequenceManager(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="material_config", value=new[]{"name" }, Converter = (hash) => { return hash.Replace("'", "").Replace(" ", "") + ".material_config"; } }
        });
    }

    /*private void ProcessWorld(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="environment_values", value=new[]{"environment" } }
        });
    }*/

    private void ProcessEnvironment(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="param", value=new[]{"value" }, ValidAttribute = (node) => { return node.Attributes.GetNamedItem("key").Value == "underlay" || node.Attributes.GetNamedItem("key").Value == "global_texture";  }, Converter = (hash) => { return hash + ".texture"; } }
        });
    }

    private void ProcessDialogIndex(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="include", value=new[]{"name" }, Converter = (hash) => { return "gamedata/dialogs/" + hash + ".dialog"; } }
        });
    }

    private void ProcessMission(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="values", value=new[]{"enemy" }, Converter = (hash) => { return hash + ".unit"; } }
        });
    }
}
