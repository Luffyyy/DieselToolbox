using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DieselToolbox;
using DieselEngineFormats.ScriptData;

public class main
{
    public void register()
    {
        Console.WriteLine("Register");
        ScriptActions.RegisterConverter(new CXMLConv());
        //ScriptActions.RegisterImport(new CXMLImport());
    }
}

public class CXMLConv
{
    public string key = "script_cxml";
    public string title = "Custom XML";
    public string extension = "xml";
    public string type = "scriptdata";

    public string export(Stream stream, bool escape = false)
    {
        ScriptData sdata = new ScriptData (new BinaryReader(stream));

		return (new CustomXMLNode("table", (Dictionary<string, object>)sdata.Root, "")).ToString(0, escape);
    }
}

/*public class CXMLImport
{
    public string title = "Custom XML";
    public string label = "Custom XML (.custom_xml)";
    public string filter = "Custom XML|*.custom_xml";
    public string type = "scriptdata";

    public ScriptData execute(string filepath)
    {
        
    }
}*/


