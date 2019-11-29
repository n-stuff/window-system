using System;
using System.Numerics;

namespace NStuff.WindowSystem.ManualTest.Terrain
{
    public sealed class Scene : IDisposable
    {
        private readonly float chunkRadius = (float)Math.Sqrt(3) * 8;
        private readonly GraphicsLibrary gl;
        private readonly ShaderProgram shaderProgram;
        private readonly uint vertexArray;
        private readonly uint vertexBuffer;
        private uint texture;
        private readonly ChunkPool<bool> chunkPool = new ChunkPool<bool>();
        private readonly PerlinNoise perlinNoise = new PerlinNoise(123);
        private readonly Camera camera;
        private readonly int viewUniform;
        private readonly Frustum frustum = new Frustum();

        internal unsafe Scene(GraphicsLibrary graphicsLibrary)
        {
            gl = graphicsLibrary;
            camera = new Camera();
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            shaderProgram = new ShaderProgram(gl,
                @"
                    layout(location = 0) in vec3 vPosition;
                    layout(location = 1) in vec2 vTexCoord;
                    layout(location = 2) in vec3 vNormal;
                    out vec2 fTexCoord;
                    out vec3 fNormal;
                    uniform mat4 view;
                    uniform mat4 proj;
                    void main() {
                        gl_Position = proj * view * vec4(vPosition, 1.0);
                        fTexCoord = vTexCoord;
                        fNormal = vNormal;
                    }
                ",
                @"
                    in vec2 fTexCoord;
                    in vec3 fNormal;
                    out vec4 outColor;
                    uniform sampler2D tex;
                    uniform vec3 lightingDirection;
                    void main() {
                        vec3 norm = normalize(fNormal);
                        float diff = max(dot(norm, normalize(lightingDirection)), 0.0);
                        vec3 diffuse = (diff + vec3(0.7)) * vec3(1.0);
                        outColor = texture(tex, fTexCoord) * vec4(diffuse, 1.0);
                    }
                "
                );
            const uint position = 0;
            const uint texCoord = 1;
            const uint normal = 2;

            uint h;
            gl.GenVertexArrays(1, &h);
            vertexArray = h;
            gl.BindVertexArray(vertexArray);

            gl.GenBuffers(1, &h);
            vertexBuffer = h;
            gl.BindBuffer(BufferTarget.Array, vertexBuffer);
            gl.BufferData(BufferTarget.Array, IntPtr.Zero + 8 * sizeof(float) * 16 * 16 * 16 * 36, IntPtr.Zero, BufferUsage.DynamicDraw);

            var stride = sizeof(float) * 8;
            gl.EnableVertexAttribArray(position);
            gl.VertexAttribPointer(position, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero);
            gl.EnableVertexAttribArray(texCoord);
            gl.VertexAttribPointer(texCoord, 2, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 3 * sizeof(float));
            gl.EnableVertexAttribArray(normal);
            gl.VertexAttribPointer(normal, 3, AttributeElementType.Float, Boolean.False, stride, IntPtr.Zero + 5 * sizeof(float));

            shaderProgram.Use(gl);
            viewUniform = shaderProgram.GetUniformLocation(gl, "view");
            var proj = shaderProgram.GetUniformLocation(gl, "proj");

            var tex = shaderProgram.GetUniformLocation(gl, "tex");
            gl.Uniform1i(tex, 0);

            var lightingDirection = shaderProgram.GetUniformLocation(gl, "lightingDirection");
            gl.Uniform3f(lightingDirection, (float)Math.Cos(Math.PI / 4), 0.3f, (float)Math.Sin(Math.PI / 4));

            var fieldOfView = (float)(Math.PI / 4); // 45 degrees
            var aspectRatio = 1.3f;
            var nearDistance = 1f;
            var farDistance = 200f;
            frustum.SetPerspectiveFieldOfView(fieldOfView, aspectRatio, nearDistance, farDistance);
            var mat = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearDistance, farDistance);
            shaderProgram.UniformMatrix(gl, proj, mat);

            camera.Position = new Vector3(0f, 40f, 0f);
            camera.Yaw = 115;
            camera.Pitch = -20;
            camera.UpdateVectors();
            shaderProgram.UniformMatrix(gl, viewUniform, camera.GetViewMatrix());
            frustum.SetLookAt(camera.Position, camera.Position + camera.Front, camera.Up);

            chunkPool.Height = 2;
            chunkPool.MaxDistance = 7;
        }

        public unsafe void Dispose()
        {
            uint t = texture;
            gl.DeleteTextures(1, &t);
            shaderProgram.Delete(gl);
            t = vertexBuffer;
            gl.DeleteBuffers(1, &t);
            t = vertexArray;
            gl.DeleteVertexArrays(1, &t);
        }

        public unsafe void AddTextureMap(byte[] data, int imageCount)
        {
            uint h;
            gl.GenTextures(1, &h);
            texture = h;
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, texture);

