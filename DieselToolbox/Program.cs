using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NDesk.Options;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.IO;
using CSScriptLibrary;
using DieselEngineFormats.Bundle;
using System.Text;
using Eto.Forms;
using DieselEngineFormats.ScriptData;
using DieselEngineFormats;
using System.Diagnostics;
using DieselToolbox.Utils;

namespace DieselToolbox
{
	public class App
	{
		public string scriptPath = "./scripts";

        public static App Instance;

        public Application MainApp;

		public App(string[] args, Application app)
		{
            Instance = this;
            this.MainApp = app;

            StaticData.Engine = Python.CreateEngine();
			StaticData.Settings = new Settings ();

			//this.LoadScripts();

			bool launchMain = true;
			bool showHelp = false;

			/*string fontFile = null;
			string textureFile = null;*/

			var p = new OptionSet() {
				/*{ "t|texture=", "the {FILEPATH} of a DDS texture you wish to use.",
					v => {
						textureFile = v;
						launchMain = false;
					} },
				{ "f|font=", "the {FILEPATH} of the font you wish to open, to open a font texture with this use -t",
					v => {
						fontFile = v;
						launchMain = false;
					} },
				{ "s|scriptdata=", 
					"the {FILEPATH} of a ScriptData file you wish to view.",
					v => {
						ScriptDataViewer textView = new ScriptDataViewer();
						textView.LoadScriptData(v);
						textView.Show();
						launchMain = false;
					} },*/
				{"db|database=", "the {FILEPATH} of the database you wish to load on launch",
					v => {
						frmPackageBrowser brows = new frmPackageBrowser();
						brows.LoadDatabase(v);
						app.Run(brows);
						launchMain = false;
					} },
				{ "h|help",  "show this message and exit", 
					v => {showHelp = v != null; launchMain = false;} },
			};

			List<string> extra;
			try
			{
				extra = p.Parse(args);
			}
			catch (OptionException exc)
			{
				Console.WriteLine(exc.Message);
				return;
			}
            
            /*if (fontFile != null)
			{
				FontEditor fontEd = new FontEditor(fontFile, textureFile);
				fontEd.Show();
			}
			else if (textureFile != null)
			{
				DDSViewer ddsView = new DDSViewer();
				ddsView.LoadTexture(textureFile);
				ddsView.Show();
			}*/

            //this.LoadHashlists ();

            LoadConverters();

			if (launchMain)
			{
                try {
				    frmPackageBrowser brower = new frmPackageBrowser();
                    app.Run(brower);
                }
                catch(Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                    Console.Read();
                }

			}


			if (showHelp)
				this.PrintHelp();
		}

		private void PrintHelp()
		{

		}

        public void LoadConverters()
        {
            FormatConverter cXML = new FormatConverter()
            {
                Key = "script_cxml",
                Title = "Custom XML",
                Extension = "xml",
                Type = "scriptdata"
            };

            cXML.ExportEvent += (MemoryStream ms, bool escape) =>
            {
                return new CustomXMLNode("table", (Dictionary<string, object>)new ScriptData(new BinaryReader(ms)).Root, "").ToString(0, escape);
            };

            ScriptActions.AddConverter(cXML);

            FormatConverter Strings = new FormatConverter()
            {
                Key = "diesel_strings",
                Title = "Diesel",
                Extension = "strings",
                Type = "strings"
            };

            Strings.ImportEvent += (path) => new StringsFile(path);
            ScriptActions.AddConverter(Strings);

            /*ScriptActions.AddConverter(new FormatConverter()
            {
                Key = "texture_dds",
                Title = "DDS",
                Extension = "dds",
                Type = "texture"
            });*/

            FormatConverter stringsCSV = new FormatConverter()
            {
                Key = "strings_csv",
                Title = "CSV",
                Extension = "csv",
                Type = "strings"
            };

            //Excel doesn't seem to like it?
            stringsCSV.ExportEvent += (MemoryStream ms, bool arg0) =>
            {
                StringsFile str = new StringsFile(ms);
                StringBuilder builder = new StringBuilder();
                builder.Append("ID,String\n");
                foreach (var entry in str.LocalizationStrings)
                    builder.Append("\"" + entry.ID.ToString() + "\",\"" + entry.Text + "\"\n");
                Console.WriteLine(builder.ToString());
                return builder.ToString();
            };

            ScriptActions.AddConverter(stringsCSV);

            FormatConverter scriptJSON = new FormatConverter()
            {
                Key = "script_json",
                Title = "JSON",
                Extension = "json",
                Type = "scriptdata"
            };

            scriptJSON.ExportEvent += (MemoryStream ms, bool arg0) =>
            {
                ScriptData sdata = new ScriptData(new BinaryReader(ms));
                return (new JSONNode("table", sdata.Root, "")).ToString();
            };

            ScriptActions.AddConverter(scriptJSON);

            FormatConverter stringsJSON = new FormatConverter()
            {
                Key = "strings_json",
                Title = "JSON",
                Extension = "json",
                Type = "strings"
            };

            //Excel doesn't seem to like it?
            stringsJSON.ExportEvent += (MemoryStream ms, bool arg0) =>
            {
                StringsFile str = new StringsFile(ms);
                StringBuilder builder = new StringBuilder();
                builder.Append("{\n");
                for (int i = 0; i < str.LocalizationStrings.Count; i++)
                {
                    StringEntry entry = str.LocalizationStrings[i];
                    builder.Append("\t");
                    builder.Append("\"" + entry.ID + "\" : \"" + entry.Text + "\"");
                    if(i < str.LocalizationStrings.Count - 1)
                        builder.Append(",");
                    builder.Append("\n");
                }
                builder.Append("}");
                Console.WriteLine(builder.ToString());
                return builder.ToString();
            };

            ScriptActions.AddConverter(stringsJSON);
        }

