using NStuff.OpenGL.Context;
using NStuff.RasterGraphics;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace NStuff.WindowSystem.ManualTest
{
    class RotateCubeLauncher
    {
        internal unsafe void Launch()
        {
            Console.WriteLine("Rotate Cube...");

            var image = new RasterImage();
            var namePrefix = typeof(DrawTextureLauncher).Namespace + ".Resources.";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "kitten.png"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException();
                }
                PortableNetworkGraphicsHelper.Load(stream, image);
            }

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext();
            using var window = windowServer.CreateWindow(renderingContext);

            window.BorderStyle = WindowBorderStyle.Sizable;
            window.Title = "Rotate Cube...";
            renderingContext.CurrentWindow = window;

            var gl = new GraphicsLibrary(renderingContext);
            gl.ClearColor(0, 0, 0, 1);

            var program = gl.CreateProgram();
            var vertexShader = OpenGLHelper.LoadShader(gl, ShaderType.Vertex, @"
                    in vec3 position;
                    in vec3 color;
                    in vec2 texCoord;
                    out vec3 fColor;
                    out vec2 fTexCoord;
                    uniform mat4 model;
                    uniform mat4 view;
                    uniform mat4 proj;
                    void main() {
                        gl_Position = proj * view * model * vec4(position, 1.0);
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

            var vertices = new float[] {
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,

                        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,

                        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,

                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,

                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,

                        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                        0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
                    };

            gl.GenBuffers(1, &h);
            uint vertexBuffer = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer);
            OpenGLHelper.BufferData(gl, BufferTarget.Array, vertices, BufferUsage.StaticDraw);

            var stride = sizeof(float) * 8;
            gl.EnableVertexAttribArray(position);
            gl.VertexAttribPointer(position, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);
            gl.EnableVertexAttribArray(color);
            gl.VertexAttribPointer(color, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 3 * sizeof(float));
            gl.EnableVertexAttribArray(texCoord);
            gl.VertexAttribPointer(texCoord, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 6 * sizeof(float));

            gl.GenTextures(1, &h);
            var texture = h;
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, texture);
            OpenGLHelper.TexImage2D(gl, image, TextureTarget2D.Texture2d, 0);

            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapS, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapT, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMinFilter, (int)TextureParameterValue.Linear);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMagFilter, (int)TextureParameterValue.Linear);

            gl.UseProgram(program);

            var tex = OpenGLHelper.GetUniformLocation(gl, program, "tex");
            gl.Uniform1i(tex, 0);

            var view = OpenGLHelper.GetUniformLocation(gl, program, "view");
            var proj = OpenGLHelper.GetUniformLocation(gl, program, "proj");
            var model = OpenGLHelper.GetUniformLocation(gl, program, "model");

            var mat = Matrix4x4.CreateLookAt(new Vector3(1.5f, 1.5f, 1.5f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f));
            OpenGLHelper.UniformMatrix(gl, view, mat);

            mat = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), 1.3f, 1f, 10f);
            OpenGLHelper.UniformMatrix(gl, proj, mat);

            var viewportSize = window.ViewportSize;
            var windowSize = window.Size;

            renderingContext.CurrentWindow = null;


            bool done = false;
            float angle = 0f;
            var drawLock = new object();

            void draw()
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var lastTime = stopWatch.ElapsedMilliseconds / 1e3f;
                var deltaTime = 0.0f;

                //renderingContext.SyncWithVerticalBlank(window, false);
                renderingContext.CurrentWindow = window;
                while (!done)
                {
                    var currentTime = stopWatch.ElapsedMilliseconds / 1e3f;
                    deltaTime = currentTime - lastTime;
                    lastTime = currentTime;

                    gl.Viewport(0, 0, (int)viewportSize.width, (int)viewportSize.height);
                    gl.UseProgram(program);
                    angle += deltaTime;
                    mat = Matrix4x4.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), angle);
                    OpenGLHelper.UniformMatrix(gl, model, mat);

                    gl.Enable(Capability.DepthTest);
                    gl.Clear(Buffers.Color | Buffers.Depth);
                    gl.BindVertexArray(vertexArray);
                    gl.BindBuffer(BufferTarget.Array, vertexBuffer);
                    gl.ActiveTexture(TextureUnit.Texture0);
                    gl.BindTexture(TextureTarget.Texture2d, texture);
                    gl.DrawArrays(DrawMode.Triangles, 0, 36);
                    gl.Disable(Capability.DepthTest);

                    renderingContext.SwapBuffers(window);
                    lock (drawLock)
                    {
                        Monitor.Wait(drawLock, 15);
                    }
                }
                renderingContext.CurrentWindow = null;
            }

            var drawThread = new Thread(() => draw());

            window.Closed += (sender, e) => {
                done = true;
                drawThread.Join();
                renderingContext.CurrentWindow = window;
                uint t = texture;
                gl.DeleteTextures(1, &t);
                gl.DeleteProgram(program);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);
                t = vertexBuffer;
                gl.DeleteBuffers(1, &t);
                t = vertexArray;
                gl.DeleteVertexArrays(1, &t);
            };

            window.Resize += (sender, e) =>
            {
                viewportSize = window.ViewportSize;
                windowSize = window.Size;
                lock (drawLock)
                {
                    Monitor.Pulse(drawLock);
                }
            };

            window.FramebufferResize += (sender, e) =>
            {
                viewportSize = window.ViewportSize;
                lock (drawLock)
                {
                    Monitor.Pulse(drawLock);
                }
            };

            drawThread.Start();

            window.Visible = true;
            while (windowServer.Windows.Count > 0)
            {
                windowServer.ProcessEvents(-1);
            }

            Console.WriteLine("Rotate Cube done.");
        }
    }
}
