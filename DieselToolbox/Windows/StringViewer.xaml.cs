using DieselEngineFormats;
using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DieselToolbox
{
    /// <summary>
    /// Interaction logic for StringViewer.xaml
    /// </summary>
    public partial class StringViewer : Window
    {
        

        public void UpdateHashes(string path)
        {
            BundleDatabase db = new BundleDatabase();
            db.Load(path);

            this.Hashes = GeneralUtils.GetHashlist(System.IO.Path.GetDirectoryName(path), db);
        }
    }
}
