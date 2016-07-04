using DieselEngineFormats.Bundle;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieselToolbox
{
    public static class StaticData
    {
		public static ScriptEngine Engine {get;set;}

		public static Settings Settings {get;set;}
    }
}
