using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using DesktopWorldClock.App.Diagnostics;

namespace DesktopWorldClock.App.Ui;

/// <summary>
/// Blurs the desktop behind a glass window and clips the window to the card's rounded corners.
/// Uses SetWindowCompositionAttribute because the documented Windows 11 backdrop API does not exist on Windows 10.
/// When the call fails, the card switches to an opaque fill so text stays readable.
/// </summary>
internal static class GlassBackdrop
{
    private const string FillResourceKey = "Brush.Glass.Fill";
    private const string OpaqueFillResourceKey = "Brush.Glass.FillOpaque";
    private const string RadiusResourceKey = "Radius.CardValue";

    public static void Attach(Window window)
    {
        window.SourceInitialized += (_, _) => EnableBlurOrFallBack(window);
        window.SizeChanged += (_, _) => ClipToRoundedCorners(window);
    }

    private static void EnableBlurOrFallBack(Window window)
    {
        if (TryEnableBlur(new WindowInteropHelper(window).Handle))
        {
            return;
        }

        window.Resources[FillResourceKey] = window.FindResource(OpaqueFillResourceKey);
        ErrorReporter.Log("Backdrop blur unavailable; using opaque glass fill.");
    }

    private static bool TryEnableBlur(nint hwnd)
    {
        var accent = new AccentPolicy { AccentState = AccentEnableBlurBehind };
        var size = Marshal.SizeOf<AccentPolicy>();
        var accentPointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(accent, accentPointer, fDeleteOld: false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WcaAccentPolicy,
                Data = accentPointer,
                SizeOfData = size,
            };
            return NativeMethods.SetWindowCompositionAttribute(hwnd, ref data) != 0;
        }
        finally
        {
            Marshal.FreeHGlobal(accentPointer);
        }
    }

    private static void ClipToRoundedCorners(Window window)
    {
        var source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget is null)
        {
            return;
        }

        var toDevice = source.CompositionTarget.TransformToDevice;
        var width = (int)Math.Ceiling(window.ActualWidth * toDevice.M11);
        var height = (int)Math.Ceiling(window.ActualHeight * toDevice.M22);
        var diameter = (int)Math.Round((double)window.FindResource(RadiusResourceKey) * 2 * toDevice.M11);

        var region = NativeMethods.CreateRoundRectRgn(0, 0, width + 1, height + 1, diameter, diameter);
        var hwnd = new WindowInteropHelper(window).Handle;
        // On success the system owns the region; only free it when the call fails.
        if (NativeMethods.SetWindowRgn(hwnd, region, bRedraw: true) == 0)
        {
            NativeMethods.DeleteObject(region);
            ErrorReporter.Log("SetWindowRgn failed; blur may show square corners.");
        }
    }

    private const int WcaAccentPolicy = 19;
    private const int AccentEnableBlurBehind = 3;

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public nint Data;
        public int SizeOfData;
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int SetWindowCompositionAttribute(nint hwnd, ref WindowCompositionAttributeData data);

        [DllImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern nint CreateRoundRectRgn(int left, int top, int right, int bottom, int widthEllipse, int heightEllipse);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int SetWindowRgn(nint hwnd, nint hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

        [DllImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(nint hObject);
    }
}
