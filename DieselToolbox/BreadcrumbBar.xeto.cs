#pragma warning disable CS0649

using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;
using System.Linq;

namespace DieselToolbox
{
    public class BreadcrumbBar : Panel
    {
        private Button btnBack;
        private Button btnForward;
        public StackLayout stckPath;
        private Scrollable scrllPath;
        public TableLayout tblMain;

        public event EventHandler SelectedItemChanged;

        private object _SelectedItem;

        public object SelectedItem { get { return this._SelectedItem; } set { object old_value = this._SelectedItem; this._SelectedItem = value; this._SelectedItemChanged(old_value, value); } }

        private object _FocusedItem;

        public object FocusedItem { get { return this._FocusedItem; } set { this._FocusedItem = value; this._FocusedItemChanged(); } }

        private int _SelectedIndex;

        public int SelectedIndex { get { return this._SelectedIndex; } set { int old_value = this._SelectedIndex; this._SelectedIndex = value; this._SelectedIndexChanged(old_value, value); } }

        protected List<object> ItemTree = new List<object>();

        public BreadcrumbBar()
        {
            XamlReader.Load(this);

        }

        protected void _FocusedItemChanged()
        {
            if (this.FocusedItem is IChild)
            {
                this.ItemTree = ((IChild)this.FocusedItem).ParentTree();
                this.ItemTree.Reverse();
            }
            else
                this.ItemTree = new List<object>() { FocusedItem };

            this.stckPath.Items.Clear();
            this.SelectedIndex = 0;
            this.SelectedIndex = this.ItemTree.Count() - 1;
        }

        protected void _SelectedItemChanged(object old, object new_value)
        {
            this.SelectedIndex = this.ItemTree.IndexOf(new_value);
            this.SelectedItemChanged.Invoke(this, null);
        }

        protected void _SelectedIndexChanged(int old, int new_value)
        {
            if (new_value < old)
            {
                for (int i = new_value + 1; i <= old && this.stckPath.Items.Count > 0; i++)
                    this.stckPath.Items.RemoveAt(this.stckPath.Items.Count - 1);
            }
            else if (new_value >= old)
            {
                for (int i = this.stckPath.Items.Count; i <= new_value; i++)
                {
                    Button btn = new Button { Text = (this.ItemTree[i] as IViewable).Name, Tag = i, BackgroundColor = Colors.White };
                    btn.MinimumSize = new Size(20, 0);
                    btn.Click += this.btn_item_Click;
                    this.stckPath.Items.Add(new StackLayoutItem(btn) { VerticalAlignment = VerticalAlignment.Stretch });
                }
            }

            this.scrllPath?.UpdateScrollSizes();
            this._SelectedItem = this.ItemTree[this.SelectedIndex];

        }

        void btnBack_Click(object sender, EventArgs e)
        {
            if (this.FocusedItem == null)
                return;

            this.SelectedIndex = Math.Max(this.SelectedIndex-1, 0);
            this.SelectedItemChanged.Invoke(this, null);
        }

        void btnForward_Click(object sender, EventArgs e)
        {
            if (this.FocusedItem == null)
                return;

            this.SelectedIndex = Math.Min(this.SelectedIndex + 1, this.ItemTree.Count - 1);
            this.SelectedItemChanged.Invoke(this, null);
        }

        void btn_item_Click(object sender, EventArgs e)
        {
            this.SelectedIndex = (int)((Button)sender).Tag;
            this.SelectedItemChanged.Invoke(this, null);
        }

        public override void ResumeLayout()
        {
            base.ResumeLayout();
            this.btnBack.Width = this.btnBack.Height;
            this.btnForward.Width = this.btnForward.Height;
        }

        void btnSizeChanged(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.Width = btn.Height;
        }
    }
}
