# Windowing Toolkit

This repository provides building blocks to write GUI applications on Windows, macOS, and Linux (X11).
* The **WindowSystem** project provides classes to create main windows and to handle events.
* The **OpenGL.Context** project provides classes to associate OpenGL contexts to windows.

To run an example of window creation:
`dotnet run -p test/ManualTest`

## Acknowledgments

* **OpenGL.Context** and **WindowSystem** projects borrow some ideas from [GLFW](https://github.com/glfw/glfw) and [SDL](https://www.libsdl.org/)
