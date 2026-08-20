using Blazor.Extensions.Canvas.WebGL;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public class ProgramInfo
    {
        public WebGLProgram Program { get; set; } = null!;
        public UniformLocations UniformLocations { get; set; } = new UniformLocations();
        public AttribLocations AttribLocations { get; set; } = new AttribLocations();
    }

    public class UniformLocations
    {
        public WebGLUniformLocation ProjectionMatrix { get; set; } = null!;
        public WebGLUniformLocation ModelViewMatrix { get; set; } = null!;
    }

    public class AttribLocations
    {
        public int VertexPosition { get; set; }
        public int VertexColor { get; set; }
    }

    public class Buffers
    {
        public WebGLBuffer Position { get; set; } = null!;
        public WebGLBuffer Color { get; set; } = null!;
        public WebGLBuffer Indices { get; set; } = null!;
    }

    public class Camera
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; } = 1.0f;
        public float Rotation { get; set; }
        public float Zoom { get; set; } = 1.0f;
    }
}
