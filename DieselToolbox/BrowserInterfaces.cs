﻿using DieselEngineFormats.Bundle;
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

    public static class ChildExt
    {
        public static List<object> ParentTree(this IChild self)
        {
            List<object> objs = new List<object>();
            object obj = self;
            while (obj != null)
            {
                objs.Add(obj);
                if (obj is IChild)
                    obj = ((IChild)obj).Parent;
                else
                    break;
            }

            return objs;
        }
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