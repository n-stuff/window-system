using NStuff.Runtime.InteropServices;
using NStuff.WindowSystem;
using NStuff.WindowSystem.Windows;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using static NStuff.OpenGL.Context.Windows.NativeMethods;

namespace NStuff.OpenGL.Context.Windows
{
    internal class NativeRenderingContext : NativeRenderingContextBase
    {
        private DynamicLinkLibrary? openGLLibrary;
        private Action<bool>? swapInterval;
        private PFNWGLCREATECONTEXTATTRIBSARBPROC? createContextAttribs;
        private PFNWGLCHOOSEPIXELFORMATARBPROC? choosePixelFormat;
        private bool multisample;

        public override void FreeResources()
        {
            openGLLibrary?.Dispose();
            openGLLibrary = null;
        }

        public override void AttachRenderingData(RenderingContext context, WindowServer windowServer, Window window)
        {
            window.RenderingData = new RenderingData();
        }

        public override void SetupRenderingData(RenderingContext context, WindowServer windowServer, Window window)
        {
            var dc = GetDC(((WindowData?)window.NativeData ?? throw new InvalidOperationException()).Handle);
            if (dc == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.DeviceContextRetrievalFailed));
            }
            var data = GetData(window);
            data.DeviceContext = dc;
            var settings = context.Settings ?? throw new InvalidOperationException();
            var shareContext = settings.ShareContext;
            var shareContextHandle = (shareContext != null) ? GetData(shareContext).DeviceContext : IntPtr.Zero;

