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
        /// <param name="imageHandle">The handle of a image.</param>
        /// <param name="data">A buffer containing the pixels to copy to the image.</param>
        /// <param name="x">The x-coordinate of the top-left corner where the new pixels should be copied to.</param>
        /// <param name="y">The y-coordinate of the top-left corner where the new pixels should be copied to.</param>
        /// <param name="width">The width in pixels of the area to update.</param>
        /// <param name="height">The height in pixels of the area to update.</param>
        public abstract void UpdateImage(ImageHandle imageHandle, byte[] data, int x, int y, int width, int height);

        /// <summary>
        /// Destroys the specified image.
        /// </summary>
        /// <param name="imageHandle">The handle of the image to destroy.</param>
        public abstract void DestroyImage(ImageHandle imageHandle);

        /// <summary>
        /// Creates a new uniform buffer.
        /// </summary>
        /// <param name="uniformType">The kind of uniforms to store in this buffer.</param>
        /// <param name="maxUniformCount">The max number of uniforms this buffer can store.</param>
        /// <returns></returns>
        public abstract UniformBufferHandle CreateUniformBuffer(UniformType uniformType, int maxUniformCount);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="uniformBufferHandle">A uniform buffer handle.</param>
        /// <param name="uniforms">The data to transfer to the buffer.</param>
        /// <param name="uniformOffset">The index of the first uniform to update.</param>
        /// <param name="uniformCount">The number of uniforms to update.</param>
        public abstract void UpdateUniformBuffer(UniformBufferHandle uniformBufferHandle, RgbaColor[] uniforms, int uniformOffset, int uniformCount);

        /// <summary>
        /// Updates a part of the specified buffer.
        /// </summary>
        /// <param name="uniformBufferHandle">A uniform buffer handle.</param>
        /// <param name="uniforms">The data to transfer to the buffer.</param>
        /// <param name="uniformOffset">The index of the first uniform to update.</param>
        /// <param name="uniformCount">The number of uniforms to update.</param>
        public abstract void UpdateUniformBuffer(UniformBufferHandle uniformBufferHandle, AffineTransform[] uniforms,
            int uniformOffset, int uniformCount);

        /// <summary>
        /// Destroys the specified buffer.
        /// </summary>
        /// <param name="uniformBufferHandle">The handle of the buffer to destroy.</param>
        public abstract void DestroyUniformBuffer(UniformBufferHandle uniformBufferHandle);

    }
}
