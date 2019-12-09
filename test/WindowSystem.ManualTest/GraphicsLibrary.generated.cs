// Generated by 'dotnet run -p build/MakeOpenGLInterop test\WindowSystem.ManualTest\glinterop.xml'
//

namespace NStuff.WindowSystem.ManualTest
{
    partial class GraphicsLibrary
    {
        private ActiveTextureDelegate glActiveTexture;
        private BindAttribLocationDelegate glBindAttribLocation;
        private BindTextureDelegate glBindTexture;
        private BlendFuncDelegate glBlendFunc;
        private DeleteTexturesDelegate glDeleteTextures;
        private GenTexturesDelegate glGenTextures;
        private GetUniformLocationDelegate glGetUniformLocation;
        private PixelStoreiDelegate glPixelStorei;
        private TexImage2DDelegate glTexImage2D;
        private TexSubImage2DDelegate glTexSubImage2D;
        private TexParameteriDelegate glTexParameteri;
        private Uniform1iDelegate glUniform1i;
        private Uniform3fDelegate glUniform3f;
        private UniformMatrix4fvDelegate glUniformMatrix4fv;

        internal ActiveTextureDelegate ActiveTexture => glActiveTexture ??= GetOpenGLEntryPoint<ActiveTextureDelegate>("glActiveTexture");
        internal AttachShaderDelegate AttachShader { get; private set; }
        internal BindAttribLocationDelegate BindAttribLocation => glBindAttribLocation ??= GetOpenGLEntryPoint<BindAttribLocationDelegate>("glBindAttribLocation");
        internal BindBufferDelegate BindBuffer { get; private set; }
        internal BindTextureDelegate BindTexture => glBindTexture ??= GetOpenGLEntryPoint<BindTextureDelegate>("glBindTexture");
        internal BindVertexArrayDelegate BindVertexArray { get; private set; }
        internal BlendFuncDelegate BlendFunc => glBlendFunc ??= GetOpenGLEntryPoint<BlendFuncDelegate>("glBlendFunc");
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
        internal DeleteTexturesDelegate DeleteTextures => glDeleteTextures ??= GetOpenGLEntryPoint<DeleteTexturesDelegate>("glDeleteTextures");
        internal DeleteVertexArraysDelegate DeleteVertexArrays { get; private set; }
        internal DisableDelegate Disable { get; private set; }
        internal DrawArraysDelegate DrawArrays { get; private set; }
        internal DrawElementsDelegate DrawElements { get; private set; }
        internal EnableDelegate Enable { get; private set; }
        internal EnableVertexAttribArrayDelegate EnableVertexAttribArray { get; private set; }
        internal GenBuffersDelegate GenBuffers { get; private set; }
        internal GenTexturesDelegate GenTextures => glGenTextures ??= GetOpenGLEntryPoint<GenTexturesDelegate>("glGenTextures");
        internal GenVertexArraysDelegate GenVertexArrays { get; private set; }
        internal GetProgramInfoLogDelegate GetProgramInfoLog { get; private set; }
        internal GetProgramivDelegate GetProgramiv { get; private set; }
        internal GetShaderInfoLogDelegate GetShaderInfoLog { get; private set; }
        internal GetShaderivDelegate GetShaderiv { get; private set; }
        internal GetStringDelegate GetString { get; private set; }
        internal GetUniformLocationDelegate GetUniformLocation => glGetUniformLocation ??= GetOpenGLEntryPoint<GetUniformLocationDelegate>("glGetUniformLocation");
        internal LinkProgramDelegate LinkProgram { get; private set; }
        internal PixelStoreiDelegate PixelStorei => glPixelStorei ??= GetOpenGLEntryPoint<PixelStoreiDelegate>("glPixelStorei");
        internal ShaderSourceDelegate ShaderSource { get; private set; }
        internal ScissorDelegate Scissor { get; private set; }
        internal TexImage2DDelegate TexImage2D => glTexImage2D ??= GetOpenGLEntryPoint<TexImage2DDelegate>("glTexImage2D");
        internal TexSubImage2DDelegate TexSubImage2D => glTexSubImage2D ??= GetOpenGLEntryPoint<TexSubImage2DDelegate>("glTexSubImage2D");
        internal TexParameteriDelegate TexParameteri => glTexParameteri ??= GetOpenGLEntryPoint<TexParameteriDelegate>("glTexParameteri");
        internal Uniform1iDelegate Uniform1i => glUniform1i ??= GetOpenGLEntryPoint<Uniform1iDelegate>("glUniform1i");
        internal Uniform3fDelegate Uniform3f => glUniform3f ??= GetOpenGLEntryPoint<Uniform3fDelegate>("glUniform3f");
        internal UniformMatrix4fvDelegate UniformMatrix4fv => glUniformMatrix4fv ??= GetOpenGLEntryPoint<UniformMatrix4fvDelegate>("glUniformMatrix4fv");
        internal UseProgramDelegate UseProgram { get; private set; }
        internal VertexAttribPointerDelegate VertexAttribPointer { get; private set; }
        internal ViewportDelegate Viewport { get; private set; }

        private void Initialize()
        {
            AttachShader = GetOpenGLEntryPoint<AttachShaderDelegate>("glAttachShader");
            BindBuffer = GetOpenGLEntryPoint<BindBufferDelegate>("glBindBuffer");
            BindVertexArray = GetOpenGLEntryPoint<BindVertexArrayDelegate>("glBindVertexArray");
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
            DeleteVertexArrays = GetOpenGLEntryPoint<DeleteVertexArraysDelegate>("glDeleteVertexArrays");
            Disable = GetOpenGLEntryPoint<DisableDelegate>("glDisable");
            DrawArrays = GetOpenGLEntryPoint<DrawArraysDelegate>("glDrawArrays");
            DrawElements = GetOpenGLEntryPoint<DrawElementsDelegate>("glDrawElements");
            Enable = GetOpenGLEntryPoint<EnableDelegate>("glEnable");
            EnableVertexAttribArray = GetOpenGLEntryPoint<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            GenBuffers = GetOpenGLEntryPoint<GenBuffersDelegate>("glGenBuffers");
            GenVertexArrays = GetOpenGLEntryPoint<GenVertexArraysDelegate>("glGenVertexArrays");
            GetProgramInfoLog = GetOpenGLEntryPoint<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
            GetProgramiv = GetOpenGLEntryPoint<GetProgramivDelegate>("glGetProgramiv");
            GetShaderInfoLog = GetOpenGLEntryPoint<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
            GetShaderiv = GetOpenGLEntryPoint<GetShaderivDelegate>("glGetShaderiv");
            GetString = GetOpenGLEntryPoint<GetStringDelegate>("glGetString");
            LinkProgram = GetOpenGLEntryPoint<LinkProgramDelegate>("glLinkProgram");
            ShaderSource = GetOpenGLEntryPoint<ShaderSourceDelegate>("glShaderSource");
            Scissor = GetOpenGLEntryPoint<ScissorDelegate>("glScissor");
            UseProgram = GetOpenGLEntryPoint<UseProgramDelegate>("glUseProgram");
            VertexAttribPointer = GetOpenGLEntryPoint<VertexAttribPointerDelegate>("glVertexAttribPointer");
            Viewport = GetOpenGLEntryPoint<ViewportDelegate>("glViewport");
        }
    }
}
