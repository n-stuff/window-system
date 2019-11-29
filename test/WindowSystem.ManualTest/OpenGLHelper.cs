using NStuff.RasterGraphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace NStuff.WindowSystem.ManualTest
{
    static class OpenGLHelper
    {
        private static byte[] byteBuffer = new byte[128];

        internal static unsafe uint LoadShader(GraphicsLibrary gl, ShaderType type, string source)
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
                throw new Exception("Shader compilation error");
            }
            return shader;
        }

        internal static unsafe void LinkProgram(GraphicsLibrary gl, uint program)
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
                throw new Exception("Link error");
            }
        }

        internal static unsafe void BindAttribLocation(GraphicsLibrary gl, uint program, uint attribute, string name)
        {
            var byteCount = Encoding.UTF8.GetBytes(name, 0, name.Length, byteBuffer, 0);
            byteBuffer[byteCount] = 0;
            fixed (byte* p = byteBuffer)
            {
                gl.BindAttribLocation(program, attribute, p);
            }
        }

        internal static unsafe void BufferData(GraphicsLibrary gl, BufferTarget target, byte[] data, BufferUsage usage)
        {
            fixed (byte* b = data)
            {
                gl.BufferData(target, new IntPtr(data.Length), new IntPtr(b), usage);
            }
        }

        internal static unsafe void BufferData(GraphicsLibrary gl, BufferTarget target, float[] data, BufferUsage usage)
        {
            fixed (float* b = data)
            {
                gl.BufferData(target, new IntPtr(data.Length * sizeof(float)), new IntPtr(b), usage);
            }
        }

        internal static unsafe void BufferData(GraphicsLibrary gl, BufferTarget target, ushort[] data, BufferUsage usage)
        {
            fixed (ushort* b = data)
            {
                gl.BufferData(target, new IntPtr(data.Length * sizeof(ushort)), new IntPtr(b), usage);
            }
        }

        internal static unsafe int GetUniformLocation(GraphicsLibrary gl, uint program, string name)
        {
            var byteCount = Encoding.UTF8.GetBytes(name, 0, name.Length, byteBuffer, 0);
            byteBuffer[byteCount] = 0;
            fixed (byte* p = byteBuffer)
            {
                return gl.GetUniformLocation(program, p);
            }
        }

        internal static void TexImage2D(GraphicsLibrary gl, RasterImage image, TextureTarget2D target, int level)
        {
            TexturePixelFormat textureFormat;
            PixelFormat format;
            if (image.Format == RasterImageFormat.TrueColor)
            {
                textureFormat = TexturePixelFormat.Rgb8;
                format = PixelFormat.Rgb;
            }
            else
            {
                textureFormat = TexturePixelFormat.Rgba8;
                format = PixelFormat.Rgba;
            }
            unsafe
            {
                fixed (byte* p = image.Data)
                {
                    var (width, height) = image.Size;
                    gl.TexImage2D(target, level, textureFormat, width, height, 0, format, PixelType.UnsignedByte, new IntPtr(p));
                }
            }
        }

        internal static void UniformMatrix(GraphicsLibrary gl, int uniform, Matrix4x4 matrix)
        {
            unsafe
            {
                var t = stackalloc float[16];
                t[0] = matrix.M11; t[1] = matrix.M12; t[2] = matrix.M13; t[3] = matrix.M14;
                t[4] = matrix.M21; t[5] = matrix.M22; t[6] = matrix.M23; t[7] = matrix.M24;
                t[8] = matrix.M31; t[9] = matrix.M32; t[10] = matrix.M33; t[11] = matrix.M34;
                t[12] = matrix.M41; t[13] = matrix.M42; t[14] = matrix.M43; t[15] = matrix.M44;
                gl.UniformMatrix4fv(uniform, 1, Boolean.False, t);
            }
        }

        private static string? shaderHeader;
        private static unsafe string GetShaderHeader(GraphicsLibrary gl)
        {
            if (shaderHeader == null)
            {
                string? s = Marshal.PtrToStringAnsi(new IntPtr(gl.GetString(StringParameter.ShadingLanguageVersion)));
                if (s != null && s.StartsWith("4."))
                {
                    shaderHeader = "#version 4" + s.Substring(2, 2) + " core\n";
                }
                else
                {
                    shaderHeader = "#version 300 es\nprecision mediump float;\n";
                }
            }
            return shaderHeader;
        }
    }
}
