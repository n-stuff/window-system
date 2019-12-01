using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Provides a base class to implement a drawing backend with direct access to GPU.
    /// </summary>
    public abstract class DrawingBackendBase
    {
        /// <summary>
        /// The scaling to apply to coordinates to get the number of pixels.
        /// </summary>
        public double PixelScaling { get; set; } = 1.0;

        /// <summary>
        /// The size of the windowing system frame containing the drawing area.
        /// </summary>
        public (double width, double height) WindowSize { get; set; }

        /// <summary>
        /// Gets the maximum number of pixels that can be specified for the width and height of a texture.
        /// </summary>
        /// <returns>A number of pixels.</returns>
        public abstract int GetMaxTextureDimension();

        /// <summary>
        /// Creates a new image.
        /// </summary>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="format">The texture format.</param>
        /// <param name="componentType">The size of the pixel components.</param>
        /// <returns>The handle to the new image.</returns>
        public abstract ImageHandle CreateImage(int width, int height, ImageFormat format, ImageComponentType componentType);

        /// <summary>
        /// Updates a part of the image.
        /// </summary>
        /// <param name="handle">The handle of a image.</param>
        /// <param name="data">A buffer containing the pixels to copy to the image.</param>
        /// <param name="x">The x-coordinate of the top-left corner where the new pixels should be copied to.</param>
        /// <param name="y">The y-coordinate of the top-left corner where the new pixels should be copied to.</param>
        /// <param name="width">The width in pixels of the area to update.</param>
        /// <param name="height">The height in pixels of the area to update.</param>
        public abstract void UpdateImage(ImageHandle handle, byte[] data, int x, int y, int width, int height);

        /// <summary>
        /// Destroys the specified image.
        /// </summary>
        /// <param name="imageHandle">The handle of the image to destroy.</param>
        public abstract void DestroyImage(ImageHandle handle);

        /// <summary>
        /// Creates a new uniform buffer.
        /// </summary>
        /// <param name="uniformType">The kind of uniforms to store in this buffer.</param>
        /// <param name="capacity">The max number of uniforms this buffer can store.</param>
        /// <returns></returns>
        public abstract UniformBufferHandle CreateUniformBuffer(UniformType uniformType, int capacity);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="handle">A uniform buffer handle.</param>
        /// <param name="uniforms">The data to transfer to the buffer.</param>
        /// <param name="offset">The index of the first uniform to update.</param>
        /// <param name="count">The number of uniforms to update.</param>
        public abstract void UpdateUniformBuffer(UniformBufferHandle handle, RgbaColor[] uniforms, int offset, int count);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="handle">A uniform buffer handle.</param>
        /// <param name="uniforms">The data to transfer to the buffer.</param>
        /// <param name="offset">The index of the first uniform to update.</param>
        /// <param name="count">The number of uniforms to update.</param>
        public abstract void UpdateUniformBuffer(UniformBufferHandle handle, AffineTransform[] uniforms, int offset, int count);

        /// <summary>
        /// Destroys the specified buffer.
        /// </summary>
        /// <param name="handle">The handle of the buffer to destroy.</param>
        public abstract void DestroyUniformBuffer(UniformBufferHandle handle);

        /// <summary>
        /// Creates a new vertex buffer.
        /// </summary>
        /// <param name="vertexType">The layout of the vertices in the buffer.</param>
        /// <param name="capacity">The maximum number of vertex this buffer can store.</param>
        /// <returns></returns>
        public abstract VertexBufferHandle CreateVertexBuffer(VertexType vertexType, int capacity);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="handle">A vertex buffer handle.</param>
        /// <param name="vertices">The data to transfer to the buffer.</param>
        /// <param name="offset">The index of the first vertex to update.</param>
        /// <param name="count">The number of vertices to update.</param>
        public abstract void UpdateVertexBuffer(VertexBufferHandle handle, PointCoordinates[] vertices, int offset, int count);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="handle">A vertex buffer handle.</param>
        /// <param name="vertices">The data to transfer to the buffer.</param>
        /// <param name="offset">The index of the first vertex to update.</param>
        /// <param name="count">The number of vertices to update.</param>
        public abstract void UpdateVertexBuffer(VertexBufferHandle handle, PointAndImageCoordinates[] vertices, int offset, int count);

        /// <summary>
        /// Destroys the specified vertex buffer.
        /// </summary>
        /// <param name="handle">The handle of the buffer to free.</param>
        public abstract void DestroyVertexBuffer(VertexBufferHandle handle);

        /// <summary>
        /// Creates a new vertex range buffer.
        /// </summary>
        /// <param name="capacity">The maximum number of ranges this buffer can store.</param>
        /// <returns></returns>
        public abstract VertexRangeBufferHandle CreateVertexRangeBuffer(int capacity);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="handle">A vertex range buffer handle.</param>
        /// <param name="vertexRanges">The data to transfer to the buffer.</param>
        /// <param name="offset">The index of the first range to update.</param>
        /// <param name="count">The number of ranges to update.</param>
        public abstract void UpdateVertexRangeBuffer(VertexRangeBufferHandle handle, VertexRange[] vertexRanges, int offset, int count);

        /// <summary>
        /// Destroys the specified vertex range buffer.
        /// </summary>
        /// <param name="handle">The handle of the buffer to free.</param>
        public abstract void DestroyVertexRangeBuffer(VertexRangeBufferHandle handle);

        /// <summary>
        /// Creates a new command buffer.
        /// </summary>
        /// <returns>The handle of a command buffer.</returns>
        public abstract CommandBufferHandle CreateCommandBuffer();

        /// <summary>
        /// Destroys the provided command buffers.
        /// </summary>
        /// <param name="handle">The handle of the command buffer to destroy.</param>
        public abstract void DestroyCommandBuffer(CommandBufferHandle handle);

        /// <summary>
        /// Places the specified command buffer in a state where it can record commands.
        /// </summary>
        /// <param name="handle">The handle of a command buffer.</param>
        public abstract void BeginRecordCommands(CommandBufferHandle handle);

        /// <summary>
        /// Places the specified command buffer in a state where it can be submitted for execution.
        /// </summary>
        /// <param name="handle">The handle of a command buffer.</param>
        public abstract void EndRecordCommands(CommandBufferHandle handle);

        /// <summary>
        /// Adds a command that binds the specified image, to be used as a texture by drawing commands.
        /// </summary>
        /// <param name="commandBufferHandle">The handle of a command buffer in recording state.</param>
        /// <param name="imageHandle">The handle of the image to bind.</param>
        public abstract void AddBindImageCommand(CommandBufferHandle commandBufferHandle, ImageHandle imageHandle);

        /// <summary>
        /// Binds the specified element of a uniform buffer as the value of the corresponding uniform of the bound shader.
        /// </summary>
        /// <param name="commandBufferHandle">The handle of a command buffer in recording state.</param>
        /// <param name="uniform">The uniform to bind to the uniform buffer.</param>
        /// <param name="uniformBufferHandle">A handle of uniform buffer containing values.</param>
        /// <param name="offset">The index of the element to use as uniform.</param>
        public abstract void AddBindUniformCommand(CommandBufferHandle commandBufferHandle, Uniform uniform,
            UniformBufferHandle uniformBufferHandle, int offset);

        /// <summary>
        /// Adds a command that binds the specified vertex buffer, to be used as a source of vertices by drawing commands.
        /// </summary>
        /// <param name="commandBufferHandle">The handle of a command buffer in recording state.</param>
        /// <param name="vertexBufferHandle">The handle of the buffer to bind.</param>
        public abstract void AddBindVertexBufferCommand(CommandBufferHandle commandBufferHandle, VertexBufferHandle vertexBufferHandle);

        /// <summary>
        /// Adds a command that clears the framebuffer, or the scissor rectangle if one is enabled.
        /// </summary>
        /// <param name="handle">The handle of a command buffer in recording state.</param>
        /// <param name="color">The color to use to clear the area.</param>
        public abstract void AddClearCommand(CommandBufferHandle handle, RgbaColor color);

        /// <summary>
        /// Adds a command that disables the scissor test.
        /// </summary>
        /// <param name="handle">The handle of a command buffer in recording state.</param>
        public abstract void AddDisableScissorTestCommand(CommandBufferHandle handle);

        /// <summary>
        /// Adds a command that draws graphic primitives.
        /// </summary>
        /// <param name="handle">The handle of a command buffer in recording state.</param>
        /// <param name="drawingPrimitive">The kind of graphic primitives to draw.</param>
        /// <param name="vertexOffset">The index of the first vertex to use in the bound vertex buffer.</param>
        /// <param name="vertexCount">The number of vertices to use in the bound vertex buffer.</param>
        public abstract void AddDrawCommand(CommandBufferHandle handle, DrawingPrimitive drawingPrimitive, int vertexOffset, int vertexCount);

        /// <summary>
        /// Adds a command that draws graphic primitives.
        /// </summary>
        /// <param name="commandBufferHandle">The handle of a command buffer in recording state.</param>
        /// <param name="drawingPrimitive">The kind of graphic primitives to draw.</param>
        /// <param name="vertexRangeBufferHandle">The handle of the buffer to use to retrieve the range of vertices of the draw command.</param>
        /// <param name="vertexRangeIndex">The index of a range in the vertex range buffer.</param>
        public abstract void AddDrawIndirectCommand(CommandBufferHandle commandBufferHandle, DrawingPrimitive drawingPrimitive,
            VertexRangeBufferHandle vertexRangeBufferHandle, int vertexRangeIndex);

        /// <summary>
        /// Adds a command that enables the scissor test, and specifies the scissor rectangle.
        /// </summary>
        /// <param name="handle">The handle of a command buffer in recording state.</param>
        /// <param name="x">The x-coordinate of the scissor rectangle.</param>
        /// <param name="y">The y-coordinate of the scissor rectangle.</param>
        /// <param name="width">The width of the scissor rectangle.</param>
        /// <param name="height">The height of the scissor rectangle.</param>
        public abstract void AddScissorCommand(CommandBufferHandle handle, int x, int y, int width, int height);

        /// <summary>
        /// Adds a command that binds the specified shader, to be used by drawing commands.
        /// </summary>
        /// <param name="handle">The handle of a command buffer in recording state.</param>
        /// <param name="shaderKind">The kind of shader to use.</param>
        public abstract void AddUseShaderCommand(CommandBufferHandle handle, ShaderKind shaderKind);

        /// <summary>
        /// Notifies the backend that a new frame will be rendered.
        /// </summary>
        public abstract void BeginRenderFrame();

        /// <summary>
        /// Notifies the backend that all the commands have been submitted to render the current frame.
        /// </summary>
        public abstract void EndRenderFrame();

        /// <summary>
        /// Submits the specified command buffers for execution.
        /// </summary>
        /// <param name="commandBufferHandles">The handles of the command buffers to submit.</param>
        /// <param name="offset">The offset of the first command buffer to submit.</param>
        /// <param name="count">The number of command buffers to submit.</param>
        public abstract void SubmitCommands(CommandBufferHandle[] handles, int offset, int count);
    }
}
