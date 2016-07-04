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

			this.LoadScripts();

			bool launchMain = true;
			bool showHelp = false;

			string fontFile = null;
			string textureFile = null;

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

            this.LoadHashlists ();

			if (launchMain)
			{
				MainForm main = new MainForm();
                try {
                    app.Run(main);
                }
                catch(Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }

			}

			if (showHelp)
				this.PrintHelp();
		}

		private void PrintHelp()
		{

		}

		public void LoadScripts()
		{
			
			foreach (string folder in Directory.EnumerateDirectories(this.scriptPath))
			{
                try
                {
                    string main_path;
					if (File.Exists(main_path = Path.Combine(folder, "main.py")))
					{
						dynamic scope = StaticData.Engine.CreateScope();
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            StaticData.Engine.ExecuteFile(file, scope);
                        }

                        if (scope.register != null)
                            scope.register();
                    }
					else if (File.Exists(main_path = Path.Combine(folder, "main.cs")))
					{
						System.Reflection.Assembly objAssembly = CSScript.LoadFiles(Directory.GetFiles(folder), "DieselEngineFormats", "System.Web");
						dynamic obj = objAssembly.CreateInstance("main", true);
						if (obj.GetType().GetMethod("register") != null)
						{
							obj.register();
						}
					}
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }
            }
		}

		public void LoadHashlists()
		{
			foreach (string file in Directory.EnumerateFiles(Definitions.HashDir)) {
				HashIndex.Load (file);
			}
		}
	}

	public static class ScriptActions
	{
		public static Dictionary<string, Dictionary<string, dynamic>> Converters = new Dictionary<string, Dictionary<string, dynamic>>();
        public static Dictionary<string, dynamic> Scripts = new Dictionary<string, dynamic>();


        public static void RegisterConverter(dynamic pis)
		{
            if (pis.key == null)
            {
                Console.WriteLine("[ERROR] Conveter to register must have a key variable!");
                return;
            }

            if (!Converters.ContainsKey(pis.type))
                Converters.Add(pis.type, new Dictionary<string, dynamic>());

            if (Converters[pis.type].ContainsKey(pis.key))
            {
                Console.WriteLine("[ERROR] Conveter is already registered with key {0}", pis.key);
                return;
            }

            Converters[pis.type].Add(pis.key, pis);
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

        public static dynamic GetConverter(string type, string key)
        {
            if (!Converters.ContainsKey(type) || !Converters[type].ContainsKey(key))
                return null;

            return Converters[type][key];
        }
    }
}

