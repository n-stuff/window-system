#pragma warning disable CA1806 // Do not ignore method results

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using NStuff.Runtime.InteropServices;
using NStuff.Runtime.InteropServices.ObjectiveC;

using static NStuff.Runtime.InteropServices.ObjectiveC.Selectors;
using static NStuff.WindowSystem.macOS.NativeMethods;
using static NStuff.WindowSystem.macOS.Selectors;

namespace NStuff.WindowSystem.macOS
{
    internal class NativeWindowServer : NativeWindowServerBase
    {
        private static readonly List<object> implementations = new();
        private static readonly Keycode[] keycodeMappings;
        private static readonly int[] scancodes = new int[(int)Keycode.Invalid];
        private static NativeWindowServer? shared;

        private DynamicLinkLibrary? appKitFramework;
        private DynamicLinkLibrary? carbonFramework;
        private readonly Class NSApplication;
        private readonly Class NSArray;
        private readonly Class NSAttributedString;
        private readonly Class NSAutoreleasePool;
        private readonly Class NSBitmapImageRep;
        private readonly Class NSCursor;
        private readonly Class NSDate;
        private readonly Class NSEvent;
        private readonly Class NSImage;
        private readonly Class NSMutableAttributedString;
        private readonly Class NSPasteboard;
        private readonly Class NSTrackingArea;
        private readonly Class NSView;
        private readonly Class NStuffWindow;
        private readonly Class NStuffWindowDelegate;
        private readonly Class NStuffView;
        private Id autoreleasePool;
        private readonly Id NSApp;
        private readonly Id distantFutureDate;
        private readonly Id distantPastDate;
        private readonly Id windowDelegate;
        private readonly Id NSDefaultRunLoopMode;
        private readonly Id NSStringPboardType;
        private readonly Id NSFilenamesPboardType;
        private readonly Id NSCalibratedRGBColorSpace;
        private readonly IntPtr KTISPropertyUnicodeKeyLayoutData;
        private readonly IntPtr inputSource;
        private NSPoint cascadePoint;
        private double eventTimestamp;
        private Window? freeLookMouseWindow;
        private (double x, double y) cursorPositionBackup;
        private bool cursorHidden;
        private readonly Dictionary<IntPtr, Window> windows = new();

