using System;
using System.Runtime.InteropServices;

using BOOL = System.Int32;
using BYTE = System.Byte;
using DWORD = System.UInt32;
using FLOAT = System.Single;
using HDC = System.IntPtr;
using HGLRC = System.IntPtr;
using HWND = System.IntPtr;
using PROC = System.IntPtr;
using UINT = System.UInt32;
using WORD = System.UInt16;

namespace NStuff.OpenGL.Context.Windows
{
    internal static class NativeMethods
    {
        internal const BYTE PFD_MAIN_PLANE = 0x00000000;
        internal const DWORD PFD_DOUBLEBUFFER = 0x00000001;
        internal const DWORD PFD_STEREO = 0x00000002;
        internal const DWORD PFD_DRAW_TO_WINDOW = 0x00000004;
        internal const DWORD PFD_SUPPORT_OPENGL = 0x00000020;

        internal const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
        internal const int WGL_ACCELERATION_ARB = 0x2003;
        internal const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
        internal const int WGL_DOUBLE_BUFFER_ARB = 0x2011;
        internal const int WGL_STEREO_ARB = 0x2012;
        internal const int WGL_PIXEL_TYPE_ARB = 0x2013;
        internal const int WGL_COLOR_BITS_ARB = 0x2014;
        internal const int WGL_ALPHA_BITS_ARB = 0x201B;
        internal const int WGL_ACCUM_BITS_ARB = 0x201D;
        internal const int WGL_DEPTH_BITS_ARB = 0x2022;
        internal const int WGL_STENCIL_BITS_ARB = 0x2023;
        internal const int WGL_AUX_BUFFERS_ARB = 0x2024;
        internal const int WGL_SAMPLES_ARB = 0x2042;

        internal const int WGL_FULL_ACCELERATION_ARB = 0x2027;
        internal const int WGL_TYPE_RGBA_ARB = 0x202B;

        [StructLayout(LayoutKind.Sequential)]
        internal struct PIXELFORMATDESCRIPTOR
        {
            internal WORD nSize;
            internal WORD nVersion;
            internal DWORD dwFlags;
            internal BYTE iPixelType;
            internal BYTE cColorBits;
            internal BYTE cRedBits;
            internal BYTE cRedShift;
            internal BYTE cGreenBits;
            internal BYTE cGreenShift;
            internal BYTE cBlueBits;
            internal BYTE cBlueShift;
            internal BYTE cAlphaBits;
            internal BYTE cAlphaShift;
            internal BYTE cAccumBits;
            internal BYTE cAccumRedBits;
            internal BYTE cAccumGreenBits;
            internal BYTE cAccumBlueBits;
            internal BYTE cAccumAlphaBits;
            internal BYTE cDepthBits;
            internal BYTE cStencilBits;
            internal BYTE cAuxBuffers;
            internal BYTE iLayerType;
            internal BYTE bReserved;
            internal DWORD dwLayerMask;
            internal DWORD dwVisibleMask;
            internal DWORD dwDamageMask;
        }

        internal delegate IntPtr PFNWGLGETEXTENSIONSSTRINGEXTPROC();
        internal delegate IntPtr PFNWGLGETEXTENSIONSSTRINGARBPROC(HDC hdc);

        internal unsafe delegate HGLRC PFNWGLCREATECONTEXTATTRIBSARBPROC(HDC hDC, HGLRC hShareContext, int* attribList);
        internal unsafe delegate BOOL PFNWGLCHOOSEPIXELFORMATARBPROC(HDC hdc, int* piAttribIList, FLOAT* pfAttribFList, UINT nMaxFormats,
            int* piFormats, UINT* nNumFormats);
        internal delegate BOOL PFNWGLSWAPINTERVALEXTPROC(int interval);

        [DllImport("Gdi32", SetLastError = true)]
        internal static extern int ChoosePixelFormat(HDC hdc, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("Gdi32", SetLastError = true)]
        internal static extern int DescribePixelFormat(HDC hdc, int iPixelFormat, UINT nBytes, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("User32")]
        internal static extern HDC GetDC(HWND hWnd);

        [DllImport("Gdi32", SetLastError = true)]
        internal static extern BOOL SetPixelFormat(HDC hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("Gdi32", SetLastError = true)]
        internal static extern BOOL SwapBuffers(HDC hdc);

#pragma warning disable IDE1006 // Naming Styles
        [DllImport("OpenGL32", SetLastError = true)]
        internal static extern HGLRC wglCreateContext(HDC hdc);

        [DllImport("OpenGL32", SetLastError = true)]
        internal static extern BOOL wglDeleteContext(HGLRC hglrc);

        [DllImport("OpenGL32", SetLastError = true)]
        internal static extern PROC wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("OpenGL32", SetLastError = true)]
        internal static extern BOOL wglMakeCurrent(HDC hdc, HGLRC hglrc);
    }
}
