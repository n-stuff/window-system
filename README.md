# Windowing Toolkit

This repository provides building blocks to write __GUI__ applications.

Because the aim is to continuously reinvent the wheel in __pure .NET__, this code only relies on:
* __.NET Core 3 64bit__
* Win32 API on __Windows__
* Cocoa API on __macOS__
* Xlib API on __Linux__
* the default __OpenGL__ libraries on those three operating systems.

It is developed on Windows 10 64bit, macOS Catalina, and Ubuntu 18.04 64bit installed in a virtual machine.
Once .NET Core 3 is installed, it works out-of-the-box.

## Modules

* The **WindowSystem** project provides classes to create main windows and to handle events.
* The **OpenGL.Context** project provides classes to associate OpenGL contexts to windows.
* The **RasterGraphics** project provides classes to manipulate raster images.

## Examples

To run examples of window creation, event handling, and OpenGL rendering, use the command:
```
dotnet run -p test/WindowSystem.ManualTest
```

## Acknowledgments

* **OpenGL.Context** and **WindowSystem** projects borrow some ideas from [GLFW](https://github.com/glfw/glfw) and [SDL](https://www.libsdl.org/)