        public void LoadScripts()
		{
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (string folder in Directory.EnumerateDirectories(this.scriptPath))
			{
                try
                {
                    string main_path;
					if (File.Exists(main_path = Path.Combine(folder, "main.py")))
					{
						dynamic scope = StaticData.Engine.CreateScope();
                        foreach (string file in Directory.GetFiles(folder))
                            StaticData.Engine.ExecuteFile(file, scope);

                        if (scope.register != null)
                            scope.register();
                    }
					else if (File.Exists(main_path = Path.Combine(folder, "main.cs")))
					{
						System.Reflection.Assembly objAssembly = CSScript.LoadFiles(Directory.GetFiles(folder), "DieselEngineFormats", "System.Web");
                        if (objAssembly.GetType("main", false, true) != null)
                        {
                            dynamic obj = objAssembly.CreateInstance("main", true);
                            if (obj.GetType().GetMethod("register") != null)
                                obj.register();
                        }
                        else
                            Console.WriteLine("Script with path \"{0}\" does not contain a main entry point type!", main_path);
					}
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }

                watch.Stop();
            }
            Console.WriteLine("Loaded scripts successfully, it took {0} seconds", watch.ElapsedMilliseconds / 1000f);
        }

        public void LoadHashlists()
		{
            HashIndex.Load(Path.Combine(Definitions.HashDir, "paths"), HashIndex.HashType.Path);
            HashIndex.Load(Path.Combine(Definitions.HashDir, "exts"));
            //HashIndex.Load(Path.Combine(Definitions.HashDir, "others"));

            foreach (string file in Directory.EnumerateFiles(Path.Combine(Definitions.HashDir, Definitions.AddedHashDir)))
				HashIndex.Load (file, HashIndex.HashType.Path);
		}
	}

    public class FormatConverter
    {
        public string Key;
        public string Title;
        public string Extension;
        public string Type;

        public delegate object ExportEventDel(MemoryStream ms, bool arg0);
        public event ExportEventDel ExportEvent;

        public object Export(MemoryStream ms, bool arg0=false)
        {
            if (ExportEvent != null)
                return ExportEvent(ms, arg0);
            return ms;
        }

        public delegate object ImportEventDel(string path);
        public event ImportEventDel ImportEvent;

        public object Import(string path)
        {
            return null;
        }
    }

    public static class ScriptActions
	{
		public static Dictionary<string, Dictionary<string, FormatConverter>> Converters = new Dictionary<string, Dictionary<string, FormatConverter>>();
        public static Dictionary<string, dynamic> Scripts = new Dictionary<string, dynamic>();

        public static void AddConverter(FormatConverter format)
        {
            if (format.Key == null)
            {
                Console.WriteLine("[ERROR] Converter must have a key variable!");
                return;
            }

            if (!Converters.ContainsKey(format.Type))
                Converters.Add(format.Type, new Dictionary<string, FormatConverter>());

            if (Converters[format.Type].ContainsKey(format.Key))
            {
                Console.WriteLine("[ERROR] Conveter is already registered with key {0}", format.Key);
                return;
            }

            Converters[format.Type].Add(format.Key, format);
        }

        public static void RegisterConverter(dynamic pis)
		{
            FormatConverter format = new FormatConverter()
            {
                Key = pis.key,
                Extension = pis.extension,
                Type = pis.type,
                Title = pis.title
            };

            format.ExportEvent += pis.export;

            AddConverter(format);
        }

        public static void RegisterScript(dynamic pis)
        {
            if (pis.key == null)
            {
                Console.WriteLine("[ERROR] Script to register must have a key variable!");
                return;
            }

            if (Scripts.ContainsKey(pis.key))
            {
                Console.WriteLine("[ERROR] Script is already registered with key {1}", pis.key);
                return;
            }

            Scripts.Add(pis.key, pis);
        }

        public static FormatConverter GetConverter(string type, string key)
        {
            if (!Converters.ContainsKey(type) || !Converters[type].ContainsKey(key))
                return null;

            return Converters[type][key];
        }
    }
}

