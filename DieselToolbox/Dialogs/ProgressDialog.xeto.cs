using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace DieselToolbox
{
	public class ProgressDialog : Dialog
	{
		public Label lblProgressString;
		public ProgressDialog ()
		{
			XamlReader.Load (this);
		}
	}
}