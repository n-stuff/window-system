using NStuff.OpenGL.Context;
using System;
using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.ManualTest
{
    class DrawPolygonLauncher
    {
        internal unsafe void Launch()
        {
            Console.WriteLine("Draw Polygon...");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext();
            using var window = windowServer.CreateWindow(renderingContext);

            renderingContext.CurrentWindow = window;

            var gl = new GraphicsLibrary(renderingContext);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Console.WriteLine("  GL VERSION: " + Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.Version))));
            Console.WriteLine("GLSL VERSION: " + Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.ShadingLanguageVersion))));
            Console.WriteLine("   GL VENDOR: " + Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.Vendor))));
            Console.WriteLine(" GL RENDERER: " + Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.Renderer))));

            var program = gl.CreateProgram();
            var vertexShader = OpenGLHelper.LoadShader(gl, ShaderType.Vertex, @"
                        layout(location = 0) in vec2 vPosition;
                        layout(location = 1) in vec3 vColor;
                        out vec3 fColor;
                        void main() {
                            gl_Position = vec4(vPosition, 0.0, 1.0);
                            fColor = vColor;
                        }
                    ");
            gl.AttachShader(program, vertexShader);
            var fragmentShader = OpenGLHelper.LoadShader(gl, ShaderType.Fragment, @"
                        in vec3 fColor;
                        out vec4 outColor;
                        void main() {
                            outColor = vec4(fColor, 1.0);
                        }
                    ");
            gl.AttachShader(program, fragmentShader);

            const uint position = 0u;
            const uint color = 1u;

            OpenGLHelper.LinkProgram(gl, program);

            uint h;
            gl.GenVertexArrays(1, &h);
            uint vertexArray = h;
            gl.BindVertexArray(vertexArray);
            gl.EnableVertexAttribArray(color);
            gl.EnableVertexAttribArray(position);

            var vertices = new float[] {
                        -0.5f,  0.5f, // Top-left
                        0.5f,  0.5f, // Top-right
                        0.5f, -0.5f, // Bottom-right
                        -0.5f, -0.5f, // Bottom-left
                    };

            gl.GenBuffers(1, &h);
            uint vertexBuffer1 = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer1);
            OpenGLHelper.BufferData(gl, BufferTarget.Array, vertices, BufferUsage.StaticDraw);

            var stride = sizeof(float) * 2;
            gl.VertexAttribPointer(position, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);

            vertices = new float[] {
                        1.0f, 0.0f, 0.0f, // Top-left
                        0.0f, 1.0f, 0.0f, // Top-right
                        0.0f, 0.0f, 1.0f, // Bottom-right
                        1.0f, 1.0f, 1.0f, // Bottom-left
                    };

            gl.GenBuffers(1, &h);
            uint vertexBuffer2 = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer2);
            OpenGLHelper.BufferData(gl, BufferTarget.Array, vertices, BufferUsage.StaticDraw);

            stride = sizeof(float) * 3;
            gl.VertexAttribPointer(color, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);

            var elements = new byte[] {
                        0, 1, 2,
                        2, 3, 0
                    };
            gl.GenBuffers(1, &h);
            uint elementBuffer = h;
            gl.BindBuffer(BufferTarget.ElementArray, elementBuffer);
            OpenGLHelper.BufferData(gl, BufferTarget.ElementArray, elements, BufferUsage.StaticDraw);

            gl.UseProgram(program);

            window.Closed += (sender, e) =>
            {
                gl.DeleteProgram(program);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);
                uint t = vertexBuffer1;
                gl.DeleteBuffers(1, &t);
                t = vertexBuffer2;
                gl.DeleteBuffers(1, &t);
                t = elementBuffer;
                gl.DeleteBuffers(1, &t);
                t = vertexArray;
                gl.DeleteVertexArrays(1, &t);
            };

            window.Visible = true;
            while (windowServer.Windows.Count > 0)
            {
                gl.Clear(Buffers.Color);
                gl.DrawElements(DrawMode.Triangles, 6, DrawIndexType.UnsignedByte, IntPtr.Zero);
                renderingContext.SwapBuffers(window);

                windowServer.ProcessEvents(0.02);
            }

            Console.WriteLine("Draw Polygon done.");
        }
    }
}
