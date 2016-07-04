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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFontModder_Click(object sender, RoutedEventArgs e)
        {
            FontEditor fntEdit = new FontEditor();
            fntEdit.Show();
            this.Close();
        }

        private void btnBundleBrowser_Click(object sender, RoutedEventArgs e)
        {
            BrowserWindow brws = new BrowserWindow();
            brws.Show();
            this.Close();
        }

        private void btnStringsViewer_Click(object sender, RoutedEventArgs e)
        {
            StringViewer view = new StringViewer();
            view.Show();
            this.Close();
        }
    }
}