            data.Handle = CreateRenderingContext(dc, shareContextHandle, settings);
            if (swapInterval == null)
            {
                wglMakeCurrent(dc, data.Handle);

                var extensionsEXT = string.Empty;
                var extensionsARB = string.Empty;
                var proc = wglGetProcAddress("wglGetExtensionsStringEXT");
                if (proc != IntPtr.Zero)
                {
                    var getExtensions = Marshal.GetDelegateForFunctionPointer<PFNWGLGETEXTENSIONSSTRINGEXTPROC>(proc);
                    extensionsEXT = Marshal.PtrToStringAnsi(getExtensions());
                }
                proc = wglGetProcAddress("wglGetExtensionsStringARB");
                if (proc != IntPtr.Zero)
                {
                    var getExtensions = Marshal.GetDelegateForFunctionPointer<PFNWGLGETEXTENSIONSSTRINGARBPROC>(proc);
                    extensionsARB = Marshal.PtrToStringAnsi(getExtensions(dc));
                }

                if (extensionsEXT.IndexOf("WGL_EXT_swap_control") >= 0 || extensionsARB.IndexOf("WGL_EXT_swap_control") >= 0)
                {
                    var swapIntervalEXT = LoadEntryPoint<PFNWGLSWAPINTERVALEXTPROC>("wglSwapIntervalEXT");
                    swapInterval = (sync) => swapIntervalEXT(sync ? 1 : 0);
                }
                else
                {
                    swapInterval = (sync) => { };
                }

                if (extensionsEXT.IndexOf("WGL_ARB_create_context") >= 0 || extensionsARB.IndexOf("WGL_ARB_create_context") >= 0)
                {
                    createContextAttribs = LoadEntryPoint<PFNWGLCREATECONTEXTATTRIBSARBPROC>("wglCreateContextAttribsARB");
                }

                if (extensionsEXT.IndexOf("WGL_ARB_pixel_format") >= 0 || extensionsARB.IndexOf("WGL_ARB_pixel_format") >= 0)
                {
                    choosePixelFormat = LoadEntryPoint<PFNWGLCHOOSEPIXELFORMATARBPROC>("wglChoosePixelFormatARB");
                }

                multisample = extensionsEXT.IndexOf("WGL_ARB_multisample") >= 0 || extensionsARB.IndexOf("WGL_ARB_multisample") >= 0;

                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                if (CurrentWindow != null)
                {
                    var currentData = (RenderingData?)CurrentWindow.RenderingData;
                    if (currentData != null)
                    {
                        wglMakeCurrent(currentData.DeviceContext, currentData.Handle);
                    }
                }

                if (choosePixelFormat != null && multisample && settings.Samples > 0)
                {
                    wglDeleteContext(data.Handle);
                    windowServer.RecreateWindow(window);
                    SetupRenderingData(context, windowServer, window);
                }
            }
        }

        public override void DetachRenderingData(Window window)
        {
            wglDeleteContext(GetData(window).Handle);
            window.RenderingData = null;
        }

        public override void UpdateRenderingData(Window window)
        {
        }

        public override void MakeWindowCurrent(Window? window)
        {
            if (window == null)
            {
                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                var data = GetData(window);
                wglMakeCurrent(data.DeviceContext, data.Handle);
            }
        }

        public override void SwapBuffers(Window window) => NativeMethods.SwapBuffers(GetData(window).DeviceContext);

        public override void SyncWithVerticalBlank(Window window, bool sync) => swapInterval?.Invoke(sync);

        public override IntPtr GetCommandAddress(string commandName)
        {
            var result = wglGetProcAddress(commandName);
            if (result == IntPtr.Zero)
            {
                if (openGLLibrary == null)
                {
                    openGLLibrary = new DynamicLinkLibrary("OpenGL32");
                }
                result = openGLLibrary.GetSymbolAddress(commandName, false);
            }
            return result;
        }

        private static TDelegate LoadEntryPoint<TDelegate>(string name)
        {
            var address = wglGetProcAddress(name);
            if (address == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.OpenGLEntryPointNotPresent, name));
            }
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
        }

        private unsafe IntPtr CreateRenderingContext(IntPtr dc, IntPtr shareContextHandle, RenderingSettings settings)
        {
            var pixelformatdescriptor = new PIXELFORMATDESCRIPTOR { nSize = (ushort)sizeof(PIXELFORMATDESCRIPTOR) };
            int pixelFormat = 0;
            if (choosePixelFormat == null)
            {
                pixelformatdescriptor.nVersion = 1;
                pixelformatdescriptor.dwFlags = PFD_DOUBLEBUFFER | PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL;
                if (settings.Stereo)
                {
                    pixelformatdescriptor.dwFlags |= PFD_STEREO;
                }
                pixelformatdescriptor.cColorBits = (byte)settings.ColorBits;
                pixelformatdescriptor.cAlphaBits = (byte)settings.AlphaBits;
                pixelformatdescriptor.cAccumBits = (byte)settings.AccumBits;
                pixelformatdescriptor.cDepthBits = (byte)settings.DepthBits;
                pixelformatdescriptor.cStencilBits = (byte)settings.StencilBits;
                pixelformatdescriptor.cAuxBuffers = (byte)settings.AuxBuffers;
                pixelformatdescriptor.iLayerType = PFD_MAIN_PLANE;

                pixelFormat = ChoosePixelFormat(dc, ref pixelformatdescriptor);
                if (pixelFormat == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else
            {
                var attributeList = stackalloc int[32];
                int i = 0;
                attributeList[i++] = WGL_SUPPORT_OPENGL_ARB;
                attributeList[i++] = 1;
                attributeList[i++] = WGL_DRAW_TO_WINDOW_ARB;
                attributeList[i++] = 1;
                attributeList[i++] = WGL_DOUBLE_BUFFER_ARB;
                attributeList[i++] = 1;
                attributeList[i++] = WGL_ACCELERATION_ARB;
                attributeList[i++] = WGL_FULL_ACCELERATION_ARB;
                attributeList[i++] = WGL_PIXEL_TYPE_ARB;
                attributeList[i++] = WGL_TYPE_RGBA_ARB;
                attributeList[i++] = WGL_COLOR_BITS_ARB;
                attributeList[i++] = settings.ColorBits;
                attributeList[i++] = WGL_ALPHA_BITS_ARB;
                attributeList[i++] = settings.AlphaBits;
                attributeList[i++] = WGL_DEPTH_BITS_ARB;
                attributeList[i++] = settings.DepthBits;
                attributeList[i++] = WGL_STENCIL_BITS_ARB;
                attributeList[i++] = settings.StencilBits;
                attributeList[i++] = WGL_ACCUM_BITS_ARB;
                attributeList[i++] = settings.AccumBits;
                attributeList[i++] = WGL_AUX_BUFFERS_ARB;
                attributeList[i++] = settings.AuxBuffers;
                if (settings.Stereo)
                {
                    attributeList[i++] = WGL_STEREO_ARB;
                    attributeList[i++] = 1;
                }
                if (multisample && settings.Samples > 0)
                {
                    attributeList[i++] = WGL_SAMPLES_ARB;
                    attributeList[i++] = settings.Samples;
                }

                uint nPixelFormats;
                if (choosePixelFormat(dc, attributeList, null, 1, &pixelFormat, &nPixelFormats) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (DescribePixelFormat(dc, pixelFormat, (uint)sizeof(PIXELFORMATDESCRIPTOR), ref pixelformatdescriptor) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            if (SetPixelFormat(dc, pixelFormat, ref pixelformatdescriptor) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            IntPtr result;
            if (createContextAttribs == null)
            {
                if (shareContextHandle != IntPtr.Zero)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.RenderingContextSharingNotSupported));
                }
                result = wglCreateContext(dc);
            }
            else
            {
                var attributeList = stackalloc int[16];
                result = createContextAttribs(dc, shareContextHandle, attributeList);
            }
            if (result == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return result;
        }

        private static RenderingData GetData(Window window) =>
            (RenderingData?)window.RenderingData ?? throw new InvalidOperationException();
    }
}
