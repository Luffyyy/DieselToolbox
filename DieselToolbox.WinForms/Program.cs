using System;
using Eto;
using Eto.Forms;

namespace DieselToolbox.WinForms
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			new Application (Platforms.WinForms).Run (new MainForm ());
		}
	}
}
