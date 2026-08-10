using System.Runtime.InteropServices;
using Raylib_cs;

namespace VrtAimTrainer.Desktop.Features.App;

/// <summary>
/// Captures unaccelerated mouse movement through the Windows Raw Input API (<c>WM_INPUT</c>),
/// bypassing "Enhance pointer precision" pointer ballistics entirely.
/// <para>
/// raylib/GLFW only expose the OS cursor position, which has already had pointer acceleration
/// applied — unusable for an aim trainer. This class registers the raylib window for raw mouse
/// input, subclasses its window procedure to intercept <c>WM_INPUT</c>, and accumulates raw
/// device deltas that the game loop drains once per frame.
/// </para>
/// <para>
/// All accumulation happens on the main thread (raylib pumps window messages on the same thread
/// that runs the game loop), so <see cref="Drain"/> observes a consistent value; a lock is kept
/// only as cheap insurance.
/// </para>
/// </summary>
public sealed class RawMouseInput : IDisposable
{
    private const int GwlpWndProc = -4;
    private const uint WmInput = 0x00FF;
    private const uint RidInput = 0x10000003;
    private const uint RimTypeMouse = 0;
    private const ushort MouseMoveAbsolute = 0x01;
    private const ushort HidUsagePageGeneric = 0x01;
    private const ushort HidUsageGenericMouse = 0x02;
    private const uint RidevRemove = 0x00000001;

    private readonly object _gate = new();
    private readonly uint _headerSize = (uint)Marshal.SizeOf<RawInputHeader>();
    private WndProcDelegate? _hookProc;
    private nint _originalWndProc;
    private nint _windowHandle;
    private nint _rawInputBuffer;
    private int _rawInputBufferSize;
    private int _accumulatedX;
    private int _accumulatedY;

    /// <summary>True when raw input was registered and the window was subclassed successfully.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>
    /// Registers raw mouse input against the current raylib window. Must be called after
    /// <c>Raylib.InitWindow</c>. Failures leave <see cref="IsAvailable"/> false so the caller
    /// can fall back to raylib's (accelerated) delta.
    /// </summary>
    public unsafe void Initialize()
    {
        if (IsAvailable)
        {
            return;
        }

        _windowHandle = (nint)Raylib.GetWindowHandle();
        if (_windowHandle == 0)
        {
            return;
        }

        if (!RegisterDevice(_windowHandle))
        {
            return;
        }

        _rawInputBufferSize = Marshal.SizeOf<RawInput>();
        _rawInputBuffer = Marshal.AllocHGlobal(_rawInputBufferSize);

        _hookProc = HookProc;
        _originalWndProc = SetWindowLongPtr(_windowHandle, GwlpWndProc, Marshal.GetFunctionPointerForDelegate(_hookProc));
        if (_originalWndProc == 0)
        {
            Marshal.FreeHGlobal(_rawInputBuffer);
            _rawInputBuffer = 0;
            _hookProc = null;
            return;
        }

        IsAvailable = true;
    }

    /// <summary>
    /// Returns the raw device movement accumulated since the previous call and resets the accumulator.
    /// </summary>
    public (int X, int Y) Drain()
    {
        lock (_gate)
        {
            var delta = (_accumulatedX, _accumulatedY);
            _accumulatedX = 0;
            _accumulatedY = 0;
            return delta;
        }
    }

    /// <summary>
    /// Re-asserts the raw-input registration against the window. Call when a run starts: raylib's
    /// <c>DisableCursor</c>/<c>EnableCursor</c> may register or remove the mouse usage for its own
    /// relative-motion handling, and raw-input registration is per-process-per-usage, so a previous
    /// <c>EnableCursor</c> can have dropped ours.
    /// </summary>
    public void EnsureRegistered()
    {
        if (IsAvailable && _windowHandle != 0)
        {
            RegisterDevice(_windowHandle);
        }
    }

    /// <summary>Discards any accumulated movement (used when (re)entering relative aim to avoid a jump).</summary>
    public void DrainAndDiscard()
    {
        lock (_gate)
        {
            _accumulatedX = 0;
            _accumulatedY = 0;
        }
    }

    public void Dispose()
    {
        if (_windowHandle != 0 && _originalWndProc != 0)
        {
            SetWindowLongPtr(_windowHandle, GwlpWndProc, _originalWndProc);
            _originalWndProc = 0;
        }

        if (IsAvailable)
        {
            var remove = new RawInputDevice
            {
                UsUsagePage = HidUsagePageGeneric,
                UsUsage = HidUsageGenericMouse,
                DwFlags = RidevRemove,
                HwndTarget = 0,
            };
            RegisterRawInputDevices([remove], 1, (uint)Marshal.SizeOf<RawInputDevice>());
        }

        if (_rawInputBuffer != 0)
        {
            Marshal.FreeHGlobal(_rawInputBuffer);
            _rawInputBuffer = 0;
        }

        _hookProc = null;
        IsAvailable = false;
    }

    private nint HookProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WmInput)
        {
            ProcessRawInput(lParam);
            // Fall through to the original proc so DefWindowProc performs the required WM_INPUT cleanup.
        }

        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private void ProcessRawInput(nint lParam)
    {
        if (_rawInputBuffer == 0)
        {
            return;
        }

        var size = (uint)_rawInputBufferSize;
        var copied = GetRawInputData(lParam, RidInput, _rawInputBuffer, ref size, _headerSize);
        if (copied == 0 || copied == unchecked((uint)-1))
        {
            return;
        }

        var raw = Marshal.PtrToStructure<RawInput>(_rawInputBuffer);
        if (raw.Header.DwType != RimTypeMouse)
        {
            return;
        }

        // Absolute movement (tablets, some remote-desktop sessions) is not a usable relative delta.
        if ((raw.Mouse.UsFlags & MouseMoveAbsolute) != 0)
        {
            return;
        }

        lock (_gate)
        {
            _accumulatedX += raw.Mouse.LastX;
            _accumulatedY += raw.Mouse.LastY;
        }
    }

    private static bool RegisterDevice(nint windowHandle)
    {
        var device = new RawInputDevice
        {
            UsUsagePage = HidUsagePageGeneric,
            UsUsage = HidUsageGenericMouse,
            DwFlags = 0, // deliver WM_INPUT only while the window is focused
            HwndTarget = windowHandle,
        };

        return RegisterRawInputDevices([device], 1, (uint)Marshal.SizeOf<RawInputDevice>());
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterRawInputDevices([In] RawInputDevice[] pRawInputDevices, uint uiNumDevices, uint cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputData(nint hRawInput, uint uiCommand, nint pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputDevice
    {
        public ushort UsUsagePage;
        public ushort UsUsage;
        public uint DwFlags;
        public nint HwndTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInputHeader
    {
        public uint DwType;
        public uint DwSize;
        public nint HDevice;
        public nint WParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawMouse
    {
        public ushort UsFlags;
        public uint UlButtons;
        public uint UlRawButtons;
        public int LastX;
        public int LastY;
        public uint UlExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RawInput
    {
        public RawInputHeader Header;
        public RawMouse Mouse;
    }
}
