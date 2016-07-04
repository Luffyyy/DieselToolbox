using System;
using System.IO;
using System.Xml.Serialization;

namespace DieselToolbox
{
	public class SettingsData
	{
		public bool StoreLocalHashlists = true;

        public bool ExtractFullFileStructure = false;
	}

	public class Settings
	{
		private const string _save_path = "./settings.xml";

		public SettingsData Data { get; set;}

		public Settings ()
		{
			this.Load();	
		}

		public void Load()
		{
			if (!File.Exists (_save_path)) {
				this.Data = new SettingsData();
				this.Save();
			} else {
				var serializer = new XmlSerializer(typeof(SettingsData));
				using (var fs = new FileStream(_save_path, FileMode.Open, FileAccess.Read))
					this.Data = (SettingsData)serializer.Deserialize(fs);
			}
		}

		public void Save()
		{
			var serializer = new XmlSerializer(typeof(SettingsData));

			if (!File.Exists(_save_path))
				File.Create(_save_path).Close();
			
			using (var fs = new FileStream(_save_path, FileMode.Truncate, FileAccess.Write))
				serializer.Serialize(fs, this.Data);
		}
	}
}
