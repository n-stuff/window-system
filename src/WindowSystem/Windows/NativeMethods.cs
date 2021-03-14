using System;
using System.Text;

using ATOM = System.UInt16;
using BOOL = System.Int32;
using BYTE = System.Byte;
using COLORREF = System.UInt32;
using DWORD = System.UInt32;
using FXPT2DOT30 = System.Int64;
using HANDLE = System.IntPtr;
using HBITMAP = System.IntPtr;
using HBRUSH = System.IntPtr;
using HCURSOR = System.IntPtr;
using HDC = System.IntPtr;
using HDROP = System.IntPtr;
using HGDIOBJ = System.IntPtr;
using HGLOBAL = System.IntPtr;
using HICON = System.IntPtr;
using HINSTANCE = System.IntPtr;
using HMENU = System.IntPtr;
using HMONITOR = System.IntPtr;
using HRAWINPUT = System.IntPtr;
using HRESULT = System.Int32;
using HWND = System.IntPtr;
using INTRESOURCE = System.IntPtr;
using LONG = System.Int32;
using LONG_PTR = System.Int64;
using LPARAM = System.IntPtr;
using LPCWSTR = System.IntPtr;
using LPVOID = System.IntPtr;
using LPWSTR = System.IntPtr;
using LRESULT = System.IntPtr;
using SIZE_T = System.UInt64;
using UINT = System.UInt32;
using ULONG = System.UInt32;
using SHORT = System.Int16;
using USHORT = System.UInt16;
using WNDPROC = System.IntPtr;
using WORD = System.UInt16;
using WPARAM = System.UIntPtr;
using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.Windows
{
    internal class NativeMethods
    {
        internal const UINT BI_BITFIELDS = 0x0003;

        internal const UINT CF_UNICODETEXT = 13;

        internal const UINT CS_VREDRAW = 0x0001;
        internal const UINT CS_HREDRAW = 0x0002;
        internal const UINT CS_OWNDC = 0x0020;

        internal const UINT CW_USEDEFAULT = 0x80000000;

        internal const UINT DIB_RGB_COLORS = 0x00;

        internal const UINT GMEM_MOVEABLE = 0x0002;

        internal const int GWL_EXSTYLE = -20;
        internal const int GWL_STYLE = -16;

        internal const int HTCLIENT = 1;
        internal const int HTMINBUTTON = 8;
        internal const int HTMAXBUTTON = 9;
        internal const int HTCLOSE = 20;

        internal readonly static HWND HWND_TOP = new(0);
        internal readonly static HWND HWND_TOPMOST = new(-1);
        internal readonly static HWND HWND_NOTOPMOST = new(-2);

        internal readonly static INTRESOURCE IDC_ARROW = (INTRESOURCE)32512;
        internal readonly static INTRESOURCE IDC_IBEAM = (INTRESOURCE)32513;
        internal readonly static INTRESOURCE IDC_CROSS = (INTRESOURCE)32515;
        internal readonly static INTRESOURCE IDC_SIZEWE = (INTRESOURCE)32644;
        internal readonly static INTRESOURCE IDC_SIZENS = (INTRESOURCE)32645;
        internal readonly static INTRESOURCE IDC_HAND = (INTRESOURCE)32649;

        internal const UINT IMAGE_CURSOR = 2;

        internal const UINT LR_DEFAULTSIZE = 0x00000040;
        internal const UINT LR_SHARED = 0x00008000;

        internal const DWORD LWA_ALPHA = 0x00000002;

        internal const DWORD MONITOR_DEFAULTTONEAREST = 2;

        internal const USHORT MOUSE_MOVE_ABSOLUTE = 1;

        internal const DWORD MSGFLT_ALLOW = 1;

        internal readonly static INTRESOURCE OCR_NORMAL = IDC_ARROW;
        internal readonly static INTRESOURCE OCR_IBEAM = IDC_IBEAM;
        internal readonly static INTRESOURCE OCR_CROSS = IDC_CROSS;
        internal readonly static INTRESOURCE OCR_SIZEWE = IDC_SIZEWE;
        internal readonly static INTRESOURCE OCR_SIZENS = IDC_SIZENS;
        internal readonly static INTRESOURCE OCR_HAND = IDC_HAND;

        internal const UINT PM_NOREMOVE = 0;
        internal const UINT PM_REMOVE = 0x0001;

        internal const UINT QS_ALLEVENTS = 0x04BF;

        internal const UINT RID_INPUT = 0x10000003;

        internal const DWORD RIDEV_REMOVE = 0x00000001;

        internal const UINT RIM_TYPEMOUSE = 0;

        internal const int SC_KEYMENU = 0xF100;

        internal const int SW_HIDE = 0;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_MAXIMIZE = 3;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_RESTORE = 9;

        internal const UINT SWP_NOSIZE = 0x0001;
        internal const UINT SWP_NOMOVE = 0x0002;
        internal const UINT SWP_NOZORDER = 0x0004;
        internal const UINT SWP_NOACTIVATE = 0x0010;
        internal const UINT SWP_FRAMECHANGED = 0x0020;
        internal const UINT SWP_NOCOPYBITS = 0x0100;
        internal const UINT SWP_NOOWNERZORDER = 0x0200;

        internal const UINT TME_LEAVE = 2;

        internal const UINT UNICODE_NOCHAR = 0xFFFF;

        internal const int VK_RETURN = 0x0D;
        internal const int VK_SHIFT = 0x10;
        internal const int VK_CONTROL = 0x11;
        internal const int VK_MENU = 0x12;
        internal const int VK_CAPITAL = 0x14;
        internal const int VK_SNAPSHOT = 0x2C;
        internal const int VK_LWIN = 0x5B;
        internal const int VK_RWIN = 0x5C;
        internal const int VK_PROCESSKEY = 0xE5;
        internal const int VK_LSHIFT = 0xA0;
        internal const int VK_RSHIFT = 0xA1;

        internal const double WHEEL_DELTA = 120.0;

        internal const UINT WM_NULL = 0x0000;
        internal const UINT WM_MOVE = 0x0003;
        internal const UINT WM_SIZE = 0x0005;
        internal const UINT WM_SETFOCUS = 0x0007;
        internal const UINT WM_KILLFOCUS = 0x0008;
        internal const UINT WM_PAINT = 0x000F;
        internal const UINT WM_CLOSE = 0x0010;
        internal const UINT WM_QUIT = 0x0012;
        internal const UINT WM_ERASEBKGND = 0x0014;
        internal const UINT WM_SHOWWINDOW = 0x0018;
        internal const UINT WM_SETCURSOR = 0x0020;
        internal const UINT WM_MOUSEACTIVATE = 0x0021;
        internal const UINT WM_GETMINMAXINFO = 0x0024;
        internal const UINT WM_COPYGLOBALDATA = 0x0049;
        internal const UINT WM_COPYDATA = 0x004A;
        internal const UINT WM_NCPAINT = 0x0085;
        internal const UINT WM_NCACTIVATE = 0x0086;
        internal const UINT WM_INPUT = 0x00FF;
        internal const UINT WM_KEYDOWN = 0x0100;
        internal const UINT WM_KEYUP = 0x0101;
        internal const UINT WM_CHAR = 0x0102;
        internal const UINT WM_SYSKEYDOWN = 0x0104;
        internal const UINT WM_SYSKEYUP = 0x0105;
        internal const UINT WM_SYSCHAR = 0x0106;
        internal const UINT WM_UNICHAR = 0x0109;
        internal const UINT WM_SYSCOMMAND = 0x0112;
        internal const UINT WM_MOUSEMOVE = 0x0200;
        internal const UINT WM_LBUTTONDOWN = 0x0201;
        internal const UINT WM_LBUTTONUP = 0x0202;
        internal const UINT WM_RBUTTONDOWN = 0x0204;
        internal const UINT WM_RBUTTONUP = 0x0205;
        internal const UINT WM_MBUTTONDOWN = 0x0207;
        internal const UINT WM_MBUTTONUP = 0x0208;
        internal const UINT WM_MOUSEWHEEL = 0x020A;
        internal const UINT WM_XBUTTONDOWN = 0x020B;
        internal const UINT WM_XBUTTONUP = 0x020C;
        internal const UINT WM_MOUSEHWHEEL = 0x020E;
        internal const UINT WM_ENTERMENULOOP = 0x0211;
        internal const UINT WM_EXITMENULOOP = 0x0212;
        internal const UINT WM_CAPTURECHANGED = 0x0215;
        internal const UINT WM_ENTERSIZEMOVE = 0x0231;
        internal const UINT WM_EXITSIZEMOVE = 0x0232;
        internal const UINT WM_DROPFILES = 0x0233;
        internal const UINT WM_MOUSELEAVE = 0x02A3;
        internal const UINT WM_DPICHANGED = 0x02E0;

        internal const UINT WS_CAPTION = 0x00C00000;
        internal const UINT WS_CLIPCHILDREN = 0x02000000;
        internal const UINT WS_CLIPSIBLINGS = 0x04000000;
        internal const UINT WS_MAXIMIZEBOX = 0x00010000;
        internal const UINT WS_MINIMIZEBOX = 0x00020000;
        internal const UINT WS_POPUP = 0x80000000;
        internal const UINT WS_SIZEBOX = 0x00040000;
        internal const UINT WS_SYSMENU = 0x00080000;

        internal const UINT WS_EX_APPWINDOW = 0x00040000;
        internal const UINT WS_EX_CLIENTEDGE = 0x00000200;
        internal const UINT WS_EX_LAYERED = 0x00080000;
        internal const UINT WS_EX_WINDOWEDGE = 0x00000100;
        internal const UINT WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;

        internal enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        internal enum PROCESS_DPI_AWARENESS
        {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        internal delegate LRESULT WNDPROCDelegate(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct BITMAPINFO
        {
            internal BITMAPINFOHEADER bmiHeader;
            internal RGBQUAD* bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPINFOHEADER
        {
            internal DWORD biSize;
            internal LONG biWidth;
            internal LONG biHeight;
            internal WORD biPlanes;
            internal WORD biBitCount;
            internal DWORD biCompression;
            internal DWORD biSizeImage;
            internal LONG biXPelsPerMeter;
            internal LONG biYPelsPerMeter;
            internal DWORD biClrUsed;
            internal DWORD biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPV5HEADER
        {
            internal DWORD bV5Size;
            internal LONG bV5Width;
            internal LONG bV5Height;
            internal WORD bV5Planes;
            internal WORD bV5BitCount;
            internal DWORD bV5Compression;
            internal DWORD bV5SizeImage;
            internal LONG bV5XPelsPerMeter;
            internal LONG bV5YPelsPerMeter;
            internal DWORD bV5ClrUsed;
            internal DWORD bV5ClrImportant;
            internal DWORD bV5RedMask;
            internal DWORD bV5GreenMask;
            internal DWORD bV5BlueMask;
            internal DWORD bV5AlphaMask;
            internal DWORD bV5CSType;
            internal CIEXYZTRIPLE bV5Endpoints;
            internal DWORD bV5GammaRed;
            internal DWORD bV5GammaGreen;
            internal DWORD bV5GammaBlue;
            internal DWORD bV5Intent;
            internal DWORD bV5ProfileData;
            internal DWORD bV5ProfileSize;
            internal DWORD bV5Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CHANGEFILTERSTRUCT
        {
            internal DWORD cbSize;
            internal DWORD ExtStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CIEXYZ
        {
            internal FXPT2DOT30 ciexyzX;
            internal FXPT2DOT30 ciexyzY;
            internal FXPT2DOT30 ciexyzZ;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CIEXYZTRIPLE
        {
            internal CIEXYZ ciexyzRed;
            internal CIEXYZ ciexyzGreen;
            internal CIEXYZ ciexyzBlue;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ICONINFO
        {
            internal BOOL fIcon;
            internal DWORD xHotspot;
            internal DWORD yHotspot;
            internal HBITMAP hbmMask;
            internal HBITMAP hbmColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MINMAXINFO
        {
            internal POINT ptReserved;
            internal POINT ptMaxSize;
            internal POINT ptMaxPosition;
            internal POINT ptMinTrackSize;
            internal POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSG
        {
            internal HWND hwnd;
            internal UINT message;
            internal WPARAM wParam;
            internal LPARAM lParam;
            internal DWORD time;
            internal POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            internal LONG x;
            internal LONG y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUT
        {
            internal RAWINPUTHEADER header;
            internal RAWINPUTData data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            internal DWORD dwType;
            internal DWORD dwSize;
            internal HANDLE hDevice;
            internal WPARAM wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUTData
        {
            [FieldOffset(0)]
            internal RAWMOUSE mouse;
            [FieldOffset(0)]
            internal RAWKEYBOARD keyboard;
            [FieldOffset(0)]
            internal RAWHID hid;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [FieldOffset(0)]
            internal USHORT usFlags;
            [FieldOffset(4)]
            internal ULONG ulButtons;
            [FieldOffset(4)]
            internal USHORT usButtonFlags;
            [FieldOffset(6)]
            internal USHORT usButtonData;
            [FieldOffset(8)]
            internal ULONG ulRawButtons;
            [FieldOffset(12)]
            internal LONG lLastX;
            [FieldOffset(16)]
            internal LONG lLastY;
            [FieldOffset(20)]
            internal ULONG ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWKEYBOARD
        {
            internal USHORT MakeCode;
            internal USHORT Flags;
            internal USHORT Reserved;
            internal USHORT VKey;
            internal UINT Message;
            internal ULONG ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWHID
        {
            internal DWORD dwSizeHid;
            internal DWORD dwCount;
            internal BYTE bRawData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            internal USHORT usUsagePage;
            internal USHORT usUsage;
            internal DWORD dwFlags;
            internal HWND hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal LONG left;
            internal LONG top;
            internal LONG right;
            internal LONG bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RGBQUAD
        {
            internal BYTE rgbBlue;
            internal BYTE rgbGreen;
            internal BYTE rgbRed;
            internal BYTE rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TRACKMOUSEEVENT
        {
            internal DWORD cbSize;
            internal DWORD dwFlags;
            internal HWND hwndTrack;
            internal DWORD dwHoverTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            internal UINT length;
            internal UINT flags;
            internal UINT showCmd;
            internal POINT ptMinPosition;
            internal POINT ptMaxPosition;
            internal RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WNDCLASS
        {
            internal UINT style;
            internal WNDPROC lpfnWndProc;
            internal int cbClsExtra;
            internal int cbWndExtra;
            internal HINSTANCE hInstance;
            internal HICON hIcon;
            internal HCURSOR hCursor;
            internal HBRUSH hbrBackground;
            internal LPCWSTR lpszMenuName;
            internal LPCWSTR lpszClassName;
        }

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL AdjustWindowRectEx(ref RECT lpRect, DWORD dwStyle, BOOL bMenu, DWORD dwExStyle);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL BringWindowToTop(HWND hWnd);

        [DllImport("User32", SetLastError = true)]
        internal static extern unsafe BOOL ChangeWindowMessageFilterEx(HWND hWnd, UINT message, DWORD action,
            CHANGEFILTERSTRUCT* pChangeFilterStruct);

        [DllImport("User32")]
        internal static extern unsafe BOOL ClientToScreen(HWND hWnd, POINT* lpPoint);

        [DllImport("User32", SetLastError = true)]
        internal static extern unsafe BOOL ClipCursor(RECT* lpRect);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL CloseClipboard();

        [DllImport("User32", SetLastError = true, EntryPoint = "CopyIcon")]
        internal static extern HCURSOR CopyCursor(HCURSOR pcur);

        [DllImport("Gdi32")]
        internal static extern HBITMAP CreateBitmap(int nWidth, int nHeight, UINT cPlanes, UINT cBitsPerPel, IntPtr lpvBits);

        [DllImport("Gdi32")]
        internal static extern unsafe HBITMAP CreateDIBSection(HDC hdc, BITMAPINFO* pbmi, UINT iUsage,
            void** ppvBits, HANDLE hSection, DWORD dwOffset);

        [DllImport("User32", SetLastError = true)]
        internal static extern HICON CreateIconIndirect(ref ICONINFO piconinfo);

        [DllImport("User32", SetLastError = true)]
        internal static extern HWND CreateWindowExW(DWORD dwExStyle, LPCWSTR lpClassName, LPCWSTR lpWindowName,
            DWORD dwStyle, int X, int Y, int nWidth, int nHeight,
            HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam);

        [DllImport("User32")]
        internal static extern LRESULT DefWindowProcW(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);

        [DllImport("Gdi32")]
        internal static extern BOOL DeleteObject(HGDIOBJ hObject);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL DestroyIcon(HICON hIcon);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL DestroyWindow(HWND hWnd);

        [DllImport("User32")]
        internal static extern LRESULT DispatchMessageW(ref MSG lpMsg);

        [DllImport("Shell32")]
        internal static extern void DragAcceptFiles(HWND hWnd, BOOL fAccept);

        [DllImport("Shell32")]
        internal static extern void DragFinish(HDROP hDrop);

        [DllImport("Shell32")]
        internal static extern UINT DragQueryFileW(HDROP hDrop, UINT iFile, LPWSTR lpszFile, UINT cch);

        [DllImport("Shell32")]
        internal static extern UINT DragQueryFileW(HDROP hDrop, UINT iFile, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder lpszFile, UINT cch);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL EmptyClipboard();

        [DllImport("User32")]
        internal static extern BOOL FlashWindow(HWND hWnd, BOOL bInvert);

        [DllImport("User32")]
        internal static extern HWND GetActiveWindow();

        [DllImport("User32")]
        internal static extern SHORT GetAsyncKeyState(int nVirtKey);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL GetClientRect(HWND hWnd, out RECT lpRect);

        [DllImport("User32", SetLastError = true)]
        internal static extern HANDLE GetClipboardData(UINT uFormat);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL GetCursorPos(out POINT lpPoint);

        [DllImport("User32")]
        internal static extern HDC GetDC(HWND hWnd);

        [DllImport("Shcore")]
        internal static extern HRESULT GetDpiForMonitor(HMONITOR hmonitor, MONITOR_DPI_TYPE dpiType, out UINT dpiX, out UINT dpiY);

        [DllImport("User32", SetLastError = true)]
        internal static extern int GetKeyNameTextW(LONG lParam, LPWSTR lpString, int cchSize);

        [DllImport("User32")]
        internal static extern SHORT GetKeyState(int nVirtKey);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL GetLayeredWindowAttributes(HWND hwnd, out COLORREF pcrKey, out BYTE pbAlpha, out DWORD pdwFlags);

        [DllImport("User32")]
        internal static extern LONG GetMessageTime();

        [DllImport("User32")]
        internal static extern UINT GetRawInputData(HRAWINPUT hRawInput, UINT uiCommand, IntPtr pData, ref UINT pcbSize, UINT cbSizeHeader);

        [DllImport("User32", SetLastError = true)]
        internal static extern LONG_PTR GetWindowLongPtrW(HWND hWnd, int nIndex);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL GetWindowPlacement(HWND hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL GetWindowRect(HWND hWnd, out RECT lpRect);

        [DllImport("User32", SetLastError = true)]
        internal static extern int GetWindowTextLengthW(HWND hWnd);

        [DllImport("User32", SetLastError = true)]
        internal static extern int GetWindowTextW(HWND hWnd, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder lpString, int nMaxCount);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern HGLOBAL GlobalAlloc(UINT uFlags, SIZE_T dwBytes);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern HGLOBAL GlobalFree(HGLOBAL hMem);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern LPVOID GlobalLock(HGLOBAL hMem);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern BOOL GlobalUnlock(HGLOBAL hMem);

        [DllImport("User32", SetLastError = true)]
        internal static extern HCURSOR LoadCursorW(HINSTANCE hInstance, INTRESOURCE resource);

        [DllImport("User32", SetLastError = true)]
        internal static extern HANDLE LoadImageW(HINSTANCE hInstance, LPCWSTR name, UINT type, int cx, int cy, UINT fuLoad);

        [DllImport("User32")]
        internal static extern HMONITOR MonitorFromWindow(HWND hwnd, DWORD dwFlags);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL MoveWindow(HWND hWnd, int X, int Y, int nWidth, int nHeight, BOOL bRepaint);

        [DllImport("User32", SetLastError = true)]
        internal static extern unsafe DWORD MsgWaitForMultipleObjects(DWORD nCount, HANDLE* pHandles, BOOL bWaitAll,
            DWORD dwMilliseconds, DWORD dwWakeMask);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL OpenClipboard(HWND hWndNewOwner);

        [DllImport("User32")]
        internal static extern BOOL PeekMessageW(out MSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL PostMessage(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);

        [DllImport("User32")]
        internal static extern BOOL PtInRect(ref RECT lprc, POINT pt);

        [DllImport("User32", SetLastError = true)]
        internal static extern ATOM RegisterClassW(ref WNDCLASS lpWndClass);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL RegisterRawInputDevices(ref RAWINPUTDEVICE pRawInputDevices, UINT uiNumDevices, UINT cbSize);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL ReleaseCapture();

        [DllImport("User32")]
        internal static extern int ReleaseDC(HWND hWnd, HDC hDC);

        [DllImport("User32")]
        internal static extern BOOL ScreenToClient(HWND hWnd, ref POINT lpPoint);

        [DllImport("User32")]
        internal static extern HWND SetCapture(HWND hWnd);

        [DllImport("User32", SetLastError = true)]
        internal static extern HANDLE SetClipboardData(UINT uFormat, HANDLE hMem);

        [DllImport("User32")]
        internal static extern HCURSOR SetCursor(HCURSOR hCursor);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL SetCursorPos(int x, int y);

        [DllImport("User32", SetLastError = true)]
        internal static extern HWND SetFocus(HWND hWnd);

        [DllImport("User32")]
        internal static extern BOOL SetForegroundWindow(HWND hWnd);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL SetLayeredWindowAttributes(HWND hwnd, COLORREF crKey, BYTE bAlpha, DWORD dwFlags);

        [DllImport("Shcore")]
        internal static extern HRESULT SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

        [DllImport("User32", SetLastError = true)]
        internal static extern LONG_PTR SetWindowLongPtrW(HWND hWnd, int nIndex, LONG_PTR dwNewLong);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL SetWindowTextW(HWND hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [DllImport("User32")]
        internal static extern BOOL ShowWindow(HWND hWnd, int nCmdShow);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        [DllImport("User32")]
        internal static extern BOOL TranslateMessage(ref MSG lpMsg);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL UnregisterClassW(LPCWSTR lpClassName, HINSTANCE hInstance);

        [DllImport("User32", SetLastError = true)]
        internal static extern BOOL WaitMessage();

        [DllImport("User32")]
        internal static extern HWND WindowFromPoint(POINT Point);
    }
}
