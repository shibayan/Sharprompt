using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sharprompt
{
    public struct PathSelected
    {
        public PathSelected(string folder, string vale,bool notfound)
        {
            PathValue = folder;
            SelectedValue = vale;
            NotFound = notfound;
            IsFile = false;
            AliasSelected = SelectedValue;
        }

        internal PathSelected(string folder, string vale, bool notfound, bool isfile, bool showMarkup, bool showpath)
        {
            PathValue = folder;
            SelectedValue = vale;
            NotFound = notfound;
            IsFile = isfile;
            var prefix = "";
            if (showMarkup)
            {
                if (isfile)
                {
                    prefix = Prompt.Symbols.File.ToString();
                }
                else
                {
                    prefix = Prompt.Symbols.Folder.ToString();
                }
            }
            if (showpath)
            {
                AliasSelected = $"{prefix} {Path.Combine(PathValue, SelectedValue)}";
            }
            else
            {
                AliasSelected = $"{prefix} {SelectedValue}";
            }
        }
        public string PathValue { get; }
        public string SelectedValue { get; }
        public bool NotFound { get; }
        internal bool IsFile { get; }
        internal string AliasSelected { get; }
    }
}
