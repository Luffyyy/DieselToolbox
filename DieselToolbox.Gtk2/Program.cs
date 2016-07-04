using System;
using Eto;
using Eto.Forms;

namespace DieselToolbox.Gtk2
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
            Platform plat = new Eto.GtkSharp.Platform();
            plat.Add<DragDropController.IDragDropController>(() => { return new DragDropGTK(); });
            
            new App (args, new Application(Platforms.Gtk2));
		}
	}
}
