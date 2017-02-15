using Eto;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieselToolbox
{
    [Handler(typeof(IDragDropController))]
    public class DragDropController : Control
    {
        new IDragDropController Handler { get { return (IDragDropController)base.Handler; } }

        public bool OutputFullPaths
        {
            get { return this.Handler.OutputFullPaths; }
            set { this.Handler.OutputFullPaths = value; }
        }

        public void DoDragDrop(IParent parent)
        {
            this.Handler.DoDragDrop(parent);
        }

        public void DoDragDrop(IChild child)
        {
            this.Handler.DoDragDrop(child);
        }

        public interface IDragDropController : Control.IHandler
        {
            bool OutputFullPaths { get; set; }
            void DoDragDrop(IParent parent);
            void DoDragDrop(IChild child);
            //string MyProperty { get; set; }
            //DragDropEffects DoDragDrop(object dragSource, System.Runtime.InteropServices.ComTypes.IDataObject dataObject, DragDropEffects allowedEffects);
        }
    }
}
