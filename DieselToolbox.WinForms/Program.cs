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
			new Application (Eto.Platform.Detect).Run (new frmPackageBrowser());
		}
	}
}
