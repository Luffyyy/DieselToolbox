using System;
using Eto;
using Eto.Forms;

namespace DieselToolbox.Gtk3
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
            Platform plat = new Eto.GtkSharp.Platform();
			new App (args, new Application(plat));
		}
	}
}
