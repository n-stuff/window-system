using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Provides a base class to implement a drawing backend with direct access to GPU.
    /// </summary>
    public abstract class DrawingBackendBase
    {
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

    }
}
