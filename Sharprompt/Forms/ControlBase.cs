using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal abstract class FormBase<T>
    {
        protected FormBase(bool cursorVisible = true)
        {
            Scope = new ConsoleScope(cursorVisible);
        }

        protected ConsoleScope Scope { get; }

        public abstract T Start();
    }
}
