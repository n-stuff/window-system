// Generated by 'dotnet run -p build/MakeOpenGLInterop src\OpenGL.Backend\glinterop.xml'
//

namespace NStuff.OpenGL.Backend
{
    partial class GraphicsLibrary
    {
        internal ActiveTextureDelegate ActiveTexture { get; private set; }
        internal AttachShaderDelegate AttachShader { get; private set; }
        internal BindBufferDelegate BindBuffer { get; private set; }
        internal BindTextureDelegate BindTexture { get; private set; }
        internal BindVertexArrayDelegate BindVertexArray { get; private set; }
        internal BlendFuncDelegate BlendFunc { get; private set; }
        internal BufferDataDelegate BufferData { get; private set; }
        internal BufferSubDataDelegate BufferSubData { get; private set; }
        internal ClearDelegate Clear { get; private set; }
        internal ClearColorDelegate ClearColor { get; private set; }
        internal CompileShaderDelegate CompileShader { get; private set; }
        internal CreateProgramDelegate CreateProgram { get; private set; }
        internal CreateShaderDelegate CreateShader { get; private set; }
        internal DeleteBuffersDelegate DeleteBuffers { get; private set; }
        internal DeleteProgramDelegate DeleteProgram { get; private set; }
        internal DeleteShaderDelegate DeleteShader { get; private set; }
        internal DeleteTexturesDelegate DeleteTextures { get; private set; }
        internal DeleteVertexArraysDelegate DeleteVertexArrays { get; private set; }
        internal DisableDelegate Disable { get; private set; }
        internal DrawArraysDelegate DrawArrays { get; private set; }
        internal EnableDelegate Enable { get; private set; }
        internal EnableVertexAttribArrayDelegate EnableVertexAttribArray { get; private set; }
        internal GenBuffersDelegate GenBuffers { get; private set; }
        internal GenTexturesDelegate GenTextures { get; private set; }
        internal GenVertexArraysDelegate GenVertexArrays { get; private set; }
        internal GetErrorDelegate GetError { get; private set; }
        internal GetIntegervDelegate GetIntegerv { get; private set; }
        internal GetProgramInfoLogDelegate GetProgramInfoLog { get; private set; }
        internal GetProgramivDelegate GetProgramiv { get; private set; }
        internal GetShaderInfoLogDelegate GetShaderInfoLog { get; private set; }
        internal GetShaderivDelegate GetShaderiv { get; private set; }
        internal GetStringDelegate GetString { get; private set; }
        internal GetUniformLocationDelegate GetUniformLocation { get; private set; }
        internal LinkProgramDelegate LinkProgram { get; private set; }
        internal PixelStoreiDelegate PixelStorei { get; private set; }
        internal ShaderSourceDelegate ShaderSource { get; private set; }
        internal ScissorDelegate Scissor { get; private set; }
        internal TexImage2DDelegate TexImage2D { get; private set; }
        internal TexSubImage2DDelegate TexSubImage2D { get; private set; }
        internal TexParameteriDelegate TexParameteri { get; private set; }
        internal Uniform1iDelegate Uniform1i { get; private set; }
        internal Uniform4fDelegate Uniform4f { get; private set; }
        internal UniformMatrix4fvDelegate UniformMatrix4fv { get; private set; }
        internal UseProgramDelegate UseProgram { get; private set; }
        internal VertexAttribPointerDelegate VertexAttribPointer { get; private set; }
        internal ViewportDelegate Viewport { get; private set; }