            var textureFormat = TexturePixelFormat.Rgb8;
            var format = PixelFormat.Rgb;
            fixed (byte* p = data)
            {
                gl.TexImage2D(TextureTarget2D.Texture2d, 0, textureFormat, 16, 16 * imageCount, 0, format, PixelType.UnsignedByte, new IntPtr(p));
            }

            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapS, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureWrapT, (int)TextureParameterValue.ClampToEdge);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMinFilter, (int)TextureParameterValue.Nearest);
            gl.TexParameteri(TextureTarget.Texture2d, TextureParameteri.TextureMagFilter, (int)TextureParameterValue.Nearest);
        }

        public void MoveForward(float deltaTime)
        {
            camera.MoveForward(deltaTime);
            Update();
        }

        public void MoveBackward(float deltaTime)
        {
            camera.MoveBackward(deltaTime);
            Update();
        }

        public void MoveLeft(float deltaTime)
        {
            camera.MoveLeft(deltaTime);
            Update();
        }

        public void MoveRight(float deltaTime)
        {
            camera.MoveRight(deltaTime);
            Update();
        }

        public void MoveUp(float deltaTime)
        {
            camera.MoveUp(deltaTime);
            Update();
        }

        public void Rotate(float xOffset, float yOffset)
        {
            camera.Rotate(xOffset, yOffset);
            Update();
        }

        private void Update()
        {
            shaderProgram.Use(gl);
            shaderProgram.UniformMatrix(gl, viewUniform, camera.GetViewMatrix());
            frustum.SetLookAt(camera.Position, camera.Position + camera.Front, camera.Up);
            chunkPool.X = (int)(camera.Position.X / 16);
            chunkPool.Z = (int)(camera.Position.Z / 16);
        }

        public void SetViewport(int x, int y, int width, int height) => gl.Viewport(x, y, width, height);

        public void Render()
        {
            gl.Enable(Capability.DepthTest);
            gl.Clear(Buffers.Color | Buffers.Depth);
            shaderProgram.Use(gl);
            gl.BindVertexArray(vertexArray);
            gl.BindBuffer(BufferTarget.Array, vertexBuffer);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2d, texture);

            chunkPool.Update();
            var count = chunkPool.Uninitialized.Count;
            foreach (var c in chunkPool.Disposed)
            {
                if (count > 0)
                {
                    var (x, y, z) = chunkPool.Uninitialized[count - 1];
                    chunkPool.Uninitialized.RemoveAt(count - 1);
                    chunkPool.AddInsideRange(InitializeChunk(x, y, z, c));
                    count--;
                }
                else
                {
                    break;
                }
            }
            foreach (var (x, y, z) in chunkPool.Uninitialized)
            {
                chunkPool.AddInsideRange(InitializeChunk(x, y, z, new Chunk<bool>()));
            }
            foreach (var c in chunkPool.InsideRange)
            {
                if (frustum.SphereInFrustum(new Vector3(c.X * 16 + 8, c.Y * 16 + 8, c.Z * 16 + 8), chunkRadius) == FrustumIntersection.Outside)
                {
                    continue;
                }
                index = 0;
                int vertexCount = 0;
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 16; k++)
                        {
                            if (c[i, j, k])
                            {
                                vertexCount += GenerateCubeFaces(c, i, j, k, vertexCount);
                            }
                        }
                    }
                }
                vertexCount += FlushVertices(vertexCount, index);
                if (vertexCount > 0)
                {
                    gl.DrawArrays(DrawMode.Triangles, 0, vertexCount);
                }
            }
            gl.Disable(Capability.DepthTest);
        }

        private Chunk<bool> InitializeChunk(int x, int y, int z, Chunk<bool> chunk)
        {
            chunk.SetLocation(x, y, z);

            var range = chunkPool.Height * 16;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    var noise = perlinNoise.OctaveNoise(((x << 4) + i) / 64.0, ((z << 4) + j) / 64.0, 0, 8);
                    var level = (noise * 0.5 + 0.5) * range * 0.6;
                    for (int k = 0; k < 16; k++)
                    {
                        chunk[i, k, j] = ((y << 4) + k) <= level;
                    }
                }
            }
            return chunk;
        }

        private readonly float[] vertices = new float[(16 * 16) * 36];
        private int index;

        private unsafe int GenerateCubeFaces(Chunk<bool> chunk, int cx, int cy, int cz, int vertexCount)
        {
            var x = (chunk.X << 4) + cx;
            var y = (chunk.Y << 4) + cy;
            var z = (chunk.Z << 4) + cz;
            var i = index;
            var v = vertices;
            var yi0 = 0f;
            var yi1 = 0.3333f;
            var yi2 = 0.6666f;
            var yi3 = 1f;
            var result = 0;

            if (cz == 0 || !chunk[cx, cy, cz - 1])
            {
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 1.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = -1f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }

            if (cz == 15 || !chunk[cx, cy, cz + 1])
            {
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 0.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = 0f; v[i++] = 1f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }

            if (cx == 0 || !chunk[cx - 1, cy, cz])
            {
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi1; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi2; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = -1f; v[i++] = 0f; v[i++] = 0f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }

            if (cx == 15 || !chunk[cx + 1, cy, cz])
            {
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi1; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi2; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 1f; v[i++] = 0f; v[i++] = 0f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }

            if (cy == 0 || !chunk[cx, cy - 1, cz])
            {
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 1.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi3; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi3; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z + 0.5f; v[i++] = 0.0f; v[i++] = yi3; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y - 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi2; v[i++] = 0f; v[i++] = -1f; v[i++] = 0f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }

            if (cy == 15 || !chunk[cx, cy + 1, cz])
            {
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi0; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 1.0f; v[i++] = yi0; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                v[i++] = x + 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 1.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z + 0.5f; v[i++] = 0.0f; v[i++] = yi1; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                v[i++] = x - 0.5f; v[i++] = y + 0.5f; v[i++] = z - 0.5f; v[i++] = 0.0f; v[i++] = yi0; v[i++] = 0f; v[i++] = 1f; v[i++] = 0f;
                if (i == v.Length)
                {
                    result = FlushVertices(vertexCount, i);
                    i = 0;
                }
            }
            index = i;
            return result;
        }

        private unsafe int FlushVertices(int vertexCount, int i)
        {
            if (i > 0)
            {
                fixed (float* p = vertices)
                {
                    gl.BufferSubData(BufferTarget.Array, IntPtr.Zero + sizeof(float) * 8 * vertexCount, IntPtr.Zero + sizeof(float) * i, new IntPtr(p));
                }
            }
            return i / 8;
        }
    }
}
