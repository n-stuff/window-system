// Generated by 'dotnet run -p build/MakeOpenGLInterop src\OpenGL.Backend\glinterop.xml'
//

using System;

using GLboolean = System.Byte;
using GLchar = System.Byte;
using GLfloat = System.Single;
using GLint = System.Int32;
using GLintptr = System.IntPtr;
using GLsizei = System.Int32;
using GLsizeiptr = System.IntPtr;
using GLubyte = System.Byte;
using GLuint = System.UInt32;

namespace NStuff.OpenGL.Backend
{
    internal delegate void ActiveTextureDelegate(TextureUnit texture);
    internal delegate void AttachShaderDelegate(GLuint program, GLuint shader);
    internal delegate void BindBufferDelegate(BufferTarget target, GLuint buffer);
    internal delegate void BindTextureDelegate(TextureTarget target, GLuint texture);
    internal delegate void BindVertexArrayDelegate(GLuint array);
    internal delegate void BlendFuncDelegate(BlendingFactor sfactor, BlendingFactor dfactor);
    internal delegate void BufferDataDelegate(BufferTarget target, GLsizeiptr size, IntPtr data, BufferUsage usage);
    internal delegate void BufferSubDataDelegate(BufferTarget target, GLintptr offset, GLsizeiptr size, IntPtr data);
    internal delegate void ClearDelegate(Buffers mask);
    internal delegate void ClearColorDelegate(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
    internal delegate void CompileShaderDelegate(GLuint shader);
    internal delegate GLuint CreateProgramDelegate();
    internal delegate GLuint CreateShaderDelegate(ShaderType type);
    internal unsafe delegate void DeleteBuffersDelegate(GLsizei n, GLuint* buffers);
    internal delegate void DeleteProgramDelegate(GLuint program);
    internal delegate void DeleteShaderDelegate(GLuint shader);
    internal unsafe delegate void DeleteTexturesDelegate(GLsizei n, GLuint* textures);
    internal unsafe delegate void DeleteVertexArraysDelegate(GLsizei n, GLuint* arrays);
    internal delegate void DisableDelegate(Capability cap);
    internal delegate void DrawArraysDelegate(DrawMode mode, GLint first, GLsizei count);
    internal delegate void EnableDelegate(Capability cap);
    internal delegate void EnableVertexAttribArrayDelegate(GLuint index);
    internal unsafe delegate void GenBuffersDelegate(GLsizei n, GLuint* buffers);
    internal unsafe delegate void GenTexturesDelegate(GLsizei n, GLuint* textures);
    internal unsafe delegate void GenVertexArraysDelegate(GLsizei n, GLuint* arrays);
    internal delegate ErrorFlag GetErrorDelegate();
    internal unsafe delegate void GetIntegervDelegate(IntegerParameter pname, GLint* data);
    internal unsafe delegate void GetProgramInfoLogDelegate(GLuint program, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
    internal unsafe delegate void GetProgramivDelegate(GLuint program, ProgramParameter pname, GLint* @params);
    internal unsafe delegate void GetShaderInfoLogDelegate(GLuint shader, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
    internal unsafe delegate void GetShaderivDelegate(GLuint shader, ShaderParameter pname, GLint* @params);
    internal unsafe delegate GLubyte* GetStringDelegate(StringParameter name);
    internal unsafe delegate GLint GetUniformLocationDelegate(GLuint program, GLchar* name);
    internal delegate void LinkProgramDelegate(GLuint program);
    internal delegate void PixelStoreiDelegate(PixelStoreParameter pname, GLint param);
    internal unsafe delegate void ShaderSourceDelegate(GLuint shader, GLsizei count, GLchar** @string, GLint* length);
    internal delegate void ScissorDelegate(GLint x, GLint y, GLsizei width, GLsizei height);
    internal delegate void TexImage2DDelegate(TextureTarget2D target, GLint level, TexturePixelFormat internalformat, GLsizei width, GLsizei height, GLint border, PixelFormat format, PixelType type, IntPtr pixels);
    internal delegate void TexSubImage2DDelegate(TextureTarget2D target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, PixelFormat format, PixelType type, IntPtr pixels);
    internal delegate void TexParameteriDelegate(TextureTarget target, TextureParameteri pname, GLint param);
    internal delegate void Uniform1iDelegate(GLint location, GLint v0);
    internal delegate void Uniform4fDelegate(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3);
    internal unsafe delegate void UniformMatrix4fvDelegate(GLint location, GLsizei count, Boolean transpose, GLfloat* value);
    internal delegate void UseProgramDelegate(GLuint program);
    internal delegate void VertexAttribPointerDelegate(GLuint index, GLint size, AttributeElementType type, Boolean normalized, GLsizei stride, IntPtr pointer);
    internal delegate void ViewportDelegate(GLint x, GLint y, GLsizei width, GLsizei height);
}
