using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using System.IO;
using CSharpImageLibrary.General;

namespace DieselToolbox
{
	public class FontEditor : Form
	{
        ImageView imgView;

		public FontEditor (string dds_path)
		{
			XamlReader.Load (this);
            ImageEngineImage img = new ImageEngineImage(dds_path);
            MemoryStream meme = new MemoryStream();
            img.GetGDIBitmap(false, false).Save(meme, System.Drawing.Imaging.ImageFormat.Bmp);

            Bitmap bmp = new Bitmap(meme.ToArray());
            imgView.Image = bmp;
        }
	}
}

