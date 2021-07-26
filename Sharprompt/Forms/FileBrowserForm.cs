using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{

    internal class FileBrowserForm : FormBase<PathSelected>
    {
        private readonly PathOptions _options;
        private readonly StringBuilder _filterBuffer = new StringBuilder();
        private PathSelected _defaultopt;
        private string _currentFolder;
        private Paginator<PathSelected> _paginator;

        public FileBrowserForm(PathOptions options) : base(false)
        {
            _options = options;
            switch (_options.BrowserChoose)
            {
                case FileBrowserChoose.File:
                    InitFilebrowser();
                    break;
                case FileBrowserChoose.Folder:
                    InitFolderbrowser();
                    break;
                default:
                    throw new NotImplementedException($"BrowserChoose {_options.BrowserChoose} Not Implemented");
            }
        }

        private void InitFilebrowser()
        {
            var defvalue = _options.DefaultValue;
            if (!string.IsNullOrEmpty(defvalue) && !IsValidFile(defvalue))
            {
                throw new ArgumentException("Default file not found or invalid");
            }
            if (!string.IsNullOrEmpty(_options.RootFolder) && !IsValidDirectory(_options.RootFolder))
            {
                throw new ArgumentException("Root folder not found or invalid");
            }
            if (!string.IsNullOrEmpty(defvalue))
            {
                var fi = new FileInfo(defvalue);
                _defaultopt = new PathSelected(fi.Directory?.FullName ?? "", fi.Name, false, true, _options.ShowMarkup, !_options.PromptCurrentPath);
                _currentFolder = fi.Directory.FullName;
            }
            else
            {
                var di = new DirectoryInfo(Directory.GetCurrentDirectory());
                _defaultopt = new PathSelected(di.FullName ?? "", "", true, true, _options.ShowMarkup, !_options.PromptCurrentPath);
                _currentFolder = di.FullName;
            }
            _paginator = new Paginator<PathSelected>(ItensFolders(_defaultopt.PathValue), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
        }

        private void InitFolderbrowser()
        {
            var defvalue = _options.DefaultValue;
            if (string.IsNullOrEmpty(defvalue))
            {
                defvalue = Directory.GetCurrentDirectory();
            }
            if (!IsValidDirectory(defvalue))
            {
                throw new ArgumentException("Default Folder not found or invalid");
            }
            if (!string.IsNullOrEmpty(_options.RootFolder) && !IsValidDirectory(_options.RootFolder))
            {
                throw new ArgumentException("Root folder not found or invalid");
            }
            var di = new DirectoryInfo(defvalue);
            _defaultopt = new PathSelected(di.Parent?.FullName ?? "", di.Name, false, false, _options.ShowMarkup, !_options.PromptCurrentPath);
            _currentFolder = defvalue;
            _paginator = new Paginator<PathSelected>(ItensFolders(defvalue), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, PathSelected result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(_options.ResultSelector(result), Prompt.ColorSchema.Answer);
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            var prompt = $"{ _options.Message}";
            if (_options.SearchPattern != "*" && _options.PromptSearchPattern)
            {
                prompt += $" ({_options.SearchPattern})";
            }
            if (_options.PromptCurrentPath)
            {
                prompt += $" | {_currentFolder}";
            }
            screenBuffer.WritePrompt(prompt);
            screenBuffer.Write(_paginator.FilterTerm);
            if (_options.DefaultValue != null && _options.StartWithDefaultValue)
            {
                if (_paginator.TryGetSelectedItem(out var result))
                {
                    screenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
                }
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

                screenBuffer.WriteLine();

                if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<PathSelected>.Default.Equals(item, selectedItem))
                {
                    screenBuffer.Write($"{Prompt.Symbols.Selector} {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    screenBuffer.Write($"  {value}");
                }
            }
            if (_options.ShowPagination)
            {
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.WriteLine();
                    screenBuffer.Write($"({_paginator.TotalCount} items, {_paginator.SelectedPage + 1}/{_paginator.PageCount} pages)");
                }
            }
        }

        protected override bool TryGetResult(CancellationToken cancellationToken, out PathSelected result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when _options.BrowserChoose == FileBrowserChoose.File:
                    {
                        if (_paginator.TryGetSelectedItem(out var resultpreview))
                        {
                            if (resultpreview.IsFile)
                            {
                                result = new PathSelected(_currentFolder, resultpreview.SelectedValue, false, false, false, !_options.PromptCurrentPath);
                                return true;
                            }
                            break;
                        }
                        var newfile = _filterBuffer.ToString();
                        if (string.IsNullOrEmpty(newfile) && !_options.AllowNotSelected)
                        {
                            SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultRequiredMessage));
                            break;
                        }
                        if (!string.IsNullOrEmpty(_options.PrefixExtension))
                        {
                            if (!newfile.ToLower().EndsWith(_options.PrefixExtension))
                            {
                                newfile += _options.PrefixExtension;
                            }
                        }
                        result = new PathSelected(_currentFolder, newfile, true, true, false, !_options.PromptCurrentPath);
                        return true;
                    }
                    case ConsoleKey.Enter when _options.BrowserChoose == FileBrowserChoose.Folder:
                    {
                        if (_paginator.TryGetSelectedItem(out var resultpreview))
                        {
                            result = new PathSelected(_currentFolder, resultpreview.SelectedValue, false, false, false, !_options.PromptCurrentPath);
                            return true;
                        }
                        var newfolder = _filterBuffer.ToString();
                        if (string.IsNullOrEmpty(newfolder) && !_options.AllowNotSelected)
                        {
                            SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultRequiredMessage));
                            break;
                        }
                        if (!string.IsNullOrEmpty(_options.PrefixExtension))
                        {
                            if (!newfolder.ToLower().EndsWith(_options.PrefixExtension))
                            {
                                newfolder += _options.PrefixExtension;
                            }
                        }
                        result = new PathSelected(_currentFolder, newfolder,true,false,false, !_options.PromptCurrentPath);
                        return true;
                    }
                    case ConsoleKey.UpArrow:
                        _paginator.PreviousItem();
                        break;
                    case ConsoleKey.DownArrow:
                        _paginator.NextItem();
                        break;
                    case ConsoleKey.LeftArrow when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && _options.BrowserChoose == FileBrowserChoose.File:
                    {
                        if (_currentFolder == _options.RootFolder)
                        {
                            break;
                        }
                        var di = new DirectoryInfo(_currentFolder);
                        if (di.Parent == null)
                        {
                            break;
                        }
                        _currentFolder = di.Parent.FullName;
                        _paginator = new Paginator<PathSelected>(ItensFolders(di.Parent.FullName), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
                        break;
                    }

                    case ConsoleKey.LeftArrow when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && _options.BrowserChoose == FileBrowserChoose.Folder:
                    {
                        if (_currentFolder == _options.RootFolder)
                        {
                            break;
                        }
                        var di = new DirectoryInfo(_currentFolder);
                        if (di.Parent == null)
                        {
                            break;
                        }
                        _currentFolder = di.Parent.FullName;
                        _paginator = new Paginator<PathSelected>(ItensFolders(di.Parent.FullName), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
                        break;
                    }
                    case ConsoleKey.RightArrow when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && _options.BrowserChoose == FileBrowserChoose.File:
                    {
                        if (_paginator.TryGetSelectedItem(out var resultpreview))
                        {
                            if (!resultpreview.IsFile)
                            {
                                if (IsDirectoryHasChilds(Path.Combine(_currentFolder, resultpreview.SelectedValue)))
                                {
                                    _currentFolder = Path.Combine(_currentFolder, resultpreview.SelectedValue);
                                    _paginator = new Paginator<PathSelected>(ItensFolders(_currentFolder), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
                                }
                            }
                        }
                        break;
                    }
                    case ConsoleKey.RightArrow when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && _options.BrowserChoose == FileBrowserChoose.Folder:
                    {
                        if (_paginator.TryGetSelectedItem(out var resultpreview))
                        {
                            if (IsDirectoryHasChilds(Path.Combine(_currentFolder,resultpreview.SelectedValue)))
                            {
                                _currentFolder = Path.Combine(_currentFolder, resultpreview.SelectedValue);
                                _paginator = new Paginator<PathSelected>(ItensFolders(_currentFolder), _options.PageSize, Optional<PathSelected>.Create(_defaultopt), _options.TextSelector);
                            }
                        }
                        break;
                    }
                    case ConsoleKey.RightArrow:
                        _paginator.NextPage();
                        break;
                    case ConsoleKey.Backspace when _filterBuffer.Length == 0:
                        ConsoleDriver.Beep();
                        break;
                    case ConsoleKey.Backspace:
                        _filterBuffer.Length -= 1;
                        _paginator.UpdateFilter(_filterBuffer.ToString());
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Delete:
                        ConsoleDriver.Beep();
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar))
                            {
                                _filterBuffer.Append(keyInfo.KeyChar);

                                _paginator.UpdateFilter(_filterBuffer.ToString());
                            }
                        }
                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested);

            result = default;

            return false;
        }

        private IEnumerable<PathSelected> ItensFolders(string folder)
        {
            switch (_options.BrowserChoose)
            {
                case FileBrowserChoose.File:
                    return LoadFileItems(folder);
                case FileBrowserChoose.Folder:
                    return LoadFolderItems(folder);
            }
            return null;
        }
        private IEnumerable<PathSelected> LoadFileItems(string folder)
        {
            var result = new List<PathSelected>();
            foreach (var item in Directory.GetFiles(folder, _options.SearchPattern))
            {
                if (IsValidFile(item))
                {
                    var fi = new FileInfo(item);
                    result.Add(new PathSelected(fi.Directory?.FullName ?? "", fi.Name, false, true, _options.ShowMarkup, !_options.PromptCurrentPath));
                }
            }
            foreach (var item in Directory.GetDirectories(folder))
            {
                if (IsValidDirectory(item))
                {
                    var di = new DirectoryInfo(item);
                    result.Add(new PathSelected(di.Parent?.FullName ?? "", di.Name, false, false, _options.ShowMarkup, !_options.PromptCurrentPath));
                }
            }
            return result.OrderBy(x => x.SelectedValue).ToArray();
        }

        private IEnumerable<PathSelected> LoadFolderItems(string folder)
        {
            var result = new List<PathSelected>();
            foreach (var item in Directory.GetDirectories(folder, _options.SearchPattern))
            {
                if (IsValidDirectory(item))
                {
                    var di = new DirectoryInfo(item);
                    result.Add(new PathSelected(di.Parent?.FullName ?? "", di.Name, false, false, _options.ShowMarkup, !_options.PromptCurrentPath));
                }
            }
            return result.OrderBy(x => x.SelectedValue).ToArray();
        }

        private bool IsDirectoryHasChilds(string folder)
        {
            try
            {
                return Directory.GetDirectories(folder,_options.SearchPattern).Where(x => IsValidDirectory(x)).Any();
            }
            catch (SecurityException)
            {
                //done - skip
            }
            catch (UnauthorizedAccessException)
            {
                //done - skip
            }
            return false;
        }

        private bool IsValidFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return false;
            }
            try
            {
                var fi = new FileInfo(file);
                if (fi.Attributes.HasFlag(FileAttributes.System))
                {
                    return false;
                }
                if (!IsValidDirectory(fi.Directory.FullName))
                {
                    return false;
                }
                if (_options.SupressHidden && fi.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return false;
                }
                return true;
            }
            catch (SecurityException)
            {
                //done - skip
            }
            catch (UnauthorizedAccessException)
            {
                //done - skip
            }
            return false;
        }

        private bool IsValidDirectory(string folder)
        {
            try
            {
                var di = new DirectoryInfo(folder);
                if (di.Name.StartsWith("$"))
                {
                    return false;
                }
                if (di.Attributes.HasFlag(FileAttributes.System))
                {
                    return false;
                }
                if (_options.SupressHidden && di.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return false;
                }
                return true;
            }
            catch (SecurityException)
            {
                //done - skip
            }
            catch (UnauthorizedAccessException)
            {
                //done - skip
            }
            return false;
        }
    }
}
