using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Sharprompt.Drivers;

namespace Sharprompt.Forms
{
    internal class FormRenderer : IDisposable
    {
        public FormRenderer(bool cursorVisible = true)
        {
            _cursorVisible = cursorVisible;

            _consoleDriver = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsConsoleDriver() : new DefaultConsoleDriver();
        }

        private readonly bool _cursorVisible;
        private readonly IConsoleDriver _consoleDriver;

        private string _errorMessage;

        public void Dispose()
        {
            _consoleDriver.CursorVisible = true;

            _consoleDriver.Dispose();
        }

        public void Beep()
        {
            _consoleDriver.Beep();
        }

        public ConsoleKeyInfo ReadKey() => _consoleDriver.ReadKey();

        public string ReadLine() => _consoleDriver.ReadLine();

        public void SetError(ValidationError error)
        {
            _errorMessage = error.Message;
        }

        public bool Validate(object input, IList<Func<object, ValidationError>> validators)
        {
            if (validators == null)
            {
                return true;
            }

            foreach (var validator in validators)
            {
                var error = validator(input);

                if (error != null)
                {
                    _errorMessage = error.Message;

                    return false;
                }
            }

            return true;
        }

        public void SetException(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<IConsoleDriver> template)
        {
            _consoleDriver.CursorVisible = false;

            _consoleDriver.Reset();

            template(_consoleDriver);

            if (_errorMessage != null)
            {
                _consoleDriver.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            _consoleDriver.CursorVisible = _cursorVisible;
        }

        public void Render<TModel>(Action<IConsoleDriver, TModel> template, TModel model)
        {
            _consoleDriver.CursorVisible = false;

            _consoleDriver.Reset();

            template(_consoleDriver, model);

            _consoleDriver.CursorVisible = _cursorVisible;
        }
    }
}
