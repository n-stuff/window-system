using System.Text;

using Xunit;
using Xunit.Abstractions;

namespace NStuff.Tessellation.Test
{
    public class TessellatorTest
    {
        private readonly ITestOutputHelper output;

        public TessellatorTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void NoVertex()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("", handler.ToString());
        }

        [Fact]
        public void OneVertexTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("", handler.ToString());
        }

        [Fact]
        public void TwoVerticesTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.AddVertex(-1, 1, 0, 1);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("", handler.ToString());
        }

        [Fact]
        public void ThreeVerticesTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.AddVertex(-1, 1, 0, 1);
            tessellator.AddVertex(-1, -1, 0, 2);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("T{(1,1,0)(-1,1,0)(-1,-1,0)}", handler.ToString());
        }

        [Fact]
        public void SquareTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.AddVertex(-1, 1, 0, 1);
            tessellator.AddVertex(-1, -1, 0, 2);
            tessellator.AddVertex(1, -1, 0, 3);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("F{(1,1,0)(-1,1,0)(-1,-1,0)(1,-1,0)}", handler.ToString());
        }

        [Fact]
        public void SquareTest2()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler)
            {
                OutputKind = OutputKind.TrianglesOnly
            };
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.AddVertex(-1, 1, 0, 1);
            tessellator.AddVertex(-1, -1, 0, 2);
            tessellator.AddVertex(1, -1, 0, 3);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("T{(1,1,0)(-1,1,0)(-1,-1,0)(1,1,0)(-1,-1,0)(1,-1,0)}", handler.ToString());
        }

        [Fact]
        public void SquareHoleTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(1, 1, 0, 0);
            tessellator.AddVertex(-1, 1, 0, 1);
            tessellator.AddVertex(-1, -1, 0, 2);
            tessellator.AddVertex(1, -1, 0, 3);
            tessellator.EndContour();

            tessellator.BeginContour();
            tessellator.AddVertex(.5, .5, 0, 4);
            tessellator.AddVertex(-.5, .5, 0, 5);
            tessellator.AddVertex(-.5, -.5, 0, 6);
            tessellator.AddVertex(.5, -.5, 0, 7);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("S{(-1,-1,0)(-0.5,-0.5,0)(-1,1,0)(-0.5,0.5,0)(1,1,0)(0.5,0.5,0)(1,-1,0)(0.5,-0.5,0)(-0.5,-0.5,0)}"
                + "T{(-0.5,-0.5,0)(-1,-1,0)(1,-1,0)}", handler.ToString());
        }

        [Fact]
        public void StarTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler);
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(0, 3, 0, 0);
            tessellator.AddVertex(-1, 0, 0, 1);
            tessellator.AddVertex(1.6, 1.9, 0, 2);
            tessellator.AddVertex(-1.6, 1.9, 0, 3);
            tessellator.AddVertex(1, 0, 0, 4);
            tessellator.EndContour();

            tessellator.EndPolygon();

            Assert.Equal("T{(-0.366666666666667,1.9,0)(-1.6,1.9,0)(-0.608247422680412,1.17525773195876,0)"
                + "(0,0.730769230769231,0)(-0.608247422680412,1.17525773195876,0)(-1,0,0)"
                + "(1,0,0)(0.608247422680412,1.17525773195876,0)(0,0.730769230769231,0)"
                + "(1.6,1.9,0)(0.366666666666667,1.9,0)(0.608247422680412,1.17525773195876,0)"
                + "(0.366666666666667,1.9,0)(0,3,0)(-0.366666666666667,1.9,0)}", handler.ToString());
        }

        [Fact]
        public void StarBoundaryTest()
        {
            var handler = new Handler();
            var tessellator = new Tessellator3D<int, int>(handler)
            {
                OutputKind = OutputKind.BoundaryOnly
            };
            tessellator.BeginPolygon(0);

            tessellator.BeginContour();
            tessellator.AddVertex(0, 3, 0, 0);
            tessellator.AddVertex(-1, 0, 0, 1);
            tessellator.AddVertex(1.6, 1.9, 0, 2);
            tessellator.AddVertex(-1.6, 1.9, 0, 3);
            tessellator.AddVertex(1, 0, 0, 4);
            tessellator.EndContour();

            tessellator.EndPolygon();

            //output.WriteLine("" + handler);
            Assert.Equal("L{(0.366666666666667,1.9,0)(0,3,0)(-0.366666666666667,1.9,0)}"
                + "L{(1.6,1.9,0)(0.366666666666667,1.9,0)(0.608247422680412,1.17525773195876,0)}"
                + "L{(1,0,0)(0.608247422680412,1.17525773195876,0)(0,0.730769230769231,0)}"
                + "L{(0,0.730769230769231,0)(-0.608247422680412,1.17525773195876,0)(-1,0,0)}"
                + "L{(-0.366666666666667,1.9,0)(-1.6,1.9,0)(-0.608247422680412,1.17525773195876,0)}", handler.ToString());
        }

        private class Handler : ITessellateHandler<int, int>
        {
            private readonly StringBuilder stringBuilder = new StringBuilder();

            public void Begin(PrimitiveKind primitiveKind, int data)
            {
                switch (primitiveKind)
                {
                    case PrimitiveKind.LineLoop:
                        stringBuilder.Append("L");
                        break;
                    case PrimitiveKind.TriangleFan:
                        stringBuilder.Append("F");
                        break;
                    case PrimitiveKind.TriangleStrip:
                        stringBuilder.Append("S");
                        break;
                    case PrimitiveKind.Triangles:
                        stringBuilder.Append("T");
                        break;
                }
                stringBuilder.Append("{");
            }

            public void End(int data)
            {
                stringBuilder.Append("}");
            }

            public void AddVertex(double x, double y, double z, int data)
            {
                stringBuilder.Append("(");
                stringBuilder.Append(x.ToString("G15"));
                stringBuilder.Append(",");
                stringBuilder.Append(y.ToString("G15"));
                stringBuilder.Append(",");
                stringBuilder.Append(z.ToString("G15"));
                stringBuilder.Append(")");
            }

            public int CombineEdges(double x, double y, double z,
                (int data, double weight) origin1, (int data, double weight) destination1,
                (int data, double weight) origin2, (int data, double weight) destination2,
                int polygonData)
            {
                return 0;
            }


            public void FlagEdges(bool onPolygonBoundary)
            {
                stringBuilder.Append((onPolygonBoundary) ? "|" : "*");
            }

            public override string ToString() => stringBuilder.ToString();
        }
    }
}