        public NativeWindowServer()
        {
            if (shared != null)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.CocoaAlreadyInitialized));
            }
            shared = this;

            appKitFramework = new DynamicLinkLibrary("/System/Library/Frameworks/AppKit.framework/AppKit");
            carbonFramework = new DynamicLinkLibrary("/System/Library/Frameworks/Carbon.framework/Carbon");
            NSAutoreleasePool = Class.Lookup("NSAutoreleasePool");
            autoreleasePool = NSAutoreleasePool.Get(@new);
            NSApplication = Class.Lookup("NSApplication");
            NSArray = Class.Lookup("NSArray");
            NSAttributedString = Class.Lookup("NSAttributedString");
            NSBitmapImageRep = Class.Lookup("NSBitmapImageRep");
            NSCursor = Class.Lookup("NSCursor");
            NSDate = Class.Lookup("NSDate");
            NSEvent = Class.Lookup("NSEvent");
            NSImage = Class.Lookup("NSImage");
            NSMutableAttributedString = Class.Lookup("NSMutableAttributedString");
            NSPasteboard = Class.Lookup("NSPasteboard");
            NSTrackingArea = Class.Lookup("NSTrackingArea");
            NSView = Class.Lookup("NSView");
            NSDefaultRunLoopMode = new Id(Marshal.ReadIntPtr(appKitFramework.GetSymbolAddress("NSDefaultRunLoopMode")));
            NSStringPboardType = new Id(Marshal.ReadIntPtr(appKitFramework.GetSymbolAddress("NSStringPboardType")));
            NSFilenamesPboardType = new Id(Marshal.ReadIntPtr(appKitFramework.GetSymbolAddress("NSFilenamesPboardType")));
            NSCalibratedRGBColorSpace = new Id(Marshal.ReadIntPtr(appKitFramework.GetSymbolAddress("NSCalibratedRGBColorSpace")));
            KTISPropertyUnicodeKeyLayoutData = Marshal.ReadIntPtr(carbonFramework.GetSymbolAddress("kTISPropertyUnicodeKeyLayoutData"));
            distantFutureDate = NSDate.Get(distantFuture);
            distantPastDate = NSDate.Get(distantPast);
            inputSource = TISCopyCurrentKeyboardInputSource();

            Class NStuffApplicationDelegate;
            NSApp = new Id(Marshal.ReadIntPtr(appKitFramework.GetSymbolAddress("NSApp")));
            if (NSApp.IsNil)
            {
                NStuffApplicationDelegate = CreateApplicationDelegateClass();
                NStuffWindow = CreateWindowClass();
                NStuffWindowDelegate = CreateWindowDelegateClass();
                NStuffView = CreateViewClass();
                NSApp = CreateApplicationClass().Get(sharedApplication);
                Class.Lookup("NSThread").Send(detachNewThreadSelector_toTarget_withObject_, startNewThread_, NSApp, default);
                NSApp.Send(setActivationPolicy_, NSApplicationActivationPolicyRegular);

                var appName = new NSString(Class.Lookup("NSProcessInfo").Get(processInfo).Get(processName).Handle);
                var NSMenu = Class.Lookup("NSMenu");
                var NSMenuItem = Class.Lookup("NSMenuItem");
                var mainMenu = NSMenu.Get(@new);
                NSApp.Send(setMainMenu_, mainMenu);
                mainMenu.Send(release);

                var appMenuItem = AddMenuItem(mainMenu, null, default, null, 0);
                var appMenu = NSMenu.Get(@new);
                appMenuItem.Send(setSubmenu_, appMenu);
                appMenu.Send(release);

                var servicesMenu = NSMenu.Get(@new);
                NSApp.Send(setServicesMenu_, servicesMenu);
                servicesMenu.Send(release);
                AddMenuItem(appMenu, "Services", default, null, 0).Send(setSubmenu_, servicesMenu);

                appMenu.Send(addItem_, NSMenuItem.Get(separatorItem));
                AddMenuItem(appMenu, string.Format("Hide {0}", appName), hide_, "h", 0);
                AddMenuItem(appMenu, "Hide Others", hideOtherApplications_, "h", NSAlternateKeyMask | NSCommandKeyMask);
                AddMenuItem(appMenu, "Show All", unhideAllApplications_, null, 0);

                appMenu.Send(addItem_, NSMenuItem.Get(separatorItem));
                AddMenuItem(appMenu, string.Format("Quit {0}", appName), terminate_, "q", 0);

                var windowMenuItem = AddMenuItem(mainMenu, null, default, null, 0);
                var windowMenu = NSMenu.Get(alloc).Get(initWithTitle_, new NSString("Window"));
                NSApp.Send(setWindowsMenu_, windowMenu);
                windowMenuItem.Send(setSubmenu_, windowMenu);
                windowMenu.Send(release);
                AddMenuItem(windowMenu, "Minimize", performMiniaturize_, "m", 0);
                AddMenuItem(windowMenu, "Zoom", performZoom_, null, 0);

                windowMenu.Send(addItem_, NSMenuItem.Get(separatorItem));
                AddMenuItem(windowMenu, "Bring All to Front", arrangeInFront_, null, 0);

                IntPtr values;
                IntPtr keys;
                unsafe
                {
                    var v = stackalloc IntPtr[1];
                    var k = stackalloc IntPtr[1];
                    values = new IntPtr(v);
                    keys = new IntPtr(k);
                    k[0] = new NSString("ApplePressAndHoldEnabled").Handle;
                    v[0] = Class.Lookup("NSNumber").Get(numberWithBool_, false).Handle;
                }
                var defaults = Class.Lookup("NSDictionary").Get(dictionaryWithObjects_forKeys_count_, values, keys, 1);
                Class.Lookup("NSUserDefaults").Get(standardUserDefaults).Send(registerDefaults_, defaults);

                windowDelegate = NStuffWindowDelegate.Get(@new);
                NSApp.Send(setDelegate_, NStuffApplicationDelegate.Get(@new));

                NSApp.Send(run);
            }
            else
            {
                NStuffApplicationDelegate = Class.Lookup("NStuffApplicationDelegate");
                NStuffWindow = Class.Lookup("NStuffWindow");
                NStuffWindowDelegate = Class.Lookup("NStuffWindowDelegate");
                NStuffView = Class.Lookup("NStuffView");
                windowDelegate = NStuffWindowDelegate.Get(@new);
                NSApp.Send(setDelegate_, NStuffApplicationDelegate.Get(@new));
            }
        }

        protected internal override void Shutdown()
        {
            CFRelease(inputSource);
            windowDelegate.Send(release);
            appKitFramework?.Dispose();
            appKitFramework = null;
            carbonFramework?.Dispose();
            carbonFramework = null;
            shared = null;
        }
        protected internal override bool IsRunning()
        {
            return shared != null;
        }

        protected internal override void CreateWindowData(Window window)
        {
            window.NativeData = new WindowData();
        }

        protected internal override void CreateWindow(Window window)
        {
            var styleMask = NSTitledWindowMask | NSClosableWindowMask | NSMiniaturizableWindowMask;
            var nsWindow = NStuffWindow.Get(alloc).Get(initWithContentRect_styleMask_backing_defer_,
                new NSRect(0, 0, DefaultSurfaceWidth, DefaultSurfaceHeight),
                styleMask, NSBackingStoreBuffered, false);
            var data = GetData(window);
            data.Id = nsWindow;
            data.MarkedText = NSMutableAttributedString.Get(alloc).Get(init);

            var view = NStuffView.Get(@new);
            nsWindow.Send(setContentView_, view);
            nsWindow.Send(makeFirstResponder_, view);
            view.Send(release);
            nsWindow.Send(setDelegate_, windowDelegate);
            nsWindow.Send(setTabbingMode_, NSWindowTabbingModeDisallowed);

            nsWindow.Send(center);
            cascadePoint = nsWindow.GetPoint(cascadeTopLeftFromPoint_, cascadePoint);

            windows.Add(nsWindow.Handle, window);
        }

        protected internal override void DestroyWindow(Window window)
        {
            var data = GetData(window);
            var nsWindow = data.Id;
            nsWindow.Send(orderOut_, default(Id));
            nsWindow.Send(close);
            nsWindow.Send(setContentView_, default(Id));
            if (!data.TrackingArea.IsNil)
            {
                data.TrackingArea.Send(release);
                data.TrackingArea = default;
            }
            if (!data.MarkedText.IsNil)
            {
                data.MarkedText.Send(release);
                data.MarkedText = default;
            }
            windows.Remove(nsWindow.Handle);
            window.NativeData = null;
        }

        protected internal override void RecreateNativeWindow(Window window)
        {
        }

        protected internal override ICollection<Window> GetWindows() => windows.Values;

        protected internal override bool IsWindowFocused(Window window) => GetId(window).GetBool(isKeyWindow);

        protected internal override bool IsWindowVisible(Window window) => GetId(window).GetBool(isVisible);

        protected internal override void SetWindowVisible(Window window, bool visible)
        {
            var nsWindow = GetId(window);
            if (visible)
            {
                nsWindow.Send(makeKeyAndOrderFront_, default(Id));
            }
            else
            {
                nsWindow.Send(orderOut_, default(Id));
            }
            VisibleChangedEventOccurred(window);
        }

        protected internal override WindowBorderStyle GetWindowBorderStyle(Window window)
        {
            var styleMask = GetId(window).GetUInteger(Selectors.styleMask);
            if ((styleMask | NSBorderlessWindowMask) != 0)
            {
                return WindowBorderStyle.None;
            }
            if ((styleMask | NSResizableWindowMask) != 0)
            {
                return WindowBorderStyle.Sizable;
            }
            return WindowBorderStyle.Fixed;
        }

        protected internal override void SetWindowBorderStyle(Window window, WindowBorderStyle borderStyle)
        {
            ulong styleMask = 0;
            switch (borderStyle)
            {
                case WindowBorderStyle.None:
                    styleMask |= NSBorderlessWindowMask;
                    break;

                case WindowBorderStyle.Sizable:
                    styleMask |= NSResizableWindowMask;
                    goto case WindowBorderStyle.Fixed;

                case WindowBorderStyle.Fixed:
                    styleMask |= NSTitledWindowMask | NSClosableWindowMask | NSMiniaturizableWindowMask;
                    break;
            }
            GetId(window).Send(setStyleMask_, styleMask);
        }

        protected internal override (double x, double y) GetWindowLocation(Window window)
        {
            var nsWindow = GetId(window);
            var rect = nsWindow.GetRect(contentRectForFrameRect_, nsWindow.GetRect(frame));
            return (rect.X, ConvertY(rect.Y + rect.Height - 1));
        }

        protected internal override void SetWindowLocation(Window window, (double x, double y) location)
        {
            var nsWindow = GetId(window);
            var rect = nsWindow.Get(contentView).GetRect(frame);
            rect = new NSRect(location.x, ConvertY(location.y + rect.Height - 1), 0, 0);
            rect = nsWindow.GetRect(frameRectForContentRect_, rect);
            nsWindow.Send(setFrameOrigin_, rect.Location);
        }

        protected internal override (double width, double height) GetWindowSize(Window window) => GetId(window).Get(contentView).GetRect(frame).Size;

        protected internal override void SetWindowSize(Window window, (double width, double height) size) =>
            GetId(window).Send(setContentSize_, new NSSize(size.width, size.height));

        protected internal override (double width, double height) GetWindowMaximumSize(Window window) => GetId(window).GetSize(contentMaxSize);

        protected internal override void SetWindowMaximumSize(Window window, (double width, double height) size) =>
            GetId(window).Send(setContentMaxSize_, new NSSize(size.width, size.height));

        protected internal override (double width, double height) GetWindowMinimumSize(Window window) => GetId(window).GetSize(contentMinSize);

        protected internal override void SetWindowMinimumSize(Window window, (double width, double height) size) =>
            GetId(window).Send(setContentMinSize_, new NSSize(size.width, size.height));

        protected internal override (double top, double left, double bottom, double right) GetWindowBorderSize(Window window)
        {
            var nsWindow = GetId(window);
            var view = nsWindow.Get(contentView);
            var frame = view.GetRect(Selectors.frame);
            var frameRect = nsWindow.GetRect(frameRectForContentRect_, frame);
            return (
                frameRect.Y + frameRect.Height - frame.Y - frame.Height,
                frame.X - frameRect.X,
                frame.Y - frameRect.Y,
                frameRect.X + frameRect.Width - frame.X - frame.Width
            );
        }

        protected internal override void SetFreeLookMouseWindow(Window window, bool enable)
        {
            if (enable)
            {
                freeLookMouseWindow = window;
                cursorPositionBackup = GetCursorPosition(window);
                CenterCursor(window);
                CGAssociateMouseAndMouseCursorPosition(0);
            }
            else if (freeLookMouseWindow == window)
            {
                freeLookMouseWindow = null;
                CGAssociateMouseAndMouseCursorPosition(1);
                SetCursorPosition(window, cursorPositionBackup);
            }
            if (IsCursorInClientAreaOutsideOfEventStream(window))
            {
                UpdateWindowCursor(window);
            }
        }

        protected internal override (double x, double y) GetWindowViewportSize(Window window)
        {
            var view = GetId(window).Get(contentView);
            return view.GetRect(convertRectToBacking_, view.GetRect(frame)).Size;
        }

        protected internal override (double x, double y) GetCursorPosition(Window window)
        {
            var nsWindow = GetId(window);
            var view = nsWindow.Get(contentView);
            var frameRect = view.GetRect(frame);
            var position = nsWindow.GetPoint(mouseLocationOutsideOfEventStream);
            return (position.X, frameRect.Height - position.Y - 1);
        }

        protected internal override void SetCursorPosition(Window window, (double x, double y) position)
        {
            UpdateWindowCursor(window);

            var data = GetData(window);
            var nsWindow = data.Id;
            var view = nsWindow.Get(contentView);
            var frameRect = view.GetRect(frame);
            var location = nsWindow.GetPoint(mouseLocationOutsideOfEventStream);

            var (dx, dy) = data.CursorWrapDelta;
            data.CursorWrapDelta = (dx + position.x - location.X, dy + position.y - frameRect.Size.Height + location.Y);

            var globalRect = nsWindow.GetRect(convertRectToScreen_, new NSRect(position.y, frameRect.Size.Height - position.y - 1, 0, 0));
            CGWarpMouseCursorPosition(new NSPoint(globalRect.Location.X, ConvertY(globalRect.Location.Y - 1)));
        }

        protected internal override void ActivateWindow(Window window)
        {
            var nsWindow = GetId(window);
            if (!nsWindow.GetBool(isMiniaturized) && nsWindow.GetBool(isVisible))
            {
                nsWindow.Send(makeKeyAndOrderFront_, default(Id));
            }
        }

        protected internal override bool IsWindowTopMost(Window window) => GetId(window).GetInteger(level) == NSFloatingWindowLevel;

        protected internal override void SetWindowTopMost(Window window, bool topMost)
        {
            GetId(window).Send(setLevel_, (long)((topMost) ? NSFloatingWindowLevel : NSNormalWindowLevel));
        }

        protected internal override double GetWindowOpacity(Window window) => GetId(window).GetDouble(alphaValue);

        protected internal override void SetWindowOpacity(Window window, double opacity) => GetId(window).Send(setAlphaValue_, opacity);

        protected internal override WindowSizeState GetWindowSizeState(Window window)
        {
            var nsWindow = GetId(window);
            if (nsWindow.GetBool(isMiniaturized))
            {
                return WindowSizeState.Minimized;
            }
            if (GetWindowBorderStyle(window) == WindowBorderStyle.Sizable && nsWindow.GetBool(isZoomed))
            {
                return WindowSizeState.Maximized;
            }
            return WindowSizeState.Normal;
        }

        protected internal override void SetWindowSizeState(Window window, WindowSizeState sizeState)
        {
            var nsWindow = GetId(window);
            switch (sizeState)
            {
                case WindowSizeState.Normal:
                    if (nsWindow.GetBool(isMiniaturized))
                    {
                        nsWindow.Send(deminiaturize_, default(Id));
                    }
                    else if (GetWindowBorderStyle(window) == WindowBorderStyle.Sizable && nsWindow.GetBool(isZoomed))
                    {
                        nsWindow.Send(zoom_, default(Id));
                        SizeStateChangedEventOccurred(window);
                    }
                    break;

                case WindowSizeState.Minimized:
                    nsWindow.Send(miniaturize_, default(Id));
                    break;

                case WindowSizeState.Maximized:
                    nsWindow.Send(zoom_, default(Id));
                    SizeStateChangedEventOccurred(window);
                    break;
            }
        }

        protected internal override string GetWindowTitle(Window window) => new NSString(GetId(window).Get(title).Handle).ToString();

        protected internal override void SetWindowTitle(Window window, string title) => GetId(window).Send(setTitle_, new NSString(title));

        protected internal override (double x, double y) ConvertFromScreen(Window window, (double x, double y) point)
        {
            var nsWindow = GetId(window);
            var frameRect = nsWindow.Get(contentView).GetRect(frame);
            var rect = new NSRect(point.x, ConvertY(point.y - 1), 0, 0);
            rect = nsWindow.GetRect(convertRectFromScreen_, rect);
            return (rect.Location.X, frameRect.Height - rect.Location.Y);
        }

        protected internal override (double x, double y) ConvertToScreen(Window window, (double x, double y) point)
        {
            var nsWindow = GetId(window);
            var rect = nsWindow.Get(contentView).GetRect(frame);
            rect = new NSRect(point.x, rect.Height - point.y, 0, 0);
            rect = nsWindow.GetRect(convertRectToScreen_, rect);
            return (rect.Location.X, ConvertY(rect.Location.Y - 1));
        }

        protected internal override void RequestWindowAttention(Window window) => NSApp.Send(requestUserAttention_, NSInformationalRequest);

        protected internal override void CreateCursor(Cursor cursor, CursorShape shape)
        {
            var nsCursor = shape switch
            {
                CursorShape.Arrow => NSCursor.Get(arrowCursor),
                CursorShape.IBeam => NSCursor.Get(IBeamCursor),
                CursorShape.Crosshair => NSCursor.Get(crosshairCursor),
                CursorShape.Hand => NSCursor.Get(pointingHandCursor),
                CursorShape.HorizontalResize => NSCursor.Get(resizeLeftRightCursor),
                CursorShape.VerticalResize => NSCursor.Get(resizeUpDownCursor),
                _ => throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.PredefinedCursorCreationFailed, shape)),
            };
            nsCursor.Get(retain);
            cursor.NativeData = new CursorData(nsCursor);
        }

        protected internal override unsafe void CreateCursor(Cursor cursor, byte[] imageData, (int width, int height) size, (double x, double y) hotSpot)
        {
            var representation = NSBitmapImageRep.Get(alloc).Get(
                initWithBitmapDataPlanes_pixelsWide_pixelsHigh_bitsPerSample_samplesPerPixel_hasAlpha_isPlanar_colorSpaceName_bitmapFormat_bytesPerRow_bitsPerPixel_,
                IntPtr.Zero, size.width, size.height, 8, 4, true, false, NSCalibratedRGBColorSpace, NSAlphaNonpremultipliedBitmapFormat, size.width * 4, 32);
            if (representation.IsNil)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }

            fixed (byte* p = imageData)
            {
                var length = size.width * size.height * 4;
                Buffer.MemoryCopy(p, (void*)representation.GetIntPtr(bitmapData), length, length);
            }

            var nsImage = NSImage.Get(alloc).Get(initWithSize_, new NSSize(size.width, size.height));
            if (nsImage.IsNil)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }
            nsImage.Send(addRepresentation_, representation);

            var nsCursor = NSCursor.Get(alloc).Get(initWithImage_hotSpot_, nsImage, new NSPoint(hotSpot.x, hotSpot.y));
            if (nsCursor.IsNil)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.CursorCreationFailed));
            }

            nsImage.Send(release);
            representation.Send(release);

            cursor.NativeData = new CursorData(nsCursor);
        }

        protected internal override void DestroyCursor(Cursor cursor)
        {
            GetId(cursor).Send(release);
            cursor.NativeData = null;
        }

        protected internal override void SetWindowCursor(Window window)
        {
            if (IsCursorInClientAreaOutsideOfEventStream(window))
            {
                UpdateWindowCursor(window);
            }
        }

        protected internal override double GetEventTime() => eventTimestamp;

        protected internal override string GetClipboardString()
        {
            var pasteboard = NSPasteboard.Get(generalPasteboard);
            if (!pasteboard.Get(types).GetBool(containsObject_, NSStringPboardType))
            {
                return string.Empty;
            }
            var s = pasteboard.Get(stringForType_, NSStringPboardType);
            return s.IsNil ? string.Empty : new NSString(s.Handle).ToString();
        }

        protected internal override unsafe void SetClipboardString(string text)
        {
            var array = stackalloc Id[1];
            array[0] = NSStringPboardType;
            var types = NSArray.Get(arrayWithObjects_count_, new IntPtr(array), 1);
            var pasteboard = NSPasteboard.Get(generalPasteboard);
            pasteboard.GetInteger(declareTypes_owner_, types, default);
            pasteboard.Send(setString_forType_, new NSString(text), NSStringPboardType);
        }

        protected internal override ModifierKeys GetModifierKeys() => TranslateModifierKeys(NSEvent.GetUInteger(modifierFlags));

        protected internal override string ConvertKeycodeToString(Keycode keycode)
        {
            var unicodeData = new Id(TISGetInputSourceProperty(inputSource, KTISPropertyUnicodeKeyLayoutData));
            var scancode = (ushort)scancodes[(int)keycode];
            uint deadKeyState = 0;
            unsafe
            {
                var characters = stackalloc char[8];
                if (UCKeyTranslate(unicodeData.GetIntPtr(bytes), scancode, kUCKeyActionDisplay, 0, LMGetKbdType(),
                    kUCKeyTranslateNoDeadKeysBit, ref deadKeyState, 8, out var characterCount, characters) != 0)
                {
                    return string.Empty;
                }
                if (characterCount == 0)
                {
                    return string.Empty;
                }
                return Marshal.PtrToStringUni(new IntPtr(characters), (int)characterCount);
            }
        }

        protected internal override bool WaitAndProcessEvents()
        {
            var @event = NSApp.Get(nextEventMatchingMask_untilDate_inMode_dequeue_, NSEventMaskAny,
                distantFutureDate, NSDefaultRunLoopMode, true);
            NSApp.Send(sendEvent_, @event);
            autoreleasePool.Send(release);
            autoreleasePool = NSAutoreleasePool.Get(@new);
            ProcessEvents();
            return true;
        }

        protected internal override bool WaitAndProcessEvents(double timeout)
        {
            var date = NSDate.Get(dateWithTimeIntervalSinceNow_, timeout);
            var @event = NSApp.Get(nextEventMatchingMask_untilDate_inMode_dequeue_, NSEventMaskAny,
                date, NSDefaultRunLoopMode, true);
            if (@event.IsNil)
            {
                return false;
            }
            NSApp.Send(sendEvent_, @event);
            autoreleasePool.Send(release);
            autoreleasePool = NSAutoreleasePool.Get(@new);
            ProcessEvents();
            return true;
        }

        protected internal override bool ProcessEvents()
        {
            var result = false;
            for (;;)
            {
                var @event = NSApp.Get(nextEventMatchingMask_untilDate_inMode_dequeue_, NSEventMaskAny,
                    distantPastDate, NSDefaultRunLoopMode, true);
                if (@event.IsNil)
                {
                    break;
                }
                result = true;
                NSApp.Send(sendEvent_, @event);
                autoreleasePool.Send(release);
                autoreleasePool = NSAutoreleasePool.Get(@new);
            }
            return result;
        }

        protected internal override void UnblockProcessEvents()
        {
            var nsEvent = NSEvent.Get(otherEventWithType_location_modifierFlags_timestamp_windowNumber_context_subtype_data1_data2_,
                NSEventTypeApplicationDefined, default, 0, 0, 0, default, 0, 0, 0);
            NSApp.Send(postEvent_atStart_, nsEvent, true);
        }

        // Application ----------------------------------------------------------------------------

        private static void ApplicationSendEvent(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var receiver = new Id(receiverPtr);
            var @event = new Id(eventPtr);
            if (@event.GetUInteger(type) == NSKeyUp && ((@event.GetUInteger(modifierFlags) & NSCommandKeyMask) != 0))
            {
                var activeWindow = receiver.Get(keyWindow);
                if (!activeWindow.IsNil)
                {
                    activeWindow.Send(new SEL(selectorPtr), @event);
                    return;
                }
            }
            receiver.SuperSend(Shared.NSApplication, new SEL(selectorPtr), @event);
        }

        private static void ApplicationStartNewThread(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr objectPtr)
        {
        }

        private static Class CreateApplicationClass()
        {
            return Class.Create("NStuffApplication", Shared.NSApplication, (builder) => {
                AddMethod<MessagePtr>(builder, "sendEvent:", ApplicationSendEvent, "v@:@");
                AddMethod<MessagePtr>(builder, startNewThread_, ApplicationStartNewThread, "v@:@");
            });
        }

        // ApplicationDelegate --------------------------------------------------------------------

        private static ulong ApplicationDelegateApplicationShouldTerminate(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr applicationPtr)
        {
            foreach (var window in new List<Window>(Shared.windows.Values))
            {
                CloseRequestOccurred(window);
            }
            return NSTerminateCancel;
        }

        private static void ApplicationDelegateApplicationDidFinishLaunching(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            Shared.NSApp.Send(stop_, default(Id));
            Shared.NSApp.Send(activateIgnoringOtherApps_, true);
        }

        private static void ApplicationDelegateApplicationDidHide(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            foreach (var window in new List<Window>(Shared.windows.Values))
            {
                VisibleChangedEventOccurred(window);
            }
        }

        private static void ApplicationDelegateApplicationDidUnhide(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            foreach (var window in new List<Window>(Shared.windows.Values))
            {
                VisibleChangedEventOccurred(window);
            }
        }

        private static Class CreateApplicationDelegateClass()
        {
            return Class.Create("NStuffApplicationDelegate", Class.Lookup("NSObject"), (builder) => {
                AddMethod<UIntegerMessagePtr>(builder, "applicationShouldTerminate:", ApplicationDelegateApplicationShouldTerminate, "Q@:@");
                AddMethod<MessagePtr>(builder, "applicationDidFinishLaunching:", ApplicationDelegateApplicationDidFinishLaunching, "v@:@");
                AddMethod<MessagePtr>(builder, "applicationDidHide:", ApplicationDelegateApplicationDidHide, "v@:@");
                AddMethod<MessagePtr>(builder, "applicationDidUnhide:", ApplicationDelegateApplicationDidUnhide, "v@:@");
            });
        }

        // Window ---------------------------------------------------------------------------------

        private static bool WindowCanBecomeKeyWindow(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static bool WindowCanBecomeMainWindow(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static Class CreateWindowClass()
        {
            return Class.Create("NStuffWindow", Class.Lookup("NSWindow"), (builder) => {
                AddMethod<BoolMessage>(builder, "canBecomeKeyWindow", WindowCanBecomeKeyWindow, "B@:");
                AddMethod<BoolMessage>(builder, "canBecomeMainWindow", WindowCanBecomeMainWindow, "B@:");
            });
        }

        // WindowDelegate -------------------------------------------------------------------------

        private static bool WindowDelegateWindowShouldClose(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr windowPtr)
        {
            if (Shared.windows.TryGetValue(windowPtr, out var window))
            {
                CloseRequestOccurred(window);
            }
            return false;
        }

        private static void WindowDelegateWindowDidResize(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                window.RenderingContext?.UpdateRenderingData(window);
                if (Shared.freeLookMouseWindow == window)
                {
                    Shared.CenterCursor(window);
                }
                ResizeEventOccurred(window);
                FramebufferResizeEventOccurred(window);
            }
        }

        private static void WindowDelegateWindowDidMove(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                window.RenderingContext?.UpdateRenderingData(window);
                if (Shared.freeLookMouseWindow == window)
                {
                    Shared.CenterCursor(window);
                }
                MoveEventOccurred(window);
            }
        }

        private static void WindowDelegateWindowDidMiniaturize(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                SizeStateChangedEventOccurred(window);
            }
        }

        private static void WindowDelegateWindowDidDeminiaturize(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                SizeStateChangedEventOccurred(window);
            }
        }

        private static void WindowDelegateWindowDidBecomeKey(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                var view = nsWindow.Get(contentView);
                view.Send(updateTrackingAreas);
                GotFocusEventOccurred(window);

                var data = GetData(window);
                if (!data.MouseInside)
                {
                    if (IsCursorInClientArea(nsWindow))
                    {
                        data.MouseInside = true;
                        MouseEnterEventOccurred(window);
                        Shared.SetFreeLookMouseWindow(window, window.FreeLookMouse);
                    }
                    else
                    {
                        if (window.FreeLookMouse)
                        {
                            Shared.UnhideCursor();
                        }
                    }
                }
                else
                {
                    if (window.FreeLookMouse)
                    {
                        if (!IsCursorInClientArea(nsWindow))
                        {
                            data.MouseInside = false;
                            Shared.UnhideCursor();
                        }
                    }
                }
            }
        }

        private static void WindowDelegateWindowDidResignKey(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr notificationPtr)
        {
            var nsWindow = new Id(notificationPtr).Get(@object);
            if (TryGetWindow(nsWindow, out var window))
            {
                LostFocusEventOccurred(window);
            }
        }

        private static Class CreateWindowDelegateClass()
        {
            return Class.Create("NStuffWindowDelegate", Class.Lookup("NSObject"), (builder) => {
                AddMethod<BoolMessagePtr>(builder, "windowShouldClose:", WindowDelegateWindowShouldClose, "B@:@");
                AddMethod<MessagePtr>(builder, "windowDidResize:", WindowDelegateWindowDidResize, "v@:@");
                AddMethod<MessagePtr>(builder, "windowDidMove:", WindowDelegateWindowDidMove, "v@:@");
                AddMethod<MessagePtr>(builder, "windowDidMiniaturize:", WindowDelegateWindowDidMiniaturize, "v@:@");
                AddMethod<MessagePtr>(builder, "windowDidDeminiaturize:", WindowDelegateWindowDidDeminiaturize, "v@:@");
                AddMethod<MessagePtr>(builder, "windowDidBecomeKey:", WindowDelegateWindowDidBecomeKey, "v@:@");
                AddMethod<MessagePtr>(builder, "windowDidResignKey:", WindowDelegateWindowDidResignKey, "v@:@");
            });
        }

        // View -----------------------------------------------------------------------------------

        private static IntPtr ViewInit(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr).SuperGet(Shared.NSView, init);
            if (!self.IsNil)
            {
                self.Send(updateTrackingAreas);

                unsafe
                {
                    var array = stackalloc Id[1];
                    array[0] = Shared.NSFilenamesPboardType;
                    var types = Shared.NSArray.Get(arrayWithObjects_count_, new IntPtr(array), 1);
                    self.Send(registerForDraggedTypes_, types);
                }
            }
            return self.Handle;
        }

        private static bool ViewIsOpaque(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static bool ViewCanBecomeKeyView(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static bool ViewAcceptsFirstResponder(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static bool ViewWantsUpdateLayer(IntPtr receiverPtr, IntPtr selectorPtr) => true;

        private static void ViewUpdateLayer(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                window.RenderingContext?.UpdateRenderingData(window);
            }
        }

        private static void ViewUpdateTrackingAreas(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var data = GetData(window);
                if (!data.TrackingArea.IsNil)
                {
                    self.Send(removeTrackingArea_, data.TrackingArea);
                    data.TrackingArea.Send(release);
                }
                data.TrackingArea = Shared.NSTrackingArea.Get(alloc).Get(initWithRect_options_owner_userInfo_,
                    self.GetRect(bounds),
                    NSTrackingMouseEnteredAndExited | NSTrackingMouseMoved | NSTrackingCursorUpdate |
                    NSTrackingActiveInActiveApp | NSTrackingInVisibleRect | NSTrackingAssumeInside,
                    self, default);
                self.Send(addTrackingArea_, data.TrackingArea);
            }
            self.SuperSend(Shared.NSView, updateTrackingAreas);
        }

        private static void ViewDidChangeBackingProperties(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                FramebufferResizeEventOccurred(window);
            }
        }

        private static void ViewCursorUpdate(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                Shared.UpdateWindowCursor(window);
            }
        }

        private static void ViewMouseDown(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                Shared.eventTimestamp = @event.GetDouble(timestamp);
                MouseDownEventOccurred(window, (MouseButton)@event.GetInteger(buttonNumber));
            }
        }

        private static void ViewMouseUp(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                Shared.eventTimestamp = @event.GetDouble(timestamp);
                MouseUpEventOccurred(window, (MouseButton)@event.GetInteger(buttonNumber));
                var data = GetData(window);
                if (data.MouseInside &&
                    Shared.NSEvent.GetUInteger(pressedMouseButtons) == 0 &&
                    !IsEventInsideFrame(@event, self.GetRect(frame)))
                {
                    data.MouseInside = false;
                    MouseLeaveEventOccurred(window);
                }
            }
        }

        private static void ViewMouseDragged(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                Shared.eventTimestamp = @event.GetDouble(timestamp);
                var data = GetData(window);
                if (Shared.freeLookMouseWindow == window)
                {
                    var (wx, wy) = data.CursorWrapDelta;
                    var dx = @event.GetDouble(deltaX) - wx;
                    var dy = @event.GetDouble(deltaY) - wy;
                    MouseMoveEventOccurred(window, window.FreeLookPosition.x + dx, window.FreeLookPosition.y + dy);
                }
                else
                {
                    var frameRect = self.GetRect(frame);
                    var location = @event.GetPoint(locationInWindow);
                    MouseMoveEventOccurred(window, location.X, frameRect.Height - location.Y);
                }

                data.CursorWrapDelta = (0, 0);
            }
        }

        private static void ViewMouseEntered(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out Window? window))
            {
                var data = GetData(window);
                if (!data.MouseInside)
                {
                    data.MouseInside = true;
                    var @event = new Id(eventPtr);
                    Shared.eventTimestamp = @event.GetDouble(timestamp);
                    MouseEnterEventOccurred(window);
                    if (!window.FreeLookMouse)
                    {
                        var frameRect = self.GetRect(frame);
                        var location = @event.GetPoint(locationInWindow);
                        MouseMoveEventOccurred(window, location.X, frameRect.Height - location.Y);
                    }
                    Shared.SetFreeLookMouseWindow(window, window.FreeLookMouse);
                }
            }
        }

        private static void ViewMouseExited(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            if (Shared.NSEvent.GetUInteger(pressedMouseButtons) != 0)
            {
                return;
            }
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var data = GetData(window);
                if (data.MouseInside)
                {
                    data.MouseInside = false;
                    var @event = new Id(eventPtr);
                    Shared.eventTimestamp = @event.GetDouble(timestamp);
                    MouseLeaveEventOccurred(window);
                }
            }
        }

        private static void ViewScrollWheel(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                var deltaX = @event.GetDouble(scrollingDeltaX);
                var deltaY = @event.GetDouble(scrollingDeltaY);
                if (@event.GetBool(hasPreciseScrollingDeltas))
                {
                    deltaX *= 0.1;
                    deltaY *= 0.1;
                }
                if (deltaX != 0 || deltaY != 0)
                {
                    Shared.eventTimestamp = @event.GetDouble(timestamp);
                    ScrollEventOccurred(window, -deltaX, -deltaY);
                }
            }
        }

        private static void ViewKeyDown(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                var keycode = TranslateKeyCode(@event.GetUShort(keyCode));
                if (keycode != Keycode.Unknown)
                {
                    var modifiers = TranslateModifierKeys(@event.GetUInteger(modifierFlags));
                    Shared.eventTimestamp = @event.GetDouble(timestamp);
                    KeyDownEventOccurred(window, keycode, modifiers);
                }
                self.Send(interpretKeyEvents_, Shared.NSArray.Get(arrayWithObject_, @event));
            }
        }

        private static void ViewKeyUp(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                var keycode = TranslateKeyCode(@event.GetUShort(keyCode));
                if (keycode != Keycode.Unknown)
                {
                    var modifiers = TranslateModifierKeys(@event.GetUInteger(modifierFlags));
                    Shared.eventTimestamp = @event.GetDouble(timestamp);
                    KeyUpEventOccurred(window, keycode, modifiers);
                }
            }
        }

        private static void ViewFlagsChanged(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @event = new Id(eventPtr);
                var flags = @event.GetUInteger(modifierFlags) & NSDeviceIndependentModifierFlags;
                var keycode = TranslateKeyCode(@event.GetUShort(keyCode));
                var modifiers = TranslateModifierKeys(flags);
                Shared.eventTimestamp = @event.GetDouble(timestamp);
                var data = GetData(window);
                if (flags == data.ModifierFlags)
                {
                    if (window.PressedKeys[(uint)keycode])
                    {
                        KeyUpEventOccurred(window, keycode, modifiers);
                    }
                    else
                    {
                        KeyDownEventOccurred(window, keycode, modifiers);
                    }
                }
                else
                {
                    if (flags > data.ModifierFlags)
                    {
                        KeyDownEventOccurred(window, keycode, modifiers);
                    }
                    else
                    {
                        KeyUpEventOccurred(window, keycode, modifiers);
                    }
                    data.ModifierFlags = flags;
                }
            }
        }

        private static ulong ViewDraggingEntered(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr senderPtr)
        {
            var self = new Id(receiverPtr);
            var sender = new Id(senderPtr);
            if ((sender.GetUInteger(draggingSourceOperationMask) & NSDragOperationGeneric) != 0)
            {
                self.Send(setNeedsDisplay_, true);
                return NSDragOperationGeneric;
            }
            return NSDragOperationNone;
        }

        private static bool ViewPrepareForDragOperation(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr senderPtr)
        {
            var self = new Id(receiverPtr);
            self.Send(setNeedsDisplay_, true);
            return true;
        }

        private static bool ViewPerformDragOperation(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr senderPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            var sender = new Id(senderPtr);
            if (TryGetWindow(nsWindow, out var window))
            {
                var pasteboard = sender.Get(draggingPasteboard);
                var files = pasteboard.Get(propertyListForType_, Shared.NSFilenamesPboardType);
                var fileCount = files.GetUInteger(count);
                if (fileCount > 0)
                {
                    var paths = new string[fileCount];
                    var e = files.Get(objectEnumerator);
                    for (int i = 0; i < (int)fileCount; i++)
                    {
                        var s = e.Get(nextObject);
                        paths[i] = new NSString(s.Handle).ToString();
                    }
                    var frameRect = self.GetRect(frame);
                    var location = sender.GetPoint(draggingLocation);
                    FileDropEventOccurred(window, location.X, frameRect.Height - location.Y, paths);
                }
            }
            return true;
        }

        private static void ViewConcludeDragOperation(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr senderPtr)
        {
            var self = new Id(receiverPtr);
            self.Send(setNeedsDisplay_, true);
        }

        private static bool ViewHasMarkedText(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                return GetData(window).MarkedText.GetUInteger(length) > 0;
            }
            return false;
        }

        private static NSRange ViewMarkedRange(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var textLength = GetData(window).MarkedText.GetUInteger(length);
                if (textLength > 0)
                {
                    return new NSRange(0, textLength - 1);
                }
            }
            return new NSRange();
        }

        private static NSRange ViewSelectedRange(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            return new NSRange();
        }

        private static void ViewSetMarkedText_SelectedRange_ReplacementRange(IntPtr receiverPtr, IntPtr selectorPtr,
            IntPtr stringPtr, NSRange selectedRange, NSRange replacementRange)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var @string = new Id(stringPtr);
                var data = GetData(window);
                data.MarkedText.Send(release);
                if (@string.GetBool(isKindOfClass_, Shared.NSAttributedString.Get(@class)))
                {
                    data.MarkedText = Shared.NSMutableAttributedString.Get(alloc).Get(initWithAttributedString_, @string);
                }
                else
                {
                    data.MarkedText = Shared.NSMutableAttributedString.Get(alloc).Get(initWithString_, @string);
                }
            }
        }

        private static void ViewUnmarkedText(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                GetData(window).MarkedText.Get(mutableString).Send(setString_, new NSString(""));
            }
        }

        private static IntPtr ViewValidAttributesForMarkedText(IntPtr receiverPtr, IntPtr selectorPtr)
        {
            return Shared.NSArray.Get(array).Handle;
        }

        private static IntPtr ViewAttributedSubstringForProposedRange_actualRange(IntPtr receiverPtr, IntPtr selectorPtr,
            NSRange range, out NSRange actualRange)
        {
            actualRange = new NSRange();
            return IntPtr.Zero;
        }

        private static ulong ViewCharacterIndexForPoint(IntPtr receiverPtr, IntPtr selectorPtr, NSPoint point)
        {
            return 0;
        }

        private static NSRect ViewFirstRectForCharacterRange_actualRange(IntPtr receiverPtr, IntPtr selectorPtr,
            NSRange range, out NSRange actualRange)
        {
            actualRange = range;
            var self = new Id(receiverPtr);
            var frameRect = self.GetRect(frame);
            return new NSRect(frameRect.X, frameRect.Y, 0, 0);
        }

        private static void ViewInsertText_replacementRange(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr stringPtr, NSRange range)
        {
            var self = new Id(receiverPtr);
            var nsWindow = self.Get(Selectors.window);
            if (TryGetWindow(nsWindow, out var window))
            {
                var nsString = new Id(stringPtr);
                if (nsString.GetBool(isKindOfClass_, Shared.NSAttributedString.Get(@class)))
                {
                    nsString = nsString.Get(@string);
                }
                var text = new NSString(nsString.Handle).ToString();
                if (text.Length > 0)
                {
                    var nsEvent = Shared.NSApp.Get(currentEvent);
                    var modifiers = TranslateModifierKeys(nsEvent.GetUInteger(modifierFlags));
                    for (int i = 0; i < text.Length; i++)
                    {
                        var c = text[i];
                        if (char.IsHighSurrogate(c))
                        {
                            if (i + 1 < text.Length)
                            {
                                TextInputEventOccurred(window, char.ConvertToUtf32(c, text[++i]), modifiers);
                            }
                        }
                        else
                        {
                            TextInputEventOccurred(window, c, modifiers);
                        }
                    }
                }
            }
        }

        private static void ViewDoCommandBySelector(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr commandSel)
        {
        }

        private static Class CreateViewClass()
        {
            return Class.Create("NStuffView", Shared.NSView, (builder) => {
                builder.AddProtocol(Protocol.Get("NSTextInputClient"));

                AddMethod<PtrMessage>(builder, init, ViewInit, "@@:");
                AddMethod<BoolMessage>(builder, "isOpaque", ViewIsOpaque, "B@:");
                AddMethod<BoolMessage>(builder, "canBecomeKeyView", ViewCanBecomeKeyView, "B@:");
                AddMethod<BoolMessage>(builder, "acceptsFirstResponder", ViewAcceptsFirstResponder, "B@:");
                AddMethod<BoolMessage>(builder, "wantsUpdateLayer", ViewWantsUpdateLayer, "B@:");
                AddMethod<Message>(builder, "updateLayer", ViewUpdateLayer, "v@:");
                AddMethod<Message>(builder, "updateTrackingAreas", ViewUpdateTrackingAreas, "v@:");
                AddMethod<Message>(builder, "viewDidChangeBackingProperties", ViewDidChangeBackingProperties, "v@:");
                AddMethod<MessagePtr>(builder, "cursorUpdate:", ViewCursorUpdate, "v@:@");
                AddMethod<MessagePtr>(builder, "mouseEntered:", ViewMouseEntered, "v@:@");
                AddMethod<MessagePtr>(builder, "mouseExited:", ViewMouseExited, "v@:@");
                AddMethod<MessagePtr>(builder, "scrollWheel:", ViewScrollWheel, "v@:@");
                AddMethod<MessagePtr>(builder, "keyDown:", ViewKeyDown, "v@:@");
                AddMethod<MessagePtr>(builder, "keyUp:", ViewKeyUp, "v@:@");
                AddMethod<MessagePtr>(builder, "flagsChanged:", ViewFlagsChanged, "v@:@");
                AddMethod<UIntegerMessagePtr>(builder, "draggingEntered:", ViewDraggingEntered, "Q@:@");
                AddMethod<BoolMessagePtr>(builder, "prepareForDragOperation:", ViewPrepareForDragOperation, "B@:@");
                AddMethod<BoolMessagePtr>(builder, "performDragOperation:", ViewPerformDragOperation, "B@:@");
                AddMethod<MessagePtr>(builder, "concludeDragOperation:", ViewConcludeDragOperation, "v@:@");
                AddMethod<BoolMessage>(builder, "hasMarkedText", ViewHasMarkedText, "B@:");
                AddMethod<RangeMessage>(builder, "markedRange", ViewMarkedRange, "{NSRange=QQ}@:");
                AddMethod<RangeMessage>(builder, "selectedRange", ViewSelectedRange, "{NSRange=QQ}@:");
                AddMethod<MessagePtrRangeRange>(builder, "setMarkedText:selectedRange:replacementRange:",
                    ViewSetMarkedText_SelectedRange_ReplacementRange, "v@:@{NSRange=QQ}{NSRange=QQ}");
                AddMethod<Message>(builder, "unmarkText", ViewUnmarkedText, "v@:");
                AddMethod<PtrMessage>(builder, "validAttributesForMarkedText", ViewValidAttributesForMarkedText, "@@:");
                AddMethod<PtrMessageRangeOutRange>(builder, "attributedStringForProposedRange:actualRange:",
                    ViewAttributedSubstringForProposedRange_actualRange, "@@:{NSRange=QQ}^{NSRange=QQ}");
                AddMethod<UIntegerMessagePoint>(builder, "characterIndexForPoint:", ViewCharacterIndexForPoint, "Q@:{NSPoint=dd}");
                AddMethod<RectMessageRangeOutRange>(builder, "firstRectForCharacterRange:actualRange:",
                    ViewFirstRectForCharacterRange_actualRange, "{NSRect=dddd}@:{NSRange=QQ}^{NSRange=QQ}");
                AddMethod<MessagePtrRange>(builder, "insertText:replacementRange:", ViewInsertText_replacementRange, "v@:@{NSRange=QQ}");
                AddMethod<MessagePtr>(builder, "doCommandBySelector:", ViewDoCommandBySelector, "v@::");

                MessagePtr viewMouseDown = ViewMouseDown;
                implementations.Add(viewMouseDown);
                builder.AddMethod(SEL.Register("mouseDown:"), viewMouseDown, "v@:@");
                builder.AddMethod(SEL.Register("otherMouseDown:"), viewMouseDown, "v@:@");
                builder.AddMethod(SEL.Register("rightMouseDown:"), viewMouseDown, "v@:@");

                MessagePtr viewMouseUp = ViewMouseUp;
                implementations.Add(viewMouseUp);
                builder.AddMethod(SEL.Register("mouseUp:"), viewMouseUp, "v@:@");
                builder.AddMethod(SEL.Register("otherMouseUp:"), viewMouseUp, "v@:@");
                builder.AddMethod(SEL.Register("rightMouseUp:"), viewMouseUp, "v@:@");

                MessagePtr viewMouseDragged = ViewMouseDragged;
                implementations.Add(viewMouseDragged);
                builder.AddMethod(SEL.Register("mouseDragged:"), viewMouseDragged, "v@:@");
                builder.AddMethod(SEL.Register("mouseMoved:"), viewMouseDragged, "v@:@");
                builder.AddMethod(SEL.Register("otherMouseDragged:"), viewMouseDragged, "v@:@");
                builder.AddMethod(SEL.Register("rightMouseDragged:"), viewMouseDragged, "v@:@");
            });
        }

        // ----------------------------------------------------------------------------------------

        private static NativeWindowServer Shared => shared ?? throw new InvalidOperationException();

        private static WindowData GetData(Window window) => (WindowData?)window.NativeData ?? throw new InvalidOperationException();

        private static Id GetId(Window window) => GetData(window).Id;

        private static Id GetId(Cursor cursor) => ((CursorData?)cursor.NativeData ?? throw new InvalidOperationException()).Id;

        private static void AddMethod<TDelegate>(ClassBuilder builder, string selector, TDelegate implementation, string types)
            where TDelegate : class => AddMethod(builder, SEL.Register(selector), implementation, types);

        private static void AddMethod<TDelegate>(ClassBuilder builder, SEL selector, TDelegate implementation, string types) where TDelegate : class
        {
            implementations.Add(implementation);
            builder.AddMethod(selector, implementation, types);
        }

        private static Id AddMenuItem(Id menu, string? title, SEL selector, string? key, ulong mask) =>
            AddMenuItem(menu, (title == null) ? NSString.Empty : new NSString(title),
                selector, (key == null) ? NSString.Empty : new NSString(key), mask);

        private static Id AddMenuItem(Id menu, NSString title, SEL selector, NSString key, ulong mask)
        {
            var result = menu.Get(addItemWithTitle_action_keyEquivalent_, title, selector, key);
            if (mask != 0)
            {
                result.Send(setKeyEquivalentModifierMask_, mask);
            }
            return result;
        }

        private static bool TryGetWindow(Id nsWindow, [NotNullWhen(returnValue: true)] out Window? window) =>
            Shared.windows.TryGetValue(nsWindow.Handle, out window);

        private void CenterCursor(Window window)
        {
            var (width, height) = GetWindowSize(window);
            SetCursorPosition(window, (width / 2, height / 2));
        }

        private static bool IsCursorInClientAreaOutsideOfEventStream(Window window)
        {
            var id = GetId(window);
            var view = id.Get(contentView);
            var frameRect = view.GetRect(frame);
            var position = id.GetPoint(mouseLocationOutsideOfEventStream);
            return view.GetBool(mouse_inRect_, position, frameRect);
        }

        private static bool IsCursorInClientArea(Id nsWindow)
        {
            var location = Shared.NSEvent.GetPoint(mouseLocation);
            var rect = new NSRect(location.X, location.Y, 0, 0);
            rect = nsWindow.GetRect(convertRectFromScreen_, rect);
            var view = nsWindow.Get(contentView);
            var frameRect = view.GetRect(frame);
            var x = rect.X;
            var y = frameRect.Height - rect.Y;
            return x >= 0 && x < frameRect.Width && y >= 0 && y < frameRect.Height;
        }

        private void UnhideCursor()
        {
            if (cursorHidden)
            {
                NSCursor.Send(unhide);
                cursorHidden = false;
            }
            Shared.freeLookMouseWindow = null;
            CGAssociateMouseAndMouseCursorPosition(1);
        }

        private void UpdateWindowCursor(Window window)
        {
            if (freeLookMouseWindow == window)
            {
                if (!cursorHidden)
                {
                    NSCursor.Send(hide);
                    cursorHidden = true;
                }
            }
            else
            {
                if (cursorHidden)
                {
                    NSCursor.Send(unhide);
                    cursorHidden = false;
                }
                var cursor = window.Cursor;
                if (cursor == null)
                {
                    NSCursor.Get(arrowCursor).Send(set);
                }
                else
                {
                    GetId(cursor).Send(set);
                }
            }
        }

        private static double ConvertY(double y) => CGDisplayBounds(CGMainDisplayID()).Size.Height - y - 1;

        private static bool IsEventInsideFrame(Id @event, NSRect frame)
        {
            var location = @event.GetPoint(locationInWindow);
            var x = location.X;
            var y = frame.Height - location.Y;
            return x >= 0 && x < frame.Width && y >= 0 && y < frame.Height;
        }

        private static ModifierKeys TranslateModifierKeys(ulong modifiers)
        {
            var result = ModifierKeys.None;
            if ((modifiers & NSShiftKeyMask) != 0)
            {
                result = ModifierKeys.Shift;
            }
            if ((modifiers & NSControlKeyMask) != 0)
            {
                result |= ModifierKeys.Control;
            }
            if ((modifiers & NSAlternateKeyMask) != 0)
            {
                result |= ModifierKeys.Alternate;
            }
            if ((modifiers & NSCommandKeyMask) != 0)
            {
                result |= ModifierKeys.Command;
            }
            if ((modifiers & NSAlphaShiftKeyMask) != 0)
            {
                result |= ModifierKeys.CapsLock;
            }
            return result;
        }

        private static Keycode TranslateKeyCode(ushort keyCode) => (keyCode > 127) ? Keycode.Unknown : keycodeMappings[keyCode];

        private delegate void Message(IntPtr receiverPtr, IntPtr selectorPtr);
        private delegate void MessagePtr(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr eventPtr);
        private delegate bool BoolMessage(IntPtr receiverPtr, IntPtr selectorPtr);
        private delegate bool BoolMessagePtr(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr windowPtr);
        private delegate IntPtr PtrMessage(IntPtr receiverPtr, IntPtr selectorPtr);
        private delegate ulong UIntegerMessagePtr(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr windowPtr);
        private delegate NSRange RangeMessage(IntPtr receiverPtr, IntPtr selectorPtr);
        private delegate void MessagePtrRangeRange(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr idPtr, NSRange range0, NSRange range1);
        private delegate IntPtr PtrMessageRangeOutRange(IntPtr receiverPtr, IntPtr selectorPtr, NSRange range, out NSRange outRange);
        private delegate ulong UIntegerMessagePoint(IntPtr receiverPtr, IntPtr selectorPtr, NSPoint point);
        private delegate NSRect RectMessageRangeOutRange(IntPtr receiverPtr, IntPtr selectorPtr, NSRange range, out NSRange outRange);
        private delegate void MessagePtrRange(IntPtr receiverPtr, IntPtr selectorPtr, IntPtr idPtr, NSRange range);

        static NativeWindowServer()
        {
            keycodeMappings = new Keycode[] {
                Keycode.A, Keycode.S, Keycode.D, Keycode.F, Keycode.H,
                Keycode.G, Keycode.Z, Keycode.X, Keycode.C, Keycode.V,
                Keycode.World1, Keycode.B, Keycode.Q, Keycode.W, Keycode.E,
                Keycode.R, Keycode.Y, Keycode.T, Keycode.One, Keycode.Two,
                Keycode.Three, Keycode.Four, Keycode.Six, Keycode.Five, Keycode.Equal,
                Keycode.Nine, Keycode.Seven, Keycode.Minus, Keycode.Eight, Keycode.Zero,
                Keycode.RightBracket, Keycode.O, Keycode.U, Keycode.LeftBracket, Keycode.I,
                Keycode.P, Keycode.Enter, Keycode.L, Keycode.J, Keycode.Apostrophe,
                Keycode.K, Keycode.SemiColon, Keycode.Backslash, Keycode.Comma, Keycode.Slash,
                Keycode.N, Keycode.M, Keycode.Dot, Keycode.Tab, Keycode.Space,

                Keycode.Backquote, Keycode.Backspace, Keycode.Unknown, Keycode.Escape, Keycode.RightCommand,
                Keycode.LeftCommand, Keycode.LeftShift, Keycode.CapsLock, Keycode.LeftAlternate, Keycode.LeftControl,
                Keycode.RightShift, Keycode.RightAlternate, Keycode.RightControl, Keycode.Unknown, Keycode.F17,
                Keycode.KeypadDot, Keycode.Unknown, Keycode.KeypadAsterisk, Keycode.Unknown, Keycode.KeypadPlus,
                Keycode.Unknown, Keycode.NumLock, Keycode.Unknown, Keycode.Unknown, Keycode.Unknown,
                Keycode.KeypadSlash, Keycode.KeypadEnter, Keycode.Unknown, Keycode.KeypadMinus, Keycode.F18,
                Keycode.F19, Keycode.KeypadEqual, Keycode.KeypadZero, Keycode.KeypadOne, Keycode.KeypadTwo,
                Keycode.KeypadThree, Keycode.KeypadFour, Keycode.KeypadFive, Keycode.KeypadSix, Keycode.KeypadSeven,
                Keycode.F20, Keycode.KeypadEight, Keycode.KeypadNine, Keycode.Unknown, Keycode.Unknown,
                Keycode.Unknown, Keycode.F5, Keycode.F6, Keycode.F7, Keycode.F3,

                Keycode.F8, Keycode.F9, Keycode.Unknown, Keycode.F11, Keycode.Unknown,
                Keycode.F13, Keycode.F16, Keycode.F14, Keycode.Unknown, Keycode.F10,
                Keycode.Menu, Keycode.F12, Keycode.Unknown, Keycode.F15, Keycode.Insert,
                Keycode.Home, Keycode.PageUp, Keycode.Delete, Keycode.F4, Keycode.End,
                Keycode.F2, Keycode.PageDown, Keycode.F1, Keycode.Left, Keycode.Right,
                Keycode.Down, Keycode.Up, Keycode.Unknown
            };

            for (int i = 0; i < keycodeMappings.Length; i++)
            {
                var keycode = keycodeMappings[i];
                if (keycode != Keycode.Unknown)
                {
                    scancodes[(int)keycode] = i;
                }
            }
        }
    }
}
