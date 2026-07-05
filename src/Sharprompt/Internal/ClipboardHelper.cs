using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal;

internal static class ClipboardHelper
{
    public static string? GetText()
    {
        // Only Windows needs to read the clipboard directly: enabling
        // TreatControlCAsInput disables the legacy console's own Ctrl+V paste
        // handling, so the key arrives as a raw ^V input record. Other platforms
        // rely on the terminal emulator to translate paste into key input.
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        if (!NativeMethods.IsClipboardFormatAvailable(NativeMethods.CF_UNICODETEXT) || !NativeMethods.OpenClipboard(IntPtr.Zero))
        {
            return null;
        }

        try
        {
            var hMem = NativeMethods.GetClipboardData(NativeMethods.CF_UNICODETEXT);

            if (hMem == IntPtr.Zero)
            {
                return null;
            }

            var pointer = NativeMethods.GlobalLock(hMem);

            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringUni(pointer);
            }
            finally
            {
                NativeMethods.GlobalUnlock(hMem);
            }
        }
        finally
        {
            NativeMethods.CloseClipboard();
        }
    }
}
