using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible = true)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly bool _cursorVisible;
        private readonly ConsoleRenderer _renderer = new ConsoleRenderer();

        private string _errorMessage;

        public void Dispose()
        {
            _renderer.Close();

            Console.CursorVisible = true;
        }

        public void Beep()
        {
            Console.Beep();
        }

        public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
        {
            return CancellableFunction
            (
                () => Console.ReadKey(true),
                CancelStandardInput,
                cancellationToken
            );
        }

        public string ReadLine(CancellationToken cancellationToken)
        {
            var left = Console.CursorLeft;

            var line = CancellableFunction(Console.ReadLine, CancelStandardInput, cancellationToken);

            if (line != null)
            {
                Console.SetCursorPosition(left, Console.CursorTop - 1);
            }

            return line;
        }

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

        public void Render<TModel>(Action<ConsoleRenderer, TModel> template, TModel model)
        {
            Console.CursorVisible = false;

            _renderer.Reset();

            template(_renderer, model);

            if (_errorMessage != null)
            {
                _renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            Console.CursorVisible = _cursorVisible;
        }

        private static void CancelStandardInput()
        {
            const int STD_INPUT_HANDLE = -10;
            CancelIoEx(GetStdHandle(STD_INPUT_HANDLE), IntPtr.Zero);
        }

        private static T CancellableFunction<T>(Func<T> function, Action cancelAction, CancellationToken cancellationToken)
        {
            using (var functionReturned = new ManualResetEvent(false))
            {
                T result = default;
                bool functionCancelled;
                Exception functionException = null;
                Exception cancelException = null;

                // Spawn a separate task that waits for one of two events
                //   1. The observed function finishes
                //   2. The cancellationToken is set.
                //
                //   If the cancellationToken is set the cancelAction is called.
                var bgTask = Task.Run(() => {
                    var handleIndex = WaitHandle.WaitAny(new WaitHandle[] { functionReturned, cancellationToken.WaitHandle });
                    functionCancelled = handleIndex != 0;

                    if (functionCancelled)
                    {
                        // Catch an exception that might happen, while canceling
                        try { cancelAction(); }
                        catch (Exception ex) { cancelException = ex; }
                    }
                });

                // Catch an exception that might happen, while running the function
                try { result = function(); }
                catch (Exception ex) { functionException = ex; }

                functionReturned.Set();
                bgTask.Wait();

                var combinedException = CombineExceptions(functionException, cancelException);

                // if the operation was canceled because of the token
                // throw an OperationCanceledException
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("The operation was canceled", combinedException, cancellationToken);

                // if an exception was thrown without canceling
                // throw an InvalidOperationException
                if (combinedException != null)
                    throw new InvalidOperationException("Operation did not complete successfully", combinedException);

                // if no exception was thrown and operation was not canceled
                // return the result of the function
                return result;
            }
        }

        private static Exception CombineExceptions(params Exception[] exceptions)
        {
            var reduced = exceptions.Where(ex => ex != null).ToList();

            if (reduced.Count < 1)
                return null;

            if (reduced.Count == 1)
                return reduced[0];

            return new AggregateException(reduced);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CancelIoEx(IntPtr handle, IntPtr lpOverlapped);
    }
}