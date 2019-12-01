# Windowing Toolkit

This repository provides building blocks to write __GUI__ applications.

Because the aim is to continuously reinvent the wheel in __pure .NET__, this code only relies on:
* __.NET Core 3 64bit__
* Win32 API on __Windows__
* Cocoa API on __macOS__
* Xlib API on __Linux__
* default __OpenGL__ libraries on those three operating systems.

Native interoperability with these APIs is achieved through __P/Invoke__.

## Examples

The code is developed on Windows 10 64bit, macOS Catalina, and Ubuntu 18.04 64bit installed in a virtual machine.
Once .NET Core 3 is installed, examples and tests work out-of-the-box.

To run examples of window creation, event handling, and OpenGL rendering, use the command:
```
dotnet run -p test/WindowSystem.ManualTest
```

## Modules

### WindowSystem project

It provides classes to create main windows and to handle events.

### RasterGraphics project

It provides classes to manipulate raster images.

For instance BMP and PNG images can be loaded.

### Tessellation project

It provides types to transform arbitrary polygons into graphics primitives.

### OpenGL.Context project

It provides classes to associate OpenGL contexts to windows.

The rendering relies on OpenGL 3.3/ES because it is available everywhere.
Theoretically the code is structured in a way that allows any library that can render plain color and
textured triangles to be used instead. And even more theoretically, any library that supports GPU command buffers
(such as Vulkan, Direct3D, or Metal) might achieve great performances.

## Acknowledgments

* **OpenGL.Context** and **WindowSystem** projects borrow some ideas from [GLFW](https://github.com/glfw/glfw) and [SDL](https://www.libsdl.org/)
* **Tessellation** project is based on the algorithm from [GLU libtess](https://gitlab.freedesktop.org/mesa/glu/tree/master/src/libtess)
