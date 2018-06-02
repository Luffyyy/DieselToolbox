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
using DieselEngineFormats.BNK;

public class main
{
    public void register()
    {
        Console.WriteLine("Register Conv");
        ScriptActions.RegisterConverter(new ParserTesting());
        ScriptActions.RegisterConverter(new StringsView());
    }
}

public class ParserTesting
{
    public string key = "parser_testing";
    public string title = "Parser Testing";
    public string extension = "banksinfo_decompile";
    public string type = "banksinfo";

    public Stream export(Stream str)
    {
        //HashIndex.GenerateHashList("./FullHashlist");
        BanksInfo bnk = new BanksInfo(str);
        return str;
    }
}

public class StringsView
{
    public string key = "strings_view";
    public string title = "Strings view";
    public string extension = "strings";
    public string type = "strings";

    public Stream export(Stream str)
    {
        //HashIndex.GenerateHashList("./FullHashlist");
        StringViewer view = new StringViewer(str);
        view.Show();
        return str;
    }
}
