using System;
using System.Runtime.InteropServices;
using NStuff.WindowSystem;
using NStuff.WindowSystem.Linux;

using static NStuff.OpenGL.Context.Linux.NativeMethods;

namespace NStuff.OpenGL.Context.Linux
{
    internal class NativeRenderingContext : NativeRenderingContextBase
    {
        private IntPtr display;
        private Action<Window, bool>? swapInterval;
        private bool multisample;

        public override void FreeResources()
        {
        }

        public override unsafe void AttachRenderingData(RenderingContext context, WindowServer server, Window window)
        {
            var windowData = GetWindowData(window);
            if (swapInterval == null)
            {
                display = windowData.Display;
                var extensions = Marshal.PtrToStringAnsi(glXQueryExtensionsString(display, XDefaultScreen(display))) ?? string.Empty;
                if (HasExtension(extensions, "GLX_EXT_swap_control"))
                {
                    var proc = Marshal.GetDelegateForFunctionPointer<PFNGLXSWAPINTERVALEXTPROC>(GetCommandAddress("glXSwapIntervalEXT"));
                    swapInterval = (w, sync) => {
                        var d = GetWindowData(w);
                        proc(d.Display, d.Id, sync ? 1 : 0);
                    };
                }
                else if (HasExtension(extensions, "GLX_MESA_swap_control"))
                {
                    var proc = Marshal.GetDelegateForFunctionPointer<PFNGLXSWAPINTERVALMESAPROC>(GetCommandAddress("glXSwapIntervalMESA"));
                    swapInterval = (d, sync) => proc(sync ? 1u : 0u);
                }
                else
                {
                    swapInterval = (d, sync) => { };
                }
                multisample = HasExtension(extensions, "GLX_ARB_multisample");
            }

            var settings = context.Settings ?? throw new InvalidOperationException();
            var attributeList = stackalloc int[32];
            attributeList[0] = GLX_RGBA;
            attributeList[1] = GLX_DOUBLEBUFFER;
            var index = 2;
            if (settings.Stereo)
            {
                attributeList[index++] = GLX_STEREO;
            }
            if (settings.AuxBuffers > 0)
            {
                attributeList[index++] = GLX_AUX_BUFFERS;
                attributeList[index++] = settings.AuxBuffers;
            }
            if (settings.ColorBits >= 3)
            {
                var bits = settings.ColorBits / 3;
                attributeList[index++] = GLX_RED_SIZE;
                attributeList[index++] = bits;
                attributeList[index++] = GLX_GREEN_SIZE;
                attributeList[index++] = bits;
                attributeList[index++] = GLX_BLUE_SIZE;
                attributeList[index++] = bits;
            }
            if (settings.AlphaBits > 0)
            {
                attributeList[index++] = GLX_ALPHA_SIZE;
                attributeList[index++] = settings.AlphaBits;
            }
            if (settings.DepthBits > 0)
            {
                attributeList[index++] = GLX_DEPTH_SIZE;
                attributeList[index++] = settings.DepthBits;
            }
            if (settings.StencilBits > 0)
            {
                attributeList[index++] = GLX_STENCIL_SIZE;
                attributeList[index++] = settings.StencilBits;
            }
            if (settings.AccumBits >= 4)
            {
                var bits = (int)(settings.AccumBits / 4);
                attributeList[index++] = GLX_ACCUM_RED_SIZE;
                attributeList[index++] = bits;
                attributeList[index++] = GLX_ACCUM_GREEN_SIZE;
                attributeList[index++] = bits;
                attributeList[index++] = GLX_ACCUM_BLUE_SIZE;
                attributeList[index++] = bits;
                attributeList[index++] = GLX_ACCUM_ALPHA_SIZE;
                attributeList[index++] = bits;
            }
            if (multisample && settings.Samples > 0)
            {
                attributeList[index++] = GLX_SAMPLES_ARB;
                attributeList[index++] = settings.Samples;
            }

            var visualInfo = glXChooseVisual(display, XDefaultScreen(display), attributeList);
            if (visualInfo == null)
            {
                attributeList[index -= 2] = 0;
                visualInfo = glXChooseVisual(display, XDefaultScreen(display), attributeList);
            }
            if (visualInfo == null)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.Visual_Selection_Failed));
            }
            var shareContext = settings.ShareContext;
            var shareContextHandle = (shareContext != null) ? GetData(shareContext).Context : IntPtr.Zero;

            var data = new RenderingData { Context = glXCreateContext(display, visualInfo, shareContextHandle, 1) };
            window.RenderingData = data;

            windowData.Visual = visualInfo->visual;
        }

        public override void SetupRenderingData(RenderingContext context, WindowServer server, Window window)
        {
        }

        public override void DetachRenderingData(Window window)
        {
            glXDestroyContext(GetWindowData(window).Display, GetData(window).Context);
            window.RenderingData = null;
        }

        public override void UpdateRenderingData(Window window)
        {
        }

        public override void MakeWindowCurrent(Window? window)
        {
            if (window == null)
            {
                glXMakeCurrent(display, None, IntPtr.Zero);
            }
            else
            {
                var windowData = GetWindowData(window);
                glXMakeCurrent(windowData.Display, windowData.Id, GetData(window).Context);
            }
        }

        public override void SwapBuffers(Window window)
        {
            var windowData = GetWindowData(window);
            glXSwapBuffers(windowData.Display, windowData.Id);
        }

        public override void SyncWithVerticalBlank(Window window, bool sync) => swapInterval?.Invoke(window, sync);

        public override IntPtr GetCommandAddress(string commandName) => glXGetProcAddress(commandName);

        private static WindowData GetWindowData(Window window) =>
            (WindowData?)window.NativeData ?? throw new InvalidOperationException();

        private static RenderingData GetData(Window window) =>
            (RenderingData?)window.RenderingData ?? throw new InvalidOperationException();

        private static bool HasExtension(string extensions, string extension) =>
            extensions.IndexOf(extension, StringComparison.Ordinal) >= 0;
    }
}
