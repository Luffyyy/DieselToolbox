using CSharpImageLibrary.General;
using DieselEngineFormats.Font;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace DieselToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FontEditor : Window
    {
        private DieselFont _selectedFont;

        public DieselFont selectedFont
        {
            get { return _selectedFont; }
            set
            {
                _selectedFont = value;
                this.ReloadFont();
            }
        }

        private ImageEngineImage _selectedImage;

        public ImageEngineImage selectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                this.ReloadFont();
            }
        }

        private int _scale = 1;

        public int Scale{
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                this.ReloadFont();
            }
        }

        public FontEditor()
        {
            InitializeComponent();
        }

        public FontEditor(string fontPath = null, string texturePath = null)
        {
            InitializeComponent();

            if (File.Exists(fontPath))
                this.selectedFont = new DieselFont(fontPath);

            if (File.Exists(texturePath))
                using (FileStream str = new FileStream(texturePath, FileMode.Open, FileAccess.Read))
                    this.selectedImage = new ImageEngineImage(str);
        }

        public FontEditor(Stream fontStream = null, Stream textureSteam = null)
        {
            InitializeComponent();

            if (fontStream != null)
                this.selectedFont = new DieselFont(fontStream);

            if (textureSteam != null)
                this.selectedImage = new ImageEngineImage(textureSteam);
        }

        private void LoadDieselFont_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Diesel Font File|*.font";
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = true;
            bool select = (bool)dlg.ShowDialog();
            if (select)
            {
                selectedFont = new DieselFont(dlg.FileName);
                this.Title = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                if (!File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) + ".texture")))
                {
                    OpenFileDialog textureDLG = new OpenFileDialog();
                    textureDLG.Multiselect = false;
                    textureDLG.Filter = "Texture|*.texture;*.dds;*.jpg;*.png;*.bmp";
                    textureDLG.CheckPathExists = true;
                    textureDLG.CheckFileExists = true;
                    bool textureSelected = (bool)textureDLG.ShowDialog();
                    if (textureSelected)
                    {
                        using (FileStream str = new FileStream(textureDLG.FileName, FileMode.Open, FileAccess.Read))
                        {
                            selectedImage = new ImageEngineImage(str);
                        }
                        
                    }
                }
                else
                {
                    using (FileStream str = new FileStream(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) + ".texture"), FileMode.Open, FileAccess.Read))
                    {
                        selectedImage = new ImageEngineImage(str);
                    }
                }
            }
        }

        private void LoadBMFontXMLFont_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "BMFont|*.fnt";
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = true;
            bool select = (bool)dlg.ShowDialog();
            if (select)
            {
                selectedFont = new DieselFont(XmlReader.Create(dlg.FileName));
                this.Title = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                //Check for page paramater in BMFont XML
                if (!File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) + ".texture")))
                {
                    OpenFileDialog textureDLG = new OpenFileDialog();
                    textureDLG.Multiselect = false;
                    textureDLG.Filter = "Texture|*.texture;*.dds;*.jpg;*.png;*.bmp";
                    textureDLG.CheckPathExists = true;
                    textureDLG.CheckFileExists = true;
                    bool textureSelected = (bool)textureDLG.ShowDialog();
                    if (textureSelected)
                    {
                        using (FileStream str = new FileStream(textureDLG.FileName, FileMode.Open, FileAccess.Read))
                        {
                            selectedImage = new ImageEngineImage(str);
                        }

                    }
                }
                else
                {
                    using (FileStream str = new FileStream(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) + ".texture"), FileMode.Open, FileAccess.Read))
                    {
                        selectedImage = new ImageEngineImage(str);
                    }
                }
            }
        }
        
        public void ReloadFont()
        {
            if (this.selectedFont == null)
                return;

            this.lstCharacters.ItemsSource = this.selectedFont.Characters;
            this.txtName.Text = this.selectedFont.Name;

            if (selectedImage != null)
            {
                this.imgFontTexture.Source = selectedImage.GetWPFBitmap();
                this.grdImage.Width = this.imgFontTexture.Source.Width * this.Scale;
                this.grdImage.Height = this.imgFontTexture.Source.Height * this.Scale;
                //this.imgFontTexture.Source = null;

                this.grdImage.Children.Clear();
                this.grdImage.Children.Add(this.imgFontTexture);

                foreach (FontCharacter fontChar in this.selectedFont.Characters)
                {
                    Grid grdChar = new Grid { 
                        Tag = fontChar,
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(fontChar.X * this.Scale, fontChar.Y * this.Scale, 0, 0),
                        Height = fontChar.H * this.Scale,
                        Width = fontChar.W * this.Scale
                    };
                    grdChar.MouseDown += rec_MouseDown;
                    fontChar.Tag = grdChar;

                    Rectangle rec = new Rectangle { 
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Fill = new SolidColorBrush(Colors.Transparent),
                        Stroke = new SolidColorBrush(Colors.Transparent),
                    };
                    grdChar.Children.Add(rec);

                    Border brd = new Border { 
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderBrush = SystemColors.WindowBrush,
                        BorderThickness = new Thickness(2)
                    };

                    grdChar.Children.Add(brd);

                    this.grdImage.Children.Add(grdChar);
                }

                this.lstCharacters.Focus();
            }
        }

        private void rec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid grd = sender as Grid;
            if (grd != null)
            {
                this.lstCharacters.SelectedItem = grd.Tag;
                (grd.Children[1] as Border).BorderBrush = SystemColors.HighlightBrush;
            }
        }

        private void lstCharacters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (FontCharacter ctrl in e.RemovedItems)
            {
                if (ctrl.Tag != null)
                    ((Border)((Grid)ctrl.Tag).Children[1]).BorderBrush = SystemColors.WindowBrush;
            }

            foreach (FontCharacter ctrl in e.AddedItems)
            {
                if (ctrl.Tag != null)
                    ((Border)((Grid)ctrl.Tag).Children[1]).BorderBrush = SystemColors.HighlightBrush;
            }
        }

        private void SaveDieselFont_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDiag = new SaveFileDialog { 
                AddExtension = true,
                DefaultExt = "font",
                OverwritePrompt = true
            };
            saveDiag.Filter = "Diesel Font File|*.font";
            if((bool)saveDiag.ShowDialog())
            {
                using (FileStream str = new FileStream(saveDiag.FileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(str))
                    {
                        this.selectedFont.WriteDieselData(bw);
                    }
                }
            }

        }

        private void ZoomClick(object sender, RoutedEventArgs e)
        {
            foreach (ItemsControl ctrl in this.mitemZoom.Items)
            {
                if (ctrl != sender)
                    ((MenuItem)ctrl).IsChecked = false;
            }

            MenuItem item = sender as MenuItem;
            
            if (item != null && item.IsChecked)
                this.Scale = int.Parse((string)item.Header);
        }
    }
}
