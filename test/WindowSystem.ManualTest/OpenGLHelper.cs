using System;
using System.Collections.Generic;
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
