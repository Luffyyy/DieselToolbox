#pragma warning disable CS0649

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

		private Dictionary<string, FormatConverter> Exporters;

		public FormatConverter SelectedExporter { get; set; }

		public SaveOptionsDialog (string type)
		{
			XamlReader.Load (this);

			this.cmbOptions.Items.Add ("Default");
			this.cmbOptions.SelectedIndex = 0;

			this.cmbOptions.SelectedIndexChanged += (object sender, EventArgs e) => {
                this.SelectedExporter = this.cmbOptions.SelectedIndex == 0 ? null : this.Exporters[this.cmbOptions.SelectedKey];
			};

			if (ScriptActions.Converters.ContainsKey (type))
				this.RegisterExporters (ScriptActions.Converters[type]);
		}

		public void RegisterExporters(Dictionary<string, FormatConverter> exporters)
		{
			this.Exporters = new Dictionary<string, FormatConverter>();

			foreach (KeyValuePair<string, FormatConverter> dyn in exporters)
				this.RegisterExporter (dyn.Key, dyn.Value);
		}

		public void RegisterExporter(string key, FormatConverter obj)
		{
            try
            {
                this.cmbOptions.Items.Add(obj.Title, key);
                this.Exporters.Add(key, obj);
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

