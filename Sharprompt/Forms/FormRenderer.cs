using System;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms;

internal class FormRenderer : IDisposable
{
    public FormRenderer(IConsoleDriver consoleDriver)
    {
        _offscreenBuffer = new OffscreenBuffer(consoleDriver);
    }

    private readonly OffscreenBuffer _offscreenBuffer;

    public string ErrorMessage { get; set; }

    public void Dispose() => _offscreenBuffer.Dispose();

    public void Cancel() => _offscreenBuffer.Cancel();

    public void Render(Action<OffscreenBuffer> template)
    {
        using (_offscreenBuffer.BeginRender())
        {
            template(_offscreenBuffer);

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                _offscreenBuffer.WriteLine();
                _offscreenBuffer.WriteError(ErrorMessage);

                ErrorMessage = null;
            }
        }
    }

    public void Render<TModel>(Action<OffscreenBuffer, TModel> template, TModel result)
    {
        using (_offscreenBuffer.BeginRender())
        {
            template(_offscreenBuffer, result);

            _offscreenBuffer.WriteLine();
        }
    }
}
