using NStuff.Runtime.InteropServices.ObjectiveC;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// Defines some Objective C selectors used to implement the native window server.
    /// </summary>
    public static class Selectors
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// The <c>activateIgnoringOtherApps:</c> selector.
        /// </summary>
        public static SEL activateIgnoringOtherApps_ { get; } = SEL.Register("activateIgnoringOtherApps:");

        /// <summary>
        /// The <c>addItem:</c> selector.
        /// </summary>
        public static SEL addItem_ { get; } = SEL.Register("addItem:");

        /// <summary>
        /// The <c>addItemWithTitle:action:keyEquivalent:</c> selector.
        /// </summary>
        public static SEL addItemWithTitle_action_keyEquivalent_ { get; } = SEL.Register("addItemWithTitle:action:keyEquivalent:");

        /// <summary>
        /// The <c>addRepresentation:</c> selector.
        /// </summary>
        public static SEL addRepresentation_ { get; } = SEL.Register("addRepresentation:");

        /// <summary>
        /// The <c>addTrackingArea:</c> selector.
        /// </summary>
        public static SEL addTrackingArea_ { get; } = SEL.Register("addTrackingArea:");

        /// <summary>
        /// The <c>alphaValue</c> selector.
        /// </summary>
        public static SEL alphaValue { get; } = SEL.Register("alphaValue");

        /// <summary>
        /// The <c>arrangeInFront:</c> selector.
        /// </summary>
        public static SEL arrangeInFront_ { get; } = SEL.Register("arrangeInFront:");

        /// <summary>
        /// The <c>array</c> selector.
        /// </summary>
        public static SEL array { get; } = SEL.Register("array");

        /// <summary>
        /// The <c>arrayWithObject:</c> selector.
        /// </summary>
        public static SEL arrayWithObject_ { get; } = SEL.Register("arrayWithObject:");

        /// <summary>
        /// The <c>arrayWithObjects:count:</c> selector.
        /// </summary>
        public static SEL arrayWithObjects_count_ { get; } = SEL.Register("arrayWithObjects:count:");

        /// <summary>
        /// The <c>arrowCursor</c> selector.
        /// </summary>
        public static SEL arrowCursor { get; } = SEL.Register("arrowCursor");

        /// <summary>
        /// The <c>bitmapData</c> selector.
        /// </summary>
        public static SEL bitmapData { get; } = SEL.Register("bitmapData");

        /// <summary>
        /// The <c>bounds</c> selector.
        /// </summary>
        public static SEL bounds { get; } = SEL.Register("bounds");

        /// <summary>
        /// The <c>buttonNumber</c> selector.
        /// </summary>
        public static SEL buttonNumber { get; } = SEL.Register("buttonNumber");

        /// <summary>
        /// The <c>bytes</c> selector.
        /// </summary>
        public static SEL bytes { get; } = SEL.Register("bytes");

        /// <summary>
        /// The <c>cascadeTopLeftFromPoint:</c> selector.
        /// </summary>
        public static SEL cascadeTopLeftFromPoint_ { get; } = SEL.Register("cascadeTopLeftFromPoint:");

        /// <summary>
        /// The <c>center</c> selector.
        /// </summary>
        public static SEL center { get; } = SEL.Register("center");

        /// <summary>
        /// The <c>close</c> selector.
        /// </summary>
        public static SEL close { get; } = SEL.Register("close");

        /// <summary>
        /// The <c>containsObject:</c> selector.
        /// </summary>
        public static SEL containsObject_ { get; } = SEL.Register("containsObject:");

        /// <summary>
        /// The <c>contentMaxSize</c> selector.
        /// </summary>
        public static SEL contentMaxSize { get; } = SEL.Register("contentMaxSize");

        /// <summary>
        /// The <c>contentMinSize</c> selector.
        /// </summary>
        public static SEL contentMinSize { get; } = SEL.Register("contentMinSize");

        /// <summary>
        /// The <c>contentRectForFrameRect:</c> selector.
        /// </summary>
        public static SEL contentRectForFrameRect_ { get; } = SEL.Register("contentRectForFrameRect:");

        /// <summary>
        /// The <c>contentView</c> selector.
        /// </summary>
        public static SEL contentView { get; } = SEL.Register("contentView");

        /// <summary>
        /// The <c>convertRectFromScreen:</c> selector.
        /// </summary>
        public static SEL convertRectFromScreen_ { get; } = SEL.Register("convertRectFromScreen:");

        /// <summary>
        /// The <c>convertRectToBacking:</c> selector.
        /// </summary>
        public static SEL convertRectToBacking_ { get; } = SEL.Register("convertRectToBacking:");

        /// <summary>
        /// The <c>convertRectToScreen:</c> selector.
        /// </summary>
        public static SEL convertRectToScreen_ { get; } = SEL.Register("convertRectToScreen:");

        /// <summary>
        /// The <c>count</c> selector.
        /// </summary>
        public static SEL count { get; } = SEL.Register("count");

        /// <summary>
        /// The <c>crosshairCursor</c> selector.
        /// </summary>
        public static SEL crosshairCursor { get; } = SEL.Register("crosshairCursor");

        /// <summary>
        /// The <c>currentEvent</c> selector.
        /// </summary>
        public static SEL currentEvent { get; } = SEL.Register("currentEvent");

        /// <summary>
        /// The <c>dateWithTimeIntervalSinceNow:</c> selector.
        /// </summary>

        public static SEL dateWithTimeIntervalSinceNow_ { get; } = SEL.Register("dateWithTimeIntervalSinceNow:");

        /// <summary>
        /// The <c>declareTypes:owner:</c> selector.
        /// </summary>
        public static SEL declareTypes_owner_ { get; } = SEL.Register("declareTypes:owner:");

        /// <summary>
        /// The <c>deltaX</c> selector.
        /// </summary>
        public static SEL deltaX { get; } = SEL.Register("deltaX");

        /// <summary>
        /// The <c>deltaY</c> selector.
        /// </summary>
        public static SEL deltaY { get; } = SEL.Register("deltaY");

        /// <summary>
        /// The <c>deminiaturize:</c> selector.
        /// </summary>
        public static SEL deminiaturize_ { get; } = SEL.Register("deminiaturize:");

        /// <summary>
        /// The <c>detachNewThreadSelector:toTarget:withObject:</c> selector.
        /// </summary>
        public static SEL detachNewThreadSelector_toTarget_withObject_ { get; } = SEL.Register("detachNewThreadSelector:toTarget:withObject:");

        /// <summary>
        /// The <c>dictionaryWithObjects:forKeys:count:</c> selector.
        /// </summary>
        public static SEL dictionaryWithObjects_forKeys_count_ { get; } = SEL.Register("dictionaryWithObjects:forKeys:count:");

        /// <summary>
        /// The <c>distantFuture</c> selector.
        /// </summary>
        public static SEL distantFuture { get; } = SEL.Register("distantFuture");

        /// <summary>
        /// The <c>distantPast</c> selector.
        /// </summary>
        public static SEL distantPast { get; } = SEL.Register("distantPast");

        /// <summary>
        /// The <c>draggingPasteboard</c> selector.
        /// </summary>
        public static SEL draggingPasteboard { get; } = SEL.Register("draggingPasteboard");

        /// <summary>
        /// The <c>draggingLocation</c> selector.
        /// </summary>
        public static SEL draggingLocation { get; } = SEL.Register("draggingLocation");

        /// <summary>
        /// The <c>draggingSourceOperationMask</c> selector.
        /// </summary>
        public static SEL draggingSourceOperationMask { get; } = SEL.Register("draggingSourceOperationMask");

        /// <summary>
        /// The <c>frame</c> selector.
        /// </summary>
        public static SEL frame { get; } = SEL.Register("frame");

        /// <summary>
        /// The <c>frameRectForContentRect:</c> selector.
        /// </summary>
        public static SEL frameRectForContentRect_ { get; } = SEL.Register("frameRectForContentRect:");

        /// <summary>
        /// The <c>generalPasteboard</c> selector.
        /// </summary>
        public static SEL generalPasteboard { get; } = SEL.Register("generalPasteboard");

        /// <summary>
        /// The <c>hasPreciseScrollingDeltas</c> selector.
        /// </summary>
        public static SEL hasPreciseScrollingDeltas { get; } = SEL.Register("hasPreciseScrollingDeltas");

        /// <summary>
        /// The <c>hide</c> selector.
        /// </summary>
        public static SEL hide { get; } = SEL.Register("hide");

        /// <summary>
        /// The <c>hide:</c> selector.
        /// </summary>
        public static SEL hide_ { get; } = SEL.Register("hide:");

        /// <summary>
        /// The <c>hideOtherApplications:</c> selector.
        /// </summary>
        public static SEL hideOtherApplications_ { get; } = SEL.Register("hideOtherApplications:");

        /// <summary>
        /// The <c>IBeamCursor</c> selector.
        /// </summary>
        public static SEL IBeamCursor { get; } = SEL.Register("IBeamCursor");

        /// <summary>
        /// The <c>initWithAttributedString:</c> selector.
        /// </summary>
        public static SEL initWithAttributedString_ { get; } = SEL.Register("initWithAttributedString:");

        /// <summary>
        /// The <c>initWithBitmapDataPlanes:pixelsWide:pixelsHigh:bitsPerSample:samplesPerPixel:hasAlpha:isPlanar:colorSpaceName:bitmapFormat:bytesPerRow:bitsPerPixel:</c> selector.
        /// </summary>
        public static SEL
            initWithBitmapDataPlanes_pixelsWide_pixelsHigh_bitsPerSample_samplesPerPixel_hasAlpha_isPlanar_colorSpaceName_bitmapFormat_bytesPerRow_bitsPerPixel_ { get; } =
            SEL.Register("initWithBitmapDataPlanes:pixelsWide:pixelsHigh:bitsPerSample:samplesPerPixel:hasAlpha:isPlanar:colorSpaceName:bitmapFormat:bytesPerRow:bitsPerPixel:");

        /// <summary>
        /// The <c>initWithContentRect:styleMask:backing:defer:</c> selector.
        /// </summary>
        public static SEL initWithContentRect_styleMask_backing_defer_ { get; } = SEL.Register("initWithContentRect:styleMask:backing:defer:");

        /// <summary>
        /// The <c>initWithImage:hotSpot:</c> selector.
        /// </summary>
        public static SEL initWithImage_hotSpot_ { get; } = SEL.Register("initWithImage:hotSpot:");

        /// <summary>
        /// The <c>initWithRect:options:owner:userInfo:</c> selector.
        /// </summary>
        public static SEL initWithRect_options_owner_userInfo_ { get; } = SEL.Register("initWithRect:options:owner:userInfo:");

        /// <summary>
        /// The <c>initWithSize:</c> selector.
        /// </summary>
        public static SEL initWithSize_ { get; } = SEL.Register("initWithSize:");

        /// <summary>
        /// The <c>initWithString:</c> selector.
        /// </summary>
        public static SEL initWithString_ { get; } = SEL.Register("initWithString:");

        /// <summary>
        /// The <c>initWithTitle:</c> selector.
        /// </summary>
        public static SEL initWithTitle_ { get; } = SEL.Register("initWithTitle:");

        /// <summary>
        /// The <c>interpretKeyEvents:</c> selector.
        /// </summary>
        public static SEL interpretKeyEvents_ { get; } = SEL.Register("interpretKeyEvents:");

        /// <summary>
        /// The <c>isKeyWindow</c> selector.
        /// </summary>
        public static SEL isKeyWindow { get; } = SEL.Register("isKeyWindow");

        /// <summary>
        /// The <c>isMiniaturized</c> selector.
        /// </summary>
        public static SEL isMiniaturized { get; } = SEL.Register("isMiniaturized");

        /// <summary>
        /// The <c>isVisible</c> selector.
        /// </summary>
        public static SEL isVisible { get; } = SEL.Register("isVisible");

        /// <summary>
        /// The <c></c> selector.
        /// </summary>
        public static SEL isZoomed { get; } = SEL.Register("isZoomed");

        /// <summary>
        /// The <c>keyCode</c> selector.
        /// </summary>
        public static SEL keyCode { get; } = SEL.Register("keyCode");

        /// <summary>
        /// The <c>keyWindow</c> selector.
        /// </summary>
        public static SEL keyWindow { get; } = SEL.Register("keyWindow");

        /// <summary>
        /// The <c>length</c> selector.
        /// </summary>
        public static SEL length { get; } = SEL.Register("length");

        /// <summary>
        /// The <c>level</c> selector.
        /// </summary>
        public static SEL level { get; } = SEL.Register("level");

        /// <summary>
        /// The <c>locationInWindow</c> selector.
        /// </summary>
        public static SEL locationInWindow { get; } = SEL.Register("locationInWindow");

        /// <summary>
        /// The <c>makeKeyAndOrderFront:</c> selector.
        /// </summary>
        public static SEL makeKeyAndOrderFront_ { get; } = SEL.Register("makeKeyAndOrderFront:");

        /// <summary>
        /// The <c>makeFirstResponder:</c> selector.
        /// </summary>
        public static SEL makeFirstResponder_ { get; } = SEL.Register("makeFirstResponder:");

        /// <summary>
        /// The <c>miniaturize:</c> selector.
        /// </summary>
        public static SEL miniaturize_ { get; } = SEL.Register("miniaturize:");

        /// <summary>
        /// The <c>modifierFlags</c> selector.
        /// </summary>
        public static SEL modifierFlags { get; } = SEL.Register("modifierFlags");

        /// <summary>
        /// The <c>mouse:inRect:</c> selector.
        /// </summary>
        public static SEL mouse_inRect_ { get; } = SEL.Register("mouse:inRect:");

        /// <summary>
        /// The <c>mouseLocation</c> selector.
        /// </summary>
        public static SEL mouseLocation { get; } = SEL.Register("mouseLocation");

        /// <summary>
        /// The <c>mouseLocationOutsideOfEventStream</c> selector.
        /// </summary>
        public static SEL mouseLocationOutsideOfEventStream { get; } = SEL.Register("mouseLocationOutsideOfEventStream");

        /// <summary>
        /// The <c>mutableString</c> selector.
        /// </summary>
        public static SEL mutableString { get; } = SEL.Register("mutableString");

        /// <summary>
        /// The <c>nextEventMatchingMask:untilDate:inMode:dequeue:</c> selector.
        /// </summary>
        public static SEL nextEventMatchingMask_untilDate_inMode_dequeue_ = SEL.Register("nextEventMatchingMask:untilDate:inMode:dequeue:");

        /// <summary>
        /// The <c>nextObject</c> selector.
        /// </summary>
        public static SEL nextObject { get; } = SEL.Register("nextObject");

        /// <summary>
        /// The <c>numberWithBool:</c> selector.
        /// </summary>
        public static SEL numberWithBool_ { get; } = SEL.Register("numberWithBool:");

        /// <summary>
        /// The <c>object</c> selector.
        /// </summary>
        public static SEL @object { get; } = SEL.Register("object");

        /// <summary>
        /// The <c>objectEnumerator</c> selector.
        /// </summary>
        public static SEL objectEnumerator { get; } = SEL.Register("objectEnumerator");

        /// <summary>
        /// The <c>orderOut:</c> selector.
        /// </summary>
        public static SEL orderOut_ { get; } = SEL.Register("orderOut:");

        /// <summary>
        /// The <c>otherEventWithType:location:modifierFlags:timestamp:windowNumber:context:subtype:data1:data2:</c> selector.
        /// </summary>
        public static SEL otherEventWithType_location_modifierFlags_timestamp_windowNumber_context_subtype_data1_data2_ =
            SEL.Register("otherEventWithType:location:modifierFlags:timestamp:windowNumber:context:subtype:data1:data2:");

        /// <summary>
        /// The <c>performMiniaturize:</c> selector.
        /// </summary>
        public static SEL performMiniaturize_ { get; } = SEL.Register("performMiniaturize:");

        /// <summary>
        /// The <c>performZoom:</c> selector.
        /// </summary>
        public static SEL performZoom_ { get; } = SEL.Register("performZoom:");

        /// <summary>
        /// The <c>pressedMouseButtons</c> selector.
        /// </summary>
        public static SEL pressedMouseButtons { get; } = SEL.Register("pressedMouseButtons");

        /// <summary>
        /// The <c>pointingHandCursor</c> selector.
        /// </summary>
        public static SEL pointingHandCursor { get; } = SEL.Register("pointingHandCursor");

        /// <summary>
        /// The <c>postEvent:atStart:</c> selector.
        /// </summary>
        public static SEL postEvent_atStart_ { get; } = SEL.Register("postEvent:atStart:");

        /// <summary>
        /// The <c>processInfo</c> selector.
        /// </summary>
        public static SEL processInfo { get; } = SEL.Register("processInfo");

        /// <summary>
        /// The <c>processName</c> selector.
        /// </summary>
        public static SEL processName { get; } = SEL.Register("processName");

        /// <summary>
        /// The <c>propertyListForType:</c> selector.
        /// </summary>
        public static SEL propertyListForType_ { get; } = SEL.Register("propertyListForType:");

        /// <summary>
        /// The <c>registerDefaults:</c> selector.
        /// </summary>
        public static SEL registerDefaults_ { get; } = SEL.Register("registerDefaults:");

        /// <summary>
        /// The <c>registerForDraggedTypes:</c> selector.
        /// </summary>
        public static SEL registerForDraggedTypes_ { get; } = SEL.Register("registerForDraggedTypes:");

        /// <summary>
        /// The <c>removeTrackingArea:</c> selector.
        /// </summary>
        public static SEL removeTrackingArea_ { get; } = SEL.Register("removeTrackingArea:");

        /// <summary>
        /// The <c>requestUserAttention:</c> selector.
        /// </summary>
        public static SEL requestUserAttention_ { get; } = SEL.Register("requestUserAttention:");

        /// <summary>
        /// The <c>resizeLeftRightCursor</c> selector.
        /// </summary>
        public static SEL resizeLeftRightCursor { get; } = SEL.Register("resizeLeftRightCursor");

        /// <summary>
        /// The <c>resizeUpDownCursor</c> selector.
        /// </summary>
        public static SEL resizeUpDownCursor { get; } = SEL.Register("resizeUpDownCursor");

        /// <summary>
        /// The <c>run</c> selector.
        /// </summary>
        public static SEL run { get; } = SEL.Register("run");

        /// <summary>
        /// The <c>scrollingDeltaX</c> selector.
        /// </summary>
        public static SEL scrollingDeltaX { get; } = SEL.Register("scrollingDeltaX");

        /// <summary>
        /// The <c>scrollingDeltaY</c> selector.
        /// </summary>
        public static SEL scrollingDeltaY { get; } = SEL.Register("scrollingDeltaY");

        /// <summary>
        /// The <c>sendEvent:</c> selector.
        /// </summary>
        public static SEL sendEvent_ { get; } = SEL.Register("sendEvent:");

        /// <summary>
        /// The <c>separatorItem</c> selector.
        /// </summary>
        public static SEL separatorItem { get; } = SEL.Register("separatorItem");

        /// <summary>
        /// The <c>set</c> selector.
        /// </summary>
        public static SEL set { get; } = SEL.Register("set");

        /// <summary>
        /// The <c>setActivationPolicy:</c> selector.
        /// </summary>
        public static SEL setActivationPolicy_ { get; } = SEL.Register("setActivationPolicy:");

        /// <summary>
        /// The <c>setAlphaValue:</c> selector.
        /// </summary>
        public static SEL setAlphaValue_ { get; } = SEL.Register("setAlphaValue:");

        /// <summary>
        /// The <c>setContentMaxSize:</c> selector.
        /// </summary>
        public static SEL setContentMaxSize_ { get; } = SEL.Register("setContentMaxSize:");

        /// <summary>
        /// The <c>setContentMinSize:</c> selector.
        /// </summary>
        public static SEL setContentMinSize_ { get; } = SEL.Register("setContentMinSize:");

        /// <summary>
        /// The <c>setContentSize:</c> selector.
        /// </summary>
        public static SEL setContentSize_ { get; } = SEL.Register("setContentSize:");

        /// <summary>
        /// The <c>setContentView:</c> selector.
        /// </summary>
        public static SEL setContentView_ { get; } = SEL.Register("setContentView:");

        /// <summary>
        /// The <c>setDelegate:</c> selector.
        /// </summary>
        public static SEL setDelegate_ { get; } = SEL.Register("setDelegate:");

        /// <summary>
        /// The <c>setFrameOrigin:</c> selector.
        /// </summary>
        public static SEL setFrameOrigin_ { get; } = SEL.Register("setFrameOrigin:");

        /// <summary>
        /// The <c>setKeyEquivalentModifierMask:</c> selector.
        /// </summary>
        public static SEL setKeyEquivalentModifierMask_ { get; } = SEL.Register("setKeyEquivalentModifierMask:");

        /// <summary>
        /// The <c>setLevel:</c> selector.
        /// </summary>
        public static SEL setLevel_ { get; } = SEL.Register("setLevel:");

        /// <summary>
        /// The <c>setMainMenu:</c> selector.
        /// </summary>
        public static SEL setMainMenu_ { get; } = SEL.Register("setMainMenu:");

        /// <summary>
        /// The <c>setNeedsDisplay:</c> selector.
        /// </summary>
        public static SEL setNeedsDisplay_ { get; } = SEL.Register("setNeedsDisplay:");

        /// <summary>
        /// The <c>setServicesMenu:</c> selector.
        /// </summary>
        public static SEL setServicesMenu_ { get; } = SEL.Register("setServicesMenu:");

        /// <summary>
        /// The <c>setString:</c> selector.
        /// </summary>
        public static SEL setString_ { get; } = SEL.Register("setString:");

        /// <summary>
        /// The <c>setString:forType:</c> selector.
        /// </summary>
        public static SEL setString_forType_ { get; } = SEL.Register("setString:forType:");

        /// <summary>
        /// The <c>setStyleMask:</c> selector.
        /// </summary>
        public static SEL setStyleMask_ { get; } = SEL.Register("setStyleMask:");

        /// <summary>
        /// The <c>setSubmenu:</c> selector.
        /// </summary>
        public static SEL setSubmenu_ { get; } = SEL.Register("setSubmenu:");

        /// <summary>
        /// The <c>setTabbingMode:</c> selector.
        /// </summary>
        public static SEL setTabbingMode_ { get; } = SEL.Register("setTabbingMode:");

        /// <summary>
        /// The <c>setTitle:</c> selector.
        /// </summary>
        public static SEL setTitle_ { get; } = SEL.Register("setTitle:");

        /// <summary>
        /// The <c>setWindowsMenu:</c> selector.
        /// </summary>
        public static SEL setWindowsMenu_ { get; } = SEL.Register("setWindowsMenu:");

        /// <summary>
        /// The <c>sharedApplication</c> selector.
        /// </summary>
        public static SEL sharedApplication { get; } = SEL.Register("sharedApplication");

        /// <summary>
        /// The <c>startNewThread:</c> selector.
        /// </summary>
        public static SEL startNewThread_ { get; } = SEL.Register("startNewThread:");

        /// <summary>
        /// The <c>standardUserDefaults</c> selector.
        /// </summary>
        public static SEL standardUserDefaults { get; } = SEL.Register("standardUserDefaults");

        /// <summary>
        /// The <c>stop:</c> selector.
        /// </summary>
        public static SEL stop_ { get; } = SEL.Register("stop:");

        /// <summary>
        /// The <c>string</c> selector.
        /// </summary>
        public static SEL @string { get; } = SEL.Register("string");

        /// <summary>
        /// The <c>stringForType:</c> selector.
        /// </summary>
        public static SEL stringForType_ { get; } = SEL.Register("stringForType:");

        /// <summary>
        /// The <c>styleMask</c> selector.
        /// </summary>
        public static SEL styleMask { get; } = SEL.Register("styleMask");

        /// <summary>
        /// The <c>terminate:</c> selector.
        /// </summary>
        public static SEL terminate_ { get; } = SEL.Register("terminate:");

        /// <summary>
        /// The <c>timestamp</c> selector.
        /// </summary>
        public static SEL timestamp { get; } = SEL.Register("timestamp");

        /// <summary>
        /// The <c>title</c> selector.
        /// </summary>
        public static SEL title { get; } = SEL.Register("title");

        /// <summary>
        /// The <c>type</c> selector.
        /// </summary>
        public static SEL type { get; } = SEL.Register("type");

        /// <summary>
        /// The <c>types</c> selector.
        /// </summary>
        public static SEL types { get; } = SEL.Register("types");

        /// <summary>
        /// The <c>unhide</c> selector.
        /// </summary>
        public static SEL unhide { get; } = SEL.Register("unhide");

        /// <summary>
        /// The <c>unhideAllApplications:</c> selector.
        /// </summary>
        public static SEL unhideAllApplications_ { get; } = SEL.Register("unhideAllApplications:");

        /// <summary>
        /// The <c>updateTrackingAreas</c> selector.
        /// </summary>
        public static SEL updateTrackingAreas { get; } = SEL.Register("updateTrackingAreas");

        /// <summary>
        /// The <c>window</c> selector.
        /// </summary>
        public static SEL window { get; } = SEL.Register("window");

        /// <summary>
        /// The <c>zoom:</c> selector.
        /// </summary>
        public static SEL zoom_ { get; } = SEL.Register("zoom:");
    }
}
