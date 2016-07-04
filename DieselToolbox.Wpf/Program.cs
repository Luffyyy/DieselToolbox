using System;
using Eto;
using Eto.Forms;

namespace DieselToolbox.Wpf
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
            Platform plat = new Eto.Wpf.Platform();
            plat.Add<DragDropController.IDragDropController>(() => { return new DragDropWPF(); });
            new App (args, new Application(plat));
		}
	}
}
