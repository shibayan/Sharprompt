using System;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T> : IDisposable
    {
        protected FormBase(bool cursorVisible = true)
        {
            Renderer = new FormRenderer(cursorVisible);
        }

        protected FormRenderer Renderer { get; }

        public void Dispose()
        {
            Renderer.Dispose();
        }

        public T Start()
        {
            while (true)
            {
                Renderer.Render(InputTemplate);

                if (!TryGetResult(out var result))
                {
                    continue;
                }

                Renderer.Render(FinishTemplate, result);

                return result;
            }
        }

        protected abstract bool TryGetResult(out T result);

        protected virtual void InputTemplate(FormRenderer formRenderer)
        {
        }

        protected virtual void FinishTemplate(FormRenderer formRenderer, T result)
        {
        }
    }
}
