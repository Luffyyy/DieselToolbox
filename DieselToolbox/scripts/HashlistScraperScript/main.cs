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
        Console.WriteLine("Register");
        ScriptActions.RegisterScript(new HashlistScraper());
    }
}

public class HashlistScraper
{
    private struct XMLTagLookup
    {
        public string node_name { get; set; }
        public string[] value { get; set; }
        public Func<string, string> Converter { get; set; }
        public Func<XmlNode, bool> ValidAttribute { get; set; }
    }

    public string key = "hashlist_scraper";
    public string title = "Hashlist Scraper";

    private string _hashlist_tag = "scraped";
    private string hashlist_tag;

    public bool ignore_files_with_path = false;

    private Dictionary<string, Action<FileEntry>> FileProcessors;
    private StreamWriter error_output;

    public HashlistScraper()
    {
        FileProcessors = new Dictionary<string, Action<FileEntry>> {
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
            { "world", ProcessWorld },
            { "environment", ProcessEnvironment },
            { "dialog_index", ProcessDialogIndex }
        };
    }

    public void execute(PackageBrowser browser)
    {
        hashlist_tag = _hashlist_tag + Directory.EnumerateFiles(Definitions.HashDir, _hashlist_tag + "*").ToList().Count;
        error_output = new StreamWriter(hashlist_tag + ".log");

        System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
        clock.Start();
        error_output.Write("Hashlist Scraper executed" + "\n");
        error_output.Flush();
        foreach (KeyValuePair<string, FileEntry> entry in browser.RawFiles)
        {
            this.ProcessFile(entry.Value);
        }
        //this.ProcessFolder(browser.Root);
        //Path.Combine(Definitions.HashDir, hashlist_tag)
        HashIndex.GenerateHashList(Path.Combine(Definitions.HashDir, hashlist_tag), hashlist_tag);
        clock.Stop();
        error_output.Write("Scrape operation took {0} seconds" + "\n", clock.Elapsed.TotalSeconds.ToString());
        error_output.Close();
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

    private void ProcessFile(FileEntry file)
    {
        if (file.BundleEntries.Count == 0 || !FileProcessors.ContainsKey(file._extension.ToString()) || (ignore_files_with_path && file._path.HasUnHashed))
            return;

        try
        {
            FileProcessors[file._extension.ToString()].Invoke(file);
        }
        catch (Exception exc)
        {
            error_output.Write("Exception occured on file: {0}\n", file.Path);
            error_output.Write(exc.Message + "\n");
            error_output.Write(exc.StackTrace + "\n");
            error_output.Flush();
        }
    }

    private void AddHash(string hash)
    {
        if (HashIndex.AddHash(hash, hashlist_tag))
            error_output.Write("Added hash {0}\n", hash);

    }

    private void ProcessXML(FileEntry file, List<XMLTagLookup> attribute_tags, string xml = null)
    {
        XmlDocument doc = new XmlDocument();

        string xml_doc = xml;
        if (xml == null)
        {
            using (var reader = new StreamReader(file.FileStream()))
                xml_doc = reader.ReadToEnd();
        }

        try
        {
            doc.LoadXml(xml_doc);
        }
        catch(Exception exc)
        {
            error_output.Write("Exception occured on file: {0}\n", file.Path);
            if (xml != null)
                error_output.Write(xml + "\n");
            error_output.Write(exc.Message + "\n");
            error_output.Write(exc.StackTrace + "\n");
            error_output.Flush();
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
                        hash = sub_depends.Attributes[0] != null ? sub_depends.Attributes[0].Value : null;
                    else
                    {
                        XmlNode named_item = sub_depends.Attributes.GetNamedItem(val);
                        hash = named_item != null ? named_item.Value : null;
                    }
                    hash = System.Web.HttpUtility.HtmlDecode(hash);
                    if (tag.Converter != null)
                        hash = tag.Converter.Invoke(hash);

                    if (!String.IsNullOrWhiteSpace(hash))
                        this.AddHash(hash);
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
            new XMLTagLookup { node_name="depends_on", value=null },
            new XMLTagLookup { node_name= "object", value= new[]{ "file" } },
            new XMLTagLookup { node_name="anim_state_machine", value=null },
            new XMLTagLookup { node_name="network", value=new[]{"remote_unit" } }
        });
    }

    private void ProcessObject(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="diesel", value=new[]{"materials" } },
            new XMLTagLookup { node_name="sequence_manager", value=new[]{"file" } },
            new XMLTagLookup { node_name="animation_def", value=new[]{"name" } },
            new XMLTagLookup { node_name="effect_spawner", value=new[]{"effect", "object" } },
            new XMLTagLookup { node_name="object", value=new[]{"name" } }
        });
    }

    private void ProcessMaterialConfig(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="diffuse_texture", value=new[]{"file" } },
            new XMLTagLookup { node_name="bump_normal_texture", value=new[]{"file" } },
            new XMLTagLookup { node_name="self_illumination_texture", value=new[]{"file" } },
            new XMLTagLookup { node_name="reflection_texture", value=new[]{"file" } },
            new XMLTagLookup { node_name="opacity_texture", value=new[]{"file" } },
            new XMLTagLookup { node_name="material", value = new[]{"name" }}
        });
    }

    private void ProcessMergedFont(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="font", value=new[]{"name" } }
        });
    }
    
    private void ProcessGUI(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="preload", value=new[]{"texture" } }
        });
    }

    private void ProcessScene(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="load_scene", value=new[]{"file" } }
        });
    }
    
    private void ProcessAnimationDef(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="bone", value=new[]{"name" } },
            new XMLTagLookup { node_name="subset", value=new[]{"file" } }
        });
    }

    private void ProcessAnimationStateMachine(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="states", value=new[]{"file" } }
        });
    }

    private void ProcessAnimationSubset(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="anim", value=new[]{"file" } }
        });
    }

    private void ProcessEffect(FileEntry file)
    {
        this.ProcessXML(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="effect_spawn", value=new[]{"effect" } },
            new XMLTagLookup { node_name="billboard", value=new[]{"texture" } }
        });
    }

    private void ProcessScriptData(FileEntry file, List<XMLTagLookup> lookup)
    {
        /*ScriptData script = new ScriptData(new BinaryReader(file.FileStream()));
        if (script.Root is Dictionary<string, object>)
        {

        }*/
        string xml = ScriptActions.GetConverter("scriptdata", "script_cxml").export(file.FileStream(), true);
        this.ProcessXML(file, lookup, xml);
    }

    private void ProcessContinent(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="unit_data", value=new[]{"name" } },
            new XMLTagLookup { node_name = "editable_gui", value=new[]{"font" } }
        });
    }

    private void ProcessSequenceManager(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="material_config", value=new[]{"name" }, Converter = (hash) => { return hash.Replace("'", "").Replace(" ", ""); } },
            new XMLTagLookup { node_name="object", value=new[]{"name" }, Converter = (hash) => { return hash.Replace("'", "").Replace(" ", ""); } }
        });
    }

    private void ProcessWorld(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="environment_values", value=new[]{"environment" } }
        });
    }

    private void ProcessEnvironment(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="param", value=new[]{"value" }, ValidAttribute = (node) => { return node.Attributes.GetNamedItem("key").Value == "underlay" || node.Attributes.GetNamedItem("key").Value == "global_texture";  } }
        });
    }

    private void ProcessDialogIndex(FileEntry file)
    {
        this.ProcessScriptData(file, new List<XMLTagLookup> {
            new XMLTagLookup { node_name="include", value=new[]{"name" }, Converter = (hash) => { return "gamedata/dialogs/" + hash; } }
        });
    }
}