        private void Initialize()
        {
            ActiveTexture = GetOpenGLEntryPoint<ActiveTextureDelegate>("glActiveTexture");
            AttachShader = GetOpenGLEntryPoint<AttachShaderDelegate>("glAttachShader");
            BindBuffer = GetOpenGLEntryPoint<BindBufferDelegate>("glBindBuffer");
            BindTexture = GetOpenGLEntryPoint<BindTextureDelegate>("glBindTexture");
            BindVertexArray = GetOpenGLEntryPoint<BindVertexArrayDelegate>("glBindVertexArray");
            BlendFunc = GetOpenGLEntryPoint<BlendFuncDelegate>("glBlendFunc");
            BufferData = GetOpenGLEntryPoint<BufferDataDelegate>("glBufferData");
            BufferSubData = GetOpenGLEntryPoint<BufferSubDataDelegate>("glBufferSubData");
            Clear = GetOpenGLEntryPoint<ClearDelegate>("glClear");
            ClearColor = GetOpenGLEntryPoint<ClearColorDelegate>("glClearColor");
            CompileShader = GetOpenGLEntryPoint<CompileShaderDelegate>("glCompileShader");
            CreateProgram = GetOpenGLEntryPoint<CreateProgramDelegate>("glCreateProgram");
            CreateShader = GetOpenGLEntryPoint<CreateShaderDelegate>("glCreateShader");
            DeleteBuffers = GetOpenGLEntryPoint<DeleteBuffersDelegate>("glDeleteBuffers");
            DeleteProgram = GetOpenGLEntryPoint<DeleteProgramDelegate>("glDeleteProgram");
            DeleteShader = GetOpenGLEntryPoint<DeleteShaderDelegate>("glDeleteShader");
            DeleteTextures = GetOpenGLEntryPoint<DeleteTexturesDelegate>("glDeleteTextures");
            DeleteVertexArrays = GetOpenGLEntryPoint<DeleteVertexArraysDelegate>("glDeleteVertexArrays");
            Disable = GetOpenGLEntryPoint<DisableDelegate>("glDisable");
            DrawArrays = GetOpenGLEntryPoint<DrawArraysDelegate>("glDrawArrays");
            Enable = GetOpenGLEntryPoint<EnableDelegate>("glEnable");
            EnableVertexAttribArray = GetOpenGLEntryPoint<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            GenBuffers = GetOpenGLEntryPoint<GenBuffersDelegate>("glGenBuffers");
            GenTextures = GetOpenGLEntryPoint<GenTexturesDelegate>("glGenTextures");
            GenVertexArrays = GetOpenGLEntryPoint<GenVertexArraysDelegate>("glGenVertexArrays");
            GetError = GetOpenGLEntryPoint<GetErrorDelegate>("glGetError");
            GetIntegerv = GetOpenGLEntryPoint<GetIntegervDelegate>("glGetIntegerv");
            GetProgramInfoLog = GetOpenGLEntryPoint<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
            GetProgramiv = GetOpenGLEntryPoint<GetProgramivDelegate>("glGetProgramiv");
            GetShaderInfoLog = GetOpenGLEntryPoint<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
            GetShaderiv = GetOpenGLEntryPoint<GetShaderivDelegate>("glGetShaderiv");
            GetString = GetOpenGLEntryPoint<GetStringDelegate>("glGetString");
            GetUniformLocation = GetOpenGLEntryPoint<GetUniformLocationDelegate>("glGetUniformLocation");
            LinkProgram = GetOpenGLEntryPoint<LinkProgramDelegate>("glLinkProgram");
            PixelStorei = GetOpenGLEntryPoint<PixelStoreiDelegate>("glPixelStorei");
            ShaderSource = GetOpenGLEntryPoint<ShaderSourceDelegate>("glShaderSource");
            Scissor = GetOpenGLEntryPoint<ScissorDelegate>("glScissor");
            TexImage2D = GetOpenGLEntryPoint<TexImage2DDelegate>("glTexImage2D");
            TexSubImage2D = GetOpenGLEntryPoint<TexSubImage2DDelegate>("glTexSubImage2D");
            TexParameteri = GetOpenGLEntryPoint<TexParameteriDelegate>("glTexParameteri");
            Uniform1i = GetOpenGLEntryPoint<Uniform1iDelegate>("glUniform1i");
            Uniform4f = GetOpenGLEntryPoint<Uniform4fDelegate>("glUniform4f");
            UniformMatrix4fv = GetOpenGLEntryPoint<UniformMatrix4fvDelegate>("glUniformMatrix4fv");
            UseProgram = GetOpenGLEntryPoint<UseProgramDelegate>("glUseProgram");
            VertexAttribPointer = GetOpenGLEntryPoint<VertexAttribPointerDelegate>("glVertexAttribPointer");
            Viewport = GetOpenGLEntryPoint<ViewportDelegate>("glViewport");
        }
    }
}
