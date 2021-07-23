using System;
using System.Collections.Generic;
using System.IO;

namespace Sharprompt
{
    public class PathOptions
    {
        public FileBrowserChoose BrowserChoose { get; set; }

        public string PrefixExtension { get; set; }

        public bool AllowNotSelected { get; set; } = false;

        public string Message { get; set; }

        public PathSelected Selected { get; internal set; }

        public string DefaultValue { get; set; }

        public string RootFolder { get; set; }

        private string _searchPattern;

        public string SearchPattern
        {
            get { return _searchPattern ?? "*"; }
            set { _searchPattern = value; }
        }

        public int? PageSize { get; set; }

        public bool SupressHidden { get; set; } = true;

        public bool PromptCurrentPath { get; set; } = true;

        public bool PromptSearchPattern { get; set; } = true;

        public bool ShowMarkup { get; set; } = true;

        internal Func<PathSelected, string> TextSelector = x => x.AliasSelected;

        internal Func<PathSelected, string> ResultSelector = x => Path.Combine(x.PathValue,x.SelectedValue);
    }
}
