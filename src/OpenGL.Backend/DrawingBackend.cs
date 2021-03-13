using NStuff.GraphicsBackend;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NStuff.OpenGL.Backend
{
    /// <summary>
    /// Provides a drawing backend implemented using OpenGL 3.3 API.
    /// </summary>
    public sealed class DrawingBackend : DrawingBackendBase, IDisposable
    {
        private enum CommandType
        {
            BindImage,
            BindUniform,
            BindVertexBuffer,
            Clear,
            DisableScissorTest,
            Draw,
            DrawIndirect,
            Scissor,
            UseShader
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Command
        {
            [FieldOffset(0)]
            internal CommandType commandType;
            [FieldOffset(4)]
            internal BindImageArgs bindImageArgs;
            [FieldOffset(4)]
            internal BindUniformArgs bindUniformArgs;
            [FieldOffset(4)]
            internal BindVertexBufferArgs bindVertexBufferArgs;
            [FieldOffset(4)]
            internal ClearArgs clearArgs;
            [FieldOffset(4)]
            internal DrawArgs drawArgs;
            [FieldOffset(4)]
            internal DrawIndirectArgs drawIndirectArgs;
            [FieldOffset(4)]
            internal ScissorArgs scissorArgs;
            [FieldOffset(4)]
            internal UseShaderArgs useShaderArgs;
        }

        private struct BindImageArgs
        {
            internal ImageHandle imageHandle;
        }

        private struct BindUniformArgs
        {
            internal Uniform uniform;
            internal UniformBufferHandle uniformBufferHandle;
            internal int offset;
        }

        private struct BindVertexBufferArgs
        {
            internal VertexBufferHandle vertexBufferHandle;
        }

        private struct ClearArgs
        {
            internal RgbaColor color;
        }

        private struct DrawArgs
        {
            internal DrawingPrimitive primitive;
            internal int vertexOffset;
            internal int vertexCount;
        }

        private struct DrawIndirectArgs
        {
            internal DrawingPrimitive primitive;
            internal VertexRangeBufferHandle bufferHandle;
            internal int vertexRangeIndex;
        }

        private struct ScissorArgs
        {
            internal int x;
            internal int y;
            internal int width;
            internal int height;
        }

        private struct UseShaderArgs
        {
            internal ShaderKind shaderKind;
        }

        private enum CommandBufferState
        {
            New,
            Recording,
            Executable
        }

        private GraphicsLibrary? gl;
        private readonly Shader plainColorShader;
        private readonly Shader greyscaleTextureShader;
        private readonly Shader trueColorTextureShader;
        private ShaderKind currentShader;
        private float[] elementBuffer = new float[1024];
        private readonly int maxTextureDimension;

        private readonly Dictionary<IntPtr, PixelFormat> imagePixelFormats =
            new Dictionary<IntPtr, PixelFormat>();
        private readonly CpuBuffers<(UniformType type, float[] buffer)> uniformBuffers =
            new CpuBuffers<(UniformType type, float[] buffer)>();
        private readonly Dictionary<IntPtr, (uint id, VertexType type, int capacity)> vertexBuffers =
            new Dictionary<IntPtr, (uint id, VertexType type, int capacity)>();
        private readonly CpuBuffers<VertexRange[]> vertexRangeBuffers =
            new CpuBuffers<VertexRange[]>();
        private readonly CpuBuffers<(CommandBufferState state, List<Command> commands)> commandBuffers =
            new CpuBuffers<(CommandBufferState state, List<Command> commands)>();

        /// <summary>
        /// Gets a value indicating whether the backend's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public override bool Disposed => gl == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingBackend"/> class using the supplied <paramref name="entryPointLoader"/>.
        /// </summary>
        /// <param name="entryPointLoader">An object used to access OpenGL API.</param>
        public unsafe DrawingBackend(IEntryPointLoader entryPointLoader)
        {
            gl = new GraphicsLibrary(entryPointLoader);
            plainColorShader = new Shader(gl,
            @"
                layout(location = 0) in vec2 vPosition;
                uniform mat4 projection;
                void main() {
                    gl_Position = projection * vec4(vPosition, 0.0, 1.0);
                }
            ",
            @"
                uniform vec4 color;
                out vec4 outColor;
                void main() {
                    outColor = color;
                }
            ",
            useTexture: false);
            greyscaleTextureShader = new Shader(gl,
            @"
                layout(location = 0) in vec2 vPosition;
                layout(location = 1) in vec2 vTexCoord;
                out vec2 fTexCoord;
                uniform mat4 projection;
                void main() {
                    gl_Position = projection * vec4(vPosition, 0.0, 1.0);
                    fTexCoord = vTexCoord;
                }
            ",
            @"
                in vec2 fTexCoord;
                out vec4 outColor;
                uniform sampler2D sampler;
                uniform vec4 color;
                void main() {
                    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(sampler, fTexCoord).r);
                    outColor = color * sampled;
                }
            ",
            useTexture: true);
            trueColorTextureShader = new Shader(gl,
            @"
                layout(location = 0) in vec2 vPosition;
                layout(location = 1) in vec2 vTexCoord;
                out vec2 fTexCoord;
                uniform mat4 projection;
                void main() {
                    gl_Position = projection * vec4(vPosition, 0.0, 1.0);
                    fTexCoord = vTexCoord;
                }
            ",
            @"
                in vec2 fTexCoord;
                out vec4 outColor;
                uniform sampler2D sampler;
                uniform vec4 color;
                void main() {
                    outColor = texture(sampler, fTexCoord) * color;
                }
            ",
            useTexture: true);

            int i;
            gl.GetIntegerv(IntegerParameter.MaxTextureSize, &i);
            maxTextureDimension = i;
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="DrawingBackend"/>.
        /// </summary>
        ~DrawingBackend() => FreeResources();

        /// <summary>
        /// Releases the resources associated with this object. After calling this method, calling the other methods of this object
        /// is throwing an <c>ObjectDisposedException</c>.
        /// </summary>
        public void Dispose()
        {
            FreeResources();
            GC.SuppressFinalize(this);
        }

        private void FreeResources()
        {
            if (gl != null)
            {
                plainColorShader.Delete(gl);
                greyscaleTextureShader.Delete(gl);
                trueColorTextureShader.Delete(gl);
                gl = null;
            }
        }

        public override int GetMaxTextureDimension() => maxTextureDimension;

        public override unsafe ImageHandle CreateImage(int width, int height, ImageFormat format, ImageComponentType componentType)
        {
            CheckIfAlive();
            uint t;
            gl!.GenTextures(1, &t);
            TexturePixelFormat textureFormat;
            PixelFormat pixelFormat;
            switch (format)
            {
                case ImageFormat.TrueColorAlpha:
                    textureFormat = TexturePixelFormat.Rgba8;
                    pixelFormat = PixelFormat.Rgba;
                    break;
                case ImageFormat.GreyscaleAlpha:
                    textureFormat = TexturePixelFormat.R8;
                    pixelFormat = PixelFormat.Red;
                    break;
                default:
                    throw new InvalidOperationException("Unhandled format: " + format);
            }
            imagePixelFormats.Add(new IntPtr(t), pixelFormat);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, t);

            gl.TexImage2D(TextureTarget2D.Texture2d, 0, textureFormat, width, height, 0, pixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapS, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapT, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMinFilter, (int)TextureParameterValue.Linear);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMagFilter, (int)TextureParameterValue.Linear);
            CheckErrors();
            return new ImageHandle(new IntPtr(t));
        }

        public override unsafe void UpdateImage(ImageHandle handle, byte[] data, int x, int y, int width, int height)
        {
            CheckIfAlive();
            gl!.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, (uint)handle.Value);
            gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
            var pixelFormat = imagePixelFormats[handle.Value];
            fixed (byte* p = data)
            {
                gl.TexSubImage2D(TextureTarget2D.Texture2d, 0, x, y, width, height, pixelFormat, PixelType.UnsignedByte, new IntPtr(p));
            }
        }

        public override unsafe void DestroyImage(ImageHandle handle)
        {
            CheckIfAlive();
            uint t = (uint)handle.Value;
            gl!.DeleteTextures(1, &t);
            imagePixelFormats.Remove(handle.Value);
        }

        public override UniformBufferHandle CreateUniformBuffer(UniformType uniformType, int capacity)
        {
            CheckIfAlive();
            var uniformSize = (uniformType == UniformType.RgbaColor) ? 4 : 6;
            var buffer = new float[uniformSize * capacity];
            var handle = uniformBuffers.NewBuffer((uniformType, buffer));
            return new UniformBufferHandle(handle);
        }

        public override void UpdateUniformBuffer(UniformBufferHandle handle, RgbaColor[] uniforms, int offset, int count)
        {
            CheckIfAlive();
            var (type, buffer) = uniformBuffers[handle.Value];
            if (type != UniformType.RgbaColor)
            {
                throw new ArgumentException("Uniform type mismatch: " + type);
            }
            if ((offset + count) * 4 > buffer.Length)
            {
                throw new ArgumentException("Capacity exceeded: " + buffer.Length / 4);
            }
            var n = 4 * offset;
            for (int i = 0; i < count; i++)
            {
                var c = uniforms[i];
                buffer[n++] = c.Red / 255f;
                buffer[n++] = c.Green / 255f;
                buffer[n++] = c.Blue / 255f;
                buffer[n++] = c.Alpha / 255f;
            }
        }

        public override void UpdateUniformBuffer(UniformBufferHandle handle, AffineTransform[] uniforms, int offset, int count)
        {
            CheckIfAlive();
            var (type, buffer) = uniformBuffers[handle.Value];
            if (type != UniformType.AffineTransform)
            {
                throw new ArgumentException("Uniform type mismatch: " + type);
            }
            if ((offset + count) * 6 > buffer.Length)
            {
                throw new ArgumentException("Capacity exceeded: " + buffer.Length / 6);
            }
            var n = 6 * offset;
            for (int i = 0; i < count; i++)
            {
                var t = uniforms[i];
                buffer[n++] = (float)t.M11;
                buffer[n++] = (float)t.M12;
                buffer[n++] = (float)t.M21;
                buffer[n++] = (float)t.M22;
                buffer[n++] = (float)t.M31;
                buffer[n++] = (float)t.M32;
            }
        }

        public override void DestroyUniformBuffer(UniformBufferHandle handle)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            uniformBuffers.Delete(handle.Value);
        }

        public override unsafe VertexBufferHandle CreateVertexBuffer(VertexType vertexType, int capacity)
        {
            CheckIfAlive();
            uint h;
            gl!.GenVertexArrays(1, &h);
            var vertexArray = h;
            gl.BindVertexArray(vertexArray);

            gl.GenBuffers(1, &h);
            var vertexBuffer = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer);
            vertexBuffers.Add(new IntPtr(vertexArray), (vertexBuffer, vertexType, capacity));

            const uint positionIndex = 0;
            const uint texCoordIndex = 1;
            int stride;
            switch (vertexType)
            {
                case VertexType.PointCoordinates:
                    stride = sizeof(float) * 2;
                    gl.EnableVertexAttribArray(positionIndex);
                    gl.VertexAttribPointer(positionIndex, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);
                    break;

                case VertexType.PointAndImageCoordinates:
                    stride = sizeof(float) * 4;
                    gl.EnableVertexAttribArray(positionIndex);
                    gl.VertexAttribPointer(positionIndex, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);
                    gl.EnableVertexAttribArray(texCoordIndex);
                    gl.VertexAttribPointer(texCoordIndex, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 2 * sizeof(float));
                    break;

                default:
                    throw new InvalidOperationException("Unhandled vertex type: " + vertexType);
            }

            gl.BufferData(BufferTarget.Array, IntPtr.Zero + stride * capacity, IntPtr.Zero, BufferUsage.DynamicDraw);
            CheckErrors();
            return new VertexBufferHandle(new IntPtr(vertexArray));
        }

        public override unsafe void UpdateVertexBuffer(VertexBufferHandle handle, PointCoordinates[] vertices, int offset, int count)
        {
            CheckIfAlive();
            var (id, type, capacity) = vertexBuffers[handle.Value];
            if (type != VertexType.PointCoordinates)
            {
                throw new ArgumentException("Vertex type mismatch: " + type);
            }
            if (offset + count > capacity)
            {
                throw new ArgumentException("Capacity exceeded: " + capacity / 2);
            }
            gl!.BindVertexArray((uint)handle.Value);
            gl.BindBuffer(BufferTarget.Array, id);

            const int vertexElementCount = 2;
            var elementCount = vertexElementCount * count;
            EnsureElementBufferCapacity(elementCount);
            var n = 0;
            for (int i = 0; i < count; i++)
            {
                var v = vertices[i];
                elementBuffer[n++] = (float)v.X;
                elementBuffer[n++] = (float)v.Y;
            }
            fixed (float* p = elementBuffer)
            {
                gl.BufferSubData(BufferTarget.Array, IntPtr.Zero + sizeof(float) * vertexElementCount * offset,
                    IntPtr.Zero + sizeof(float) * elementCount, new IntPtr(p));
            }
        }

        public override unsafe void UpdateVertexBuffer(VertexBufferHandle handle, PointAndImageCoordinates[] vertices, int offset, int count)
        {
            CheckIfAlive();
            var (id, type, capacity) = vertexBuffers[handle.Value];
            if (type != VertexType.PointAndImageCoordinates)
            {
                throw new ArgumentException("Vertex type mismatch: " + type);
            }
            if (offset + count > capacity)
            {
                throw new ArgumentException("Capacity exceeded: " + capacity / 4);
            }
            gl!.BindVertexArray((uint)handle.Value);
            gl.BindBuffer(BufferTarget.Array, id);

            const int vertexElementCount = 4;
            var elementCount = vertexElementCount * count;
            EnsureElementBufferCapacity(elementCount);
            var n = 0;
            for (int i = 0; i < count; i++)
            {
                var v = vertices[i];
                elementBuffer[n++] = (float)v.X;
                elementBuffer[n++] = (float)v.Y;
                elementBuffer[n++] = (float)v.XImage;
                elementBuffer[n++] = (float)v.YImage;
            }
            fixed (float* p = elementBuffer)
            {
                gl.BufferSubData(BufferTarget.Array, IntPtr.Zero + sizeof(float) * vertexElementCount * offset,
                    IntPtr.Zero + sizeof(float) * elementCount, new IntPtr(p));
            }
        }

        public override unsafe void DestroyVertexBuffer(VertexBufferHandle handle)
        {
            CheckIfAlive();
            var (id, type, capacity) = vertexBuffers[handle.Value];
            gl!.DeleteBuffers(1, &id);
            var h = (uint)handle.Value;
            gl.DeleteVertexArrays(1, &h);
            vertexBuffers.Remove(handle.Value);
        }

        public override VertexRangeBufferHandle CreateVertexRangeBuffer(int capacity)
        {
            CheckIfAlive();
            var handle = vertexRangeBuffers.NewBuffer(new VertexRange[capacity]);
            return new VertexRangeBufferHandle(handle);
        }

        public override void UpdateVertexRangeBuffer(VertexRangeBufferHandle handle, VertexRange[] vertexRanges, int offset, int count)
        {
            CheckIfAlive();
            var buffer = vertexRangeBuffers[handle.Value];
            if (buffer == null)
            {
                throw new NullReferenceException();
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Capacity exceeded: " + buffer.Length);
            }
            var n = offset;
            for (int i = 0; i < count; i++)
            {
                buffer[n++] = vertexRanges[i];
            }
        }

        public override void DestroyVertexRangeBuffer(VertexRangeBufferHandle handle)
        {
            CheckIfAlive();
            vertexRangeBuffers.Delete(handle.Value);
        }

        public override CommandBufferHandle CreateCommandBuffer()
        {
            CheckIfAlive();
            var handle = commandBuffers.NewBuffer((CommandBufferState.New, new List<Command>()));
            return new CommandBufferHandle(handle);
        }

        public override void DestroyCommandBuffer(CommandBufferHandle handle)
        {
            CheckIfAlive();
            commandBuffers.Delete(handle.Value);
        }

        public override void BeginRecordCommands(CommandBufferHandle handle)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.New)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commandBuffers[handle.Value] = (CommandBufferState.Recording, commands);
        }

        public override void EndRecordCommands(CommandBufferHandle handle)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commandBuffers[handle.Value] = (CommandBufferState.Executable, commands);
        }


        public override void AddBindImageCommand(CommandBufferHandle commandBufferHandle, ImageHandle imageHandle)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[commandBufferHandle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.BindImage,
                bindImageArgs = new BindImageArgs { imageHandle = imageHandle }
            });
        }

        public override void AddBindUniformCommand(CommandBufferHandle commandBufferHandle, Uniform uniform,
            UniformBufferHandle uniformBufferHandle, int offset)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[commandBufferHandle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.BindUniform,
                bindUniformArgs = new BindUniformArgs
                {
                    uniform = uniform,
                    uniformBufferHandle = uniformBufferHandle,
                    offset = offset
                }
            });
        }

        public override void AddBindVertexBufferCommand(CommandBufferHandle commandBufferHandle, VertexBufferHandle vertexBufferHandle)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[commandBufferHandle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.BindVertexBuffer,
                bindVertexBufferArgs = new BindVertexBufferArgs { vertexBufferHandle = vertexBufferHandle }
            });
        }

        public override void AddClearCommand(CommandBufferHandle handle, RgbaColor color)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.Clear,
                clearArgs = new ClearArgs { color = color }
            });
        }

        public override void AddDisableScissorTestCommand(CommandBufferHandle handle)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.DisableScissorTest
            });
        }

        public override void AddDrawCommand(CommandBufferHandle handle, DrawingPrimitive drawingPrimitive, int vertexOffset, int vertexCount)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.Draw,
                drawArgs = new DrawArgs
                {
                    primitive = drawingPrimitive,
                    vertexOffset = vertexOffset,
                    vertexCount = vertexCount
                }
            });
        }

        public override void AddDrawIndirectCommand(CommandBufferHandle commandBufferHandle, DrawingPrimitive drawingPrimitive,
            VertexRangeBufferHandle vertexRangeBufferHandle, int vertexRangeIndex)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[commandBufferHandle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.DrawIndirect,
                drawIndirectArgs = new DrawIndirectArgs
                {
                    primitive = drawingPrimitive,
                    bufferHandle = vertexRangeBufferHandle,
                    vertexRangeIndex = vertexRangeIndex
                }
            });
        }

        public override void AddScissorCommand(CommandBufferHandle handle, int x, int y, int width, int height)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.Scissor,
                scissorArgs = new ScissorArgs { x = x, y = y, width = width, height = height }
            });
        }

        public override void AddUseShaderCommand(CommandBufferHandle handle, ShaderKind shaderKind)
        {
            CheckIfAlive();
            var (state, commands) = commandBuffers[handle.Value];
            if (state != CommandBufferState.Recording)
            {
                throw new InvalidOperationException("Invalid command buffer state: " + state);
            }
            commands.Add(new Command
            {
                commandType = CommandType.UseShader,
                useShaderArgs = new UseShaderArgs { shaderKind = shaderKind }
            });
        }

        public override void BeginRenderFrame()
        {
            CheckIfAlive();
            gl!.Enable(Capability.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl.Viewport(0, 0, (int)Math.Ceiling(WindowSize.width * PixelScaling), (int)Math.Ceiling(WindowSize.height * PixelScaling));
        }

        public override void EndRenderFrame()
        {
            CheckIfAlive();
            gl!.Disable(Capability.Blend);
            gl.Disable(Capability.ScissorTest);
        }

        public override void SubmitCommands(CommandBufferHandle[] handles, int offset, int count)
        {
            CheckIfAlive();

            for (int i = 0; i < count; i++)
            {
                var (state, commands) = commandBuffers[handles[i + offset].Value];
                if (state != CommandBufferState.Executable)
                {
                    throw new InvalidOperationException("EndRecordCommands() not called.");
                }
                foreach (var c in commands)
                {
                    switch (c.commandType)
                    {
                        case CommandType.BindImage:
                            {
                                gl!.ActiveTexture(TextureUnit.Texture0);
                                gl.BindTexture(TextureTarget.Texture2d, (uint)c.bindImageArgs.imageHandle.Value);
                            }
                            break;

                        case CommandType.BindUniform:
                            {
                                var buffer = uniformBuffers[c.bindUniformArgs.uniformBufferHandle.Value].buffer;
                                switch (c.bindUniformArgs.uniform)
                                {
                                    case Uniform.Color:
                                        {
                                            var first = c.bindUniformArgs.offset * 4;
                                            var red = buffer[first];
                                            var green = buffer[first + 1];
                                            var blue = buffer[first + 2];
                                            var alpha = buffer[first + 3];
                                            switch (currentShader)
                                            {
                                                case ShaderKind.GreyscaleImage:
                                                    greyscaleTextureShader.SetColor(gl!, red, green, blue, alpha);
                                                    break;
                                                case ShaderKind.PlainColor:
                                                    plainColorShader.SetColor(gl!, red, green, blue, alpha);
                                                    break;
                                                case ShaderKind.TrueColorImage:
                                                    trueColorTextureShader.SetColor(gl!, red, green, blue, alpha);
                                                    break;
                                            }
                                        }
                                        break;

                                    case Uniform.Transform:
                                        {
                                            var first = c.bindUniformArgs.offset * 6;
                                            var m11 = buffer[first];
                                            var m12 = buffer[first + 1];
                                            var m21 = buffer[first + 2];
                                            var m22 = buffer[first + 3];
                                            var m31 = buffer[first + 4];
                                            var m32 = buffer[first + 5];
                                            var projection = new Matrix4x4(m11, m12, 0, 0, m21, m22, 0, 0, 0, 0, 1, 0, m31, m32, 0, 1);
                                            projection *= Matrix4x4.CreateOrthographicOffCenter(0, (float)WindowSize.width,
                                                (float)WindowSize.height, 0, -1, 1);
                                            switch (currentShader)
                                            {
                                                case ShaderKind.GreyscaleImage:
                                                    greyscaleTextureShader.SetProjection(gl!, projection);
                                                    break;
                                                case ShaderKind.PlainColor:
                                                    plainColorShader.SetProjection(gl!, projection);
                                                    break;
                                                case ShaderKind.TrueColorImage:
                                                    trueColorTextureShader.SetProjection(gl!, projection);
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;

                        case CommandType.BindVertexBuffer:
                            {
                                var handle = c.bindVertexBufferArgs.vertexBufferHandle;
                                var (id, type, capacity) = vertexBuffers[handle.Value];
                                gl!.BindVertexArray((uint)handle.Value);
                                gl.BindBuffer(BufferTarget.Array, id);
                            }
                            break;

                        case CommandType.Clear:
                            {
                                var color = c.clearArgs.color;
                                gl!.ClearColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);
                                gl.Clear(Buffers.Color);
                            }
                            break;

                        case CommandType.DisableScissorTest:
                            {
                                gl!.Disable(Capability.ScissorTest);
                            }
                            break;

                        case CommandType.Draw:
                            {
                                var drawMode = ConvertPrimitiveType(c.drawArgs.primitive);
                                gl!.DrawArrays(drawMode, c.drawArgs.vertexOffset, c.drawArgs.vertexCount);
                            }
                            break;

                        case CommandType.DrawIndirect:
                            {
                                var drawMode = ConvertPrimitiveType(c.drawIndirectArgs.primitive);
                                var buffer = vertexRangeBuffers[c.drawIndirectArgs.bufferHandle.Value] ?? throw new NullReferenceException();
                                var range = buffer[c.drawIndirectArgs.vertexRangeIndex];
                                gl!.DrawArrays(drawMode, range.Offset, range.Count);
                            }
                            break;

                        case CommandType.Scissor:
                            {
                                gl!.Enable(Capability.ScissorTest);
                                gl.Scissor(
                                    (int)Math.Floor(c.scissorArgs.x * PixelScaling),
                                    (int)Math.Floor((WindowSize.height - c.scissorArgs.y - c.scissorArgs.height) * PixelScaling),
                                    (int)Math.Ceiling(c.scissorArgs.width * PixelScaling),
                                    (int)Math.Ceiling(c.scissorArgs.height * PixelScaling));
                            }
                            break;

                        case CommandType.UseShader:
                            {
                                switch (c.useShaderArgs.shaderKind)
                                {
                                    case ShaderKind.GreyscaleImage:
                                        greyscaleTextureShader.Use(gl!);
                                        break;
                                    case ShaderKind.PlainColor:
                                        plainColorShader.Use(gl!);
                                        break;
                                    case ShaderKind.TrueColorImage:
                                        trueColorTextureShader.Use(gl!);
                                        break;
                                }
                                currentShader = c.useShaderArgs.shaderKind;
                            }
                            break;
                    }
                }
            }
        }

        private static DrawMode ConvertPrimitiveType(DrawingPrimitive primitive)
        {
            return primitive switch
            {
                DrawingPrimitive.LineLoop => DrawMode.LineLoop,
                DrawingPrimitive.LineStrip => DrawMode.LineStrip,
                DrawingPrimitive.TriangleFan => DrawMode.TriangleFan,
                DrawingPrimitive.TriangleStrip => DrawMode.TriangleStrip,
                DrawingPrimitive.Triangles => DrawMode.Triangles,
                _ => throw new InvalidOperationException("Unhandled primitive: " + primitive),
            };
        }

        private void EnsureElementBufferCapacity(int capacity)
        {
            if (capacity > elementBuffer.Length)
            {
                var newLength = elementBuffer.Length * 2;
                while (newLength < capacity)
                {
                    newLength *= 2;
                }
                elementBuffer = new float[newLength];
            }
        }

        private void CheckErrors()
        {
            CheckIfAlive();
            ErrorFlag errorFlag;
            var fatal = false;
            while ((errorFlag = gl!.GetError()) != ErrorFlag.NoError)
            {
                switch (errorFlag)
                {
                    case ErrorFlag.OutOfMemory:
                        fatal = true;
                        break;
                    default:
                        Console.WriteLine("[OpenGL Error] " + errorFlag);
                        break;
                }
            }
            if (fatal)
            {
                throw new OutOfMemoryException();
            }
        }

        private void CheckIfAlive()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
