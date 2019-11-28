using System;
using NStuff.Runtime.InteropServices;
using NStuff.Runtime.InteropServices.ObjectiveC;
using NStuff.WindowSystem;
using NStuff.WindowSystem.macOS;

using static NStuff.OpenGL.Context.macOS.NativeMethods;
using static NStuff.OpenGL.Context.macOS.Selectors;
using static NStuff.Runtime.InteropServices.ObjectiveC.Selectors;
using static NStuff.WindowSystem.macOS.Selectors;

namespace NStuff.OpenGL.Context.macOS
{
    internal class NativeRenderingContext : NativeRenderingContextBase
    {
        private DynamicLinkLibrary? openGLLibrary;
        private Class NSOpenGLPixelFormat;
        private Class NSOpenGLContext;

        public override void FreeResources()
        {
            openGLLibrary?.Dispose();
            openGLLibrary = null;
        }

        public override void AttachRenderingData(RenderingContext context, WindowServer server, Window window) =>
            window.RenderingData = new RenderingData();

        public override unsafe void SetupRenderingData(RenderingContext context, WindowServer server, Window window)
        {
            if (NSOpenGLContext.Handle == IntPtr.Zero)
            {
                NSOpenGLPixelFormat = Class.Lookup("NSOpenGLPixelFormat");
                NSOpenGLContext = Class.Lookup("NSOpenGLContext");
            }

            var windowData = (WindowData?)window.NativeData ?? throw new InvalidOperationException();
            var nsView = windowData.Id.Get(contentView);
            nsView.Send(setWantsBestResolutionOpenGLSurface_, true);

            var pixelFormatAttributes = stackalloc uint[32];
            var index = 0;
            pixelFormatAttributes[index++] = NSOpenGLPFADoubleBuffer;
            pixelFormatAttributes[index++] = NSOpenGLPFAClosestPolicy;
            pixelFormatAttributes[index++] = NSOpenGLPFAOpenGLProfile;
            pixelFormatAttributes[index++] = NSOpenGLProfileVersion4_1Core;
            var settings = context.Settings ?? throw new InvalidOperationException();
            if (settings.ColorBits > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAColorSize;
                pixelFormatAttributes[index++] = (uint)settings.ColorBits;
            }
            if (settings.AlphaBits > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAAlphaSize;
                pixelFormatAttributes[index++] = (uint)settings.AlphaBits;
            }
            if (settings.DepthBits > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFADepthSize;
                pixelFormatAttributes[index++] = (uint)settings.DepthBits;
            }
            if (settings.StencilBits > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAStencilSize;
                pixelFormatAttributes[index++] = (uint)settings.StencilBits;
            }
            if (settings.AccumBits > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAAccumSize;
                pixelFormatAttributes[index++] = (uint)settings.AccumBits;
            }
            if (settings.AuxBuffers > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAAuxBuffers;
                pixelFormatAttributes[index++] = (uint)settings.AuxBuffers;
            }
            if (settings.Stereo)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFAStereo;
            }
            if (settings.Samples > 0)
            {
                pixelFormatAttributes[index++] = NSOpenGLPFASampleBuffers;
                pixelFormatAttributes[index++] = 1;
                pixelFormatAttributes[index++] = NSOpenGLPFASamples;
                pixelFormatAttributes[index++] = (uint)settings.Samples;
            }

            var pixelFormat = NSOpenGLPixelFormat.Get(alloc).Get(initWithAttributes_, new IntPtr(pixelFormatAttributes));
            var shareContext = (settings.ShareContext != null) ? GetData(settings.ShareContext).Context : Id.Nil;
            var nsContext = NSOpenGLContext.Get(alloc).Get(initWithFormat_shareContext_, pixelFormat, shareContext);
            pixelFormat.Send(release);

            nsContext.Send(setView_, nsView);

            GetData(window).Context = nsContext;
        }

        public override void DetachRenderingData(Window window) => window.RenderingData = null;

        public override void UpdateRenderingData(Window window) => GetData(window).Context.Send(update);

        public override void MakeWindowCurrent(Window? window)
        {
            if (window == null)
            {
                NSOpenGLContext.Send(clearCurrentContext);
            }
            else
            {
                GetData(window).Context.Send(makeCurrentContext);
            }
        }

        public override void SwapBuffers(Window window) => GetData(window).Context.Send(flushBuffer);

        public override unsafe void SyncWithVerticalBlank(Window window, bool sync)
        {
            var openGLContextValues = stackalloc int[1];
            openGLContextValues[0] = sync ? 1 : 0;
            GetData(window).Context.Send(setValues_forParameter_, new IntPtr(openGLContextValues), NSOpenGLCPSwapInterval);
        }

        public override IntPtr GetCommandAddress(string commandName)
        {
            if (openGLLibrary == null)
            {
                openGLLibrary = new DynamicLinkLibrary("OpenGL.framework/OpenGL");
            }
            openGLLibrary.TryGetSymbolAddress(commandName, out var result);
            return result;
        }

        private static RenderingData GetData(Window window) =>
            (RenderingData?)window.RenderingData ?? throw new InvalidOperationException();
    }
}
