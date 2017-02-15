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
        ScriptActions.RegisterScript(new HashlistOutputterExt());
    }
}

public class HashlistOutputterExt
{
    public string key = "hashlist_outputter_ext";
    public string title = "Hashlist Outputter Ext";

    public HashSet<string> Extensions = new HashSet<string>();

    public void execute(PackageBrowser browser)
    {
        foreach(var file in browser.RawFiles)
        {
            Extensions.Add(file.Value._extension.UnHashed);
            if (file.Value._language != null && file.Value._language.HasUnHashed)
                Extensions.Add(file.Value._language.UnHashed);
        }

        string path = Path.Combine(Definitions.HashDir, "exts");

        using (StreamWriter strw = new StreamWriter(path, false))
        {
            foreach (string str in Extensions)
            {
                strw.Write(str + "\n");
            }
        }
    }
}
