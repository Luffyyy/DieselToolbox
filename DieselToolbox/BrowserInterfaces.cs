using DieselEngineFormats.Bundle;
using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;

namespace DieselToolbox
{
    public interface IViewable
    {
        Icon Icon { get; }
        string Name { get; }
        string Type { get; }
        string Size { get; }
        string Path { get; }
    }

    public interface IChild
    {
        IParent Parent { get; set; }
    }

    public interface IParent
    {
        bool ContainsAnyBundleEntries(Idstring package = null);
        void AddToTree(TreeItem item, Idstring pck = null);
        void GetSubFileEntries(Dictionary<string, FileEntry> fileList);
        SortedDictionary<string, IChild> Children { get; set; }
        List<object> ChildObjects(Idstring ids = null);
    }

}