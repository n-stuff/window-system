# Windowing Toolkit

This repository provides building blocks to write cross-platform __GUI__ applications in __pure .NET__.

The code only relies on __[.NET Core](https://dotnet.microsoft.com/download) 3.0 64bit__, and on a minimal set of functionalities from those APIs:
* Win32 on __Windows__
* Cocoa on __macOS__
* Xlib on __Linux__
* the __OpenGL__ libraries shipped with those three operating systems.

Native interoperability with these APIs is achieved in C# through __P/Invoke__: the `dotnet` tool is the only one needed to build the libraries.

## Examples

The code is developed on Windows 10 64bit, macOS Catalina, and Ubuntu 18.04 64bit installed in a virtual machine.
Once .NET Core is installed, samples and tests work out-of-the-box.

To run the samples launcher use the following command:
```
dotnet run -p test/WindowSystem.ManualTest
```
Then choose a sample by typing a letter. Samples demontrate how to:
* create windows and handle events
* draw polygons and textures using OpenGL
* animate 3D objects using OpenGL
* create a minecraft-like procedural world and move inside it using OpenGL
* display OpenType font glyphs as images or triangles
* decode a simple SVG file containing paths and render it
* build a simple text area widget from the supplied building blocks

#### OpenGL bindings

A tool is provided (`dotnet run -p build/MakeOpenGLInterop`) that generates C# OpenGL bindings automatically from
[the file](build/MakeOpenGLInterop/gl.xml) used to generate the official C headers, from
[a file](build/MakeOpenGLInterop/gl_override.xml) written for this tool that tries to fix the mess in the first file,
and from [an input file](test/WindowSystem.ManualTest/glinterop.xml) listing the wanted APIs.

## Acknowledgments

* **OpenGL.Context** and **WindowSystem** projects borrow some ideas from [GLFW](https://github.com/glfw/glfw) and [SDL](https://www.libsdl.org/)
* **Tessellation** project is based on the algorithm from [GLU libtess](https://gitlab.freedesktop.org/mesa/glu/tree/master/src/libtess)
* **Geometry** project is approximating Bézier curves using recursive subdivision as described in an article on
antigrain.com (site mostly down).
* The **Typography.Font** OpenType decoding code is inspired by [this public domain C code](https://github.com/nothings/stb/blob/master/stb_truetype.h)
* The font rasterization code of the **Typography.Typesetting** project is inspired by [that article](http://nothings.org/gamedev/rasterize/).
