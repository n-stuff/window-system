using NStuff.OpenGL.Context;
using NStuff.RasterGraphics;
using System;
using System.Reflection;

namespace NStuff.WindowSystem.ManualTest
{
    class DrawTextureLauncher
    {
        internal unsafe void Launch()
        {
            Console.WriteLine("Draw Texture...");

            var image = new RasterImage();
            var namePrefix = typeof(DrawTextureLauncher).Namespace + ".Resources.";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "kitten.bmp"))
            {
                image.LoadBmp(stream ?? throw new InvalidOperationException());
            }

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext();
            using var window = windowServer.CreateWindow(renderingContext);

            window.BorderStyle = WindowBorderStyle.Sizable;
            renderingContext.CurrentWindow = window;
            window.MinimumSize = (400, 300);
            window.MaximumSize = (600, 500);
            window.Size = (450, 350);
            window.Opacity = 0.8;

            var gl = new GraphicsLibrary(renderingContext);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            var program = gl.CreateProgram();
            var vertexShader = OpenGLHelper.LoadShader(gl, ShaderType.Vertex, @"
                    in vec2 position;
                    in vec3 color;
                    in vec2 texCoord;
                    out vec3 fColor;
                    out vec2 fTexCoord;
                    void main() {
                        gl_Position = vec4(position, 0.0, 1.0);
                        fColor = color;
                        fTexCoord = texCoord;
                    }
                ");
            gl.AttachShader(program, vertexShader);
            var fragmentShader = OpenGLHelper.LoadShader(gl, ShaderType.Fragment, @"
                    in vec3 fColor;
                    in vec2 fTexCoord;
                    out vec4 outColor;
                    uniform sampler2D tex;
                    void main() {
                        outColor = texture(tex, fTexCoord) * vec4(fColor, 1.0);
                    }
                ");
            gl.AttachShader(program, fragmentShader);

            var position = 0u;
            OpenGLHelper.BindAttribLocation(gl, program, position, "position");
            var color = 1u;
            OpenGLHelper.BindAttribLocation(gl, program, color, "color");
            var texCoord = 2u;
            OpenGLHelper.BindAttribLocation(gl, program, texCoord, "texCoord");

            OpenGLHelper.LinkProgram(gl, program);

            uint h;
            gl.GenVertexArrays(1, &h);
            uint vertexArray = h;
            gl.BindVertexArray(vertexArray);

            float[] vertices = new float[] {
                    -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // Top-left
                    0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, // Top-right
                    0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, // Bottom-right
                    -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f  // Bottom-left
                };
            gl.GenBuffers(1, &h);
            uint vertexBuffer = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer);
            OpenGLHelper.BufferData(gl, BufferTarget.Array, vertices, BufferUsage.StaticDraw);

            var elements = new ushort[] {
                    0, 1, 2,
                    2, 3, 0
                };
            gl.GenBuffers(1, &h);
            uint elementBuffer = h;
            gl.BindBuffer(BufferTarget.ElementArray, elementBuffer);
            OpenGLHelper.BufferData(gl, BufferTarget.ElementArray, elements, BufferUsage.StaticDraw);

            gl.UseProgram(program);
            var stride = sizeof(float) * 7;
            gl.EnableVertexAttribArray(position);
            gl.VertexAttribPointer(position, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);
            gl.EnableVertexAttribArray(color);
            gl.VertexAttribPointer(color, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 2 * sizeof(float));
            gl.EnableVertexAttribArray(texCoord);
            gl.VertexAttribPointer(texCoord, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 5 * sizeof(float));

            gl.GenTextures(1, &h);
            var texture = h;
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, texture);
            OpenGLHelper.TexImage2D(gl, image, TextureTarget2D.Texture2d, 0);

            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapS, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapT, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMinFilter, (int)TextureParameterValue.Linear);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMagFilter, (int)TextureParameterValue.Linear);

            var tex = OpenGLHelper.GetUniformLocation(gl, program, "tex");
            gl.Uniform1i(tex, 0);

            window.Closed += (sender, e) => {
                uint t = texture;
                gl.DeleteTextures(1, &t);
                gl.DeleteProgram(program);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);
                t = vertexBuffer;
                gl.DeleteBuffers(1, &t);
                t = elementBuffer;
                gl.DeleteBuffers(1, &t);
                t = vertexArray;
                gl.DeleteVertexArrays(1, &t);
            };

            void draw()
            {
                var (width, height) = window.ViewportSize;
                gl.Viewport(0, 0, (int)width, (int)height);
                gl.Clear(Buffers.Color);
                gl.DrawElements(DrawMode.Triangles, 6, DrawIndexType.UnsignedShort, IntPtr.Zero);
                renderingContext.SwapBuffers(window);
            }

            window.Resize += (sender, e) => draw();

            window.Visible = true;
            while (windowServer.Windows.Count > 0)
            {
                draw();

                windowServer.ProcessEvents(0.02);
            }

            Console.WriteLine("Draw Texture done.");
        }
    }
}
