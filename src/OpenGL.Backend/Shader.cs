using NStuff.GraphicsBackend;
using System.Numerics;

namespace NStuff.OpenGL.Backend
{
    internal struct Shader
    {
        private readonly ShaderProgram program;
        private readonly int ColorLocation;
        private readonly int projectionLocation;

        internal Shader(GraphicsLibrary gl, string vertexShaderSource, string fragmentShaderSource, bool useTexture)
        {
            program = new ShaderProgram(gl, vertexShaderSource, fragmentShaderSource);
            program.Use(gl);
            ColorLocation = program.GetUniformLocation(gl, "color");
            projectionLocation = program.GetUniformLocation(gl, "projection");
            if (useTexture)
            {
                var sampler = program.GetUniformLocation(gl, "sampler");
                program.Uniform1i(gl, sampler, 0);
            }
        }

        internal void Delete(GraphicsLibrary gl) => program.Delete(gl);

        internal void Use(GraphicsLibrary gl) => program.Use(gl);

        internal void SetProjection(GraphicsLibrary gl, Matrix4x4 projection) => program.UniformMatrix(gl, projectionLocation, projection);

        internal void SetColor(GraphicsLibrary gl, RgbaColor color) =>
            program.Uniform4f(gl, ColorLocation, color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);

        internal void SetColor(GraphicsLibrary gl, float red, float green, float blue, float alpha) =>
            program.Uniform4f(gl, ColorLocation, red, green, blue, alpha);
    }
}
