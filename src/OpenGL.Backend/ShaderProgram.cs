using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace NStuff.OpenGL.Backend
{
    internal struct ShaderProgram
    {
        private static string? shaderHeader;
        private static byte[] byteBuffer = new byte[128];
        private readonly uint program;

        internal ShaderProgram(GraphicsLibrary gl, string vertexShaderSource, string fragmentShaderSource)
        {
            program = gl.CreateProgram();
            var vertexShader = CompileShader(gl, ShaderType.Vertex, vertexShaderSource);
            var fragmentShader = CompileShader(gl, ShaderType.Fragment, fragmentShaderSource);
            LinkProgram(gl);
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
        }

        internal void Delete(GraphicsLibrary gl) => gl.DeleteProgram(program);

        internal void Use(GraphicsLibrary gl) => gl.UseProgram(program);

        internal unsafe int GetUniformLocation(GraphicsLibrary gl, string name)
        {
            var byteCount = Encoding.UTF8.GetBytes(name, 0, name.Length, byteBuffer, 0);
            byteBuffer[byteCount] = 0;
            fixed (byte* p = byteBuffer)
            {
                return gl.GetUniformLocation(program, p);
            }
        }

        internal unsafe void UniformMatrix(GraphicsLibrary gl, int uniform, Matrix4x4 matrix)
        {
            var t = stackalloc float[16];
            t[0] = matrix.M11; t[1] = matrix.M12; t[2] = matrix.M13; t[3] = matrix.M14;
            t[4] = matrix.M21; t[5] = matrix.M22; t[6] = matrix.M23; t[7] = matrix.M24;
            t[8] = matrix.M31; t[9] = matrix.M32; t[10] = matrix.M33; t[11] = matrix.M34;
            t[12] = matrix.M41; t[13] = matrix.M42; t[14] = matrix.M43; t[15] = matrix.M44;
            gl.UniformMatrix4fv(uniform, 1, Boolean.False, t);
        }

        internal void Uniform1i(GraphicsLibrary gl, int uniform, int i0) => gl.Uniform1i(uniform, i0);

        internal void Uniform4f(GraphicsLibrary gl, int uniform, float f0, float f1, float f2, float f3) => gl.Uniform4f(uniform, f0, f1, f2, f3);

        private unsafe uint CompileShader(GraphicsLibrary gl, ShaderType type, string source)
        {
            var shader = gl.CreateShader(type);
            var header = GetShaderHeader(gl);
            var byteCount = Encoding.UTF8.GetMaxByteCount(header.Length);
            byteCount += Encoding.UTF8.GetMaxByteCount(source.Length);
            if (byteCount + 1 > byteBuffer.Length)
            {
                byteBuffer = new byte[byteCount + 1];
            }
            byteCount = Encoding.UTF8.GetBytes(header, 0, header.Length, byteBuffer, 0);
            byteCount += Encoding.UTF8.GetBytes(source, 0, source.Length, byteBuffer, byteCount);
            byteBuffer[byteCount] = 0;
            fixed (byte* p = byteBuffer)
            {
                byte* t = p;
                gl.ShaderSource(shader, 1, &t, null);
            }
            gl.CompileShader(shader);
            int len;
            gl.GetShaderiv(shader, ShaderParameter.InfoLogLength, &len);
            if (len > 1)
            {
                var buffer = new byte[len];
                fixed (byte* p = buffer)
                {
                    gl.GetShaderInfoLog(shader, len, &len, p);
                    Console.WriteLine("Shader Compiler message: " + Marshal.PtrToStringAnsi(new IntPtr(p)));
                }
            }
            int cs;
            gl.GetShaderiv(shader, ShaderParameter.CompileStatus, &cs);
            if (cs == 0)
            {
                gl.DeleteShader(shader);
                throw new InvalidOperationException("Shader compilation error");
            }
            gl.AttachShader(program, shader);
            return shader;
        }

        internal unsafe void LinkProgram(GraphicsLibrary gl)
        {
            gl.LinkProgram(program);
            int ls;
            gl.GetProgramiv(program, ProgramParameter.LinkStatus, &ls);
            if (ls == 0)
            {
                int len;
                gl.GetProgramiv(program, ProgramParameter.InfoLogLength, &len);
                if (len > 1)
                {
                    var buffer = new byte[len];
                    fixed (byte* p = buffer)
                    {
                        gl.GetProgramInfoLog(program, len, &len, p);
                        Console.WriteLine("Shader link message: " + Marshal.PtrToStringAnsi(new IntPtr(p)));
                    }
                }
                gl.DeleteProgram(program);
                throw new InvalidOperationException("Shader link error");
            }
        }

        private static unsafe string GetShaderHeader(GraphicsLibrary gl)
        {
            if (shaderHeader == null)
            {
                string? s = Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.ShadingLanguageVersion)));
                if (!string.IsNullOrEmpty(s) && s.StartsWith("4."))
                {
                    shaderHeader = $"#version 4{s.Substring(2, 2)} core\n";
                }
                else
                {
                    shaderHeader =
                        @"#version 300 es
                        #ifdef GL_FRAGMENT_PRECISION_HIGH
                        precision highp float;
                        #else
                        precision mediump float;
                        #endif
                        ";
                }
            }
            return shaderHeader;
        }
    }
}
