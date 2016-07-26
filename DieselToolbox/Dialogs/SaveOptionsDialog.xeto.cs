using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace DieselToolbox
{
	public class SaveOptionsDialog : Dialog<DialogResult>
	{
		DropDown cmbOptions;

		private Dictionary<string, dynamic> Exporters;

		public dynamic SelectedExporter { get; set; }

		public SaveOptionsDialog (string type)
		{
			XamlReader.Load (this);

			this.cmbOptions.Items.Add ("Default");
			this.cmbOptions.SelectedIndex = 0;

			this.cmbOptions.SelectedIndexChanged += (object sender, EventArgs e) => {
				SelectedExporter = this.cmbOptions.SelectedIndex == 0 ? null : Exporters[this.cmbOptions.SelectedKey];
			};

			if (ScriptActions.Converters.ContainsKey (type))
				this.RegisterExporters (ScriptActions.Converters[type]);
		}

		public void RegisterExporters(Dictionary<string, dynamic> exporters)
		{
			this.Exporters = new Dictionary<string, dynamic>();

			foreach (KeyValuePair<string, dynamic> dyn in exporters)
				this.RegisterExporter (dyn.Key, dyn.Value);
		}

		public void RegisterExporter(string key, dynamic obj)
		{
            try
            {

                if ((obj.GetType().GetMethod("export") != null) || obj.export != null)
                {
                    this.cmbOptions.Items.Add(obj.title, key);
                    this.Exporters.Add(key, obj);

                    //if (obj.options != null) {

                    //}
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

		public void FinishedButtonClick(object sender, EventArgs e)
		{
			this.Result = DialogResult.Yes;
			this.Close ();
		}

		public void CloseButtonClick(object sender, EventArgs e)
		{
			this.Result = DialogResult.Cancel;
			this.Close ();
		}
	}
}

