using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace DieselToolbox
{
	public class MainForm : Form
	{
		ImageView imgBundleBrowser;

		public MainForm ()
		{
			XamlReader.Load (this);
			imgBundleBrowser.Image = Definitions.FolderIcon ["closed"];
		}

		protected void OnBundleBrowserClicked (object sender, EventArgs e)
		{
			frmPackageBrowser brows = new frmPackageBrowser ();
			brows.Show ();
            this.Visible = false;
			//this.Close ();
		}

		protected void HandleQuit (object sender, EventArgs e)
		{
			Application.Instance.Quit ();
		}
	}
}

