namespace NStuff.Tessellation
{
    internal class SimpleTessellator2D<TVertexData>
    {
        private int sign;
        private int index;
        private int vertexIndex;

        internal (double x, double y, TVertexData data)[] Vertices { get; } = new (double x, double y, TVertexData data)[128];

        internal int Count { get; set; }

        internal OutputKind OutputKind { get; set; }

        internal (double x, double y, TVertexData data) Vertex { get; set; }

        internal bool AddVertex(double x, double y, TVertexData data)
        {
            if (Count == Vertices.Length)
            {
                return false;
            }
            Vertices[Count++] = (x, y, data);
            return true;
        }

        internal bool Tessellate<TPolygonData>(ITessellateHandler<TPolygonData, TVertexData> handler,
            WindingRule windingRule, TPolygonData polygonData)
        {
            if (Count < 3)
            {
                index = Count;
                return true;
            }
            ComputeSign();
            switch (sign)
            {
                case -2:
                    index = Count;
                    return false;
                case 0:
                    index = Count;
                    return true;
            }
            switch (windingRule)
            {
                case WindingRule.Positive:
                    if (sign < 0)
                    {
                        index = Count;
                        return true;
                    }
                    break;
                case WindingRule.Negative:
                    if (sign > 0)
                    {
                        index = Count;
                        return true;
                    }
                    break;
                case WindingRule.AbsGreaterOrEqual2:
                    index = Count;
                    return true;
            }
            switch (OutputKind)
            {
                case OutputKind.BoundaryEnumerator:
                case OutputKind.TriangleEnumerator:
                    index = 0;
                    vertexIndex = 0;
                    break;

                case OutputKind.TrianglesOnly:
                    {
                        handler.Begin(PrimitiveKind.Triangles, polygonData);
                        var (x, y, data) = Vertices[0];
                        (double x, double y, TVertexData data) v;
                        if (sign > 0)
                        {
                            for (int i = 1; i < Count - 1; i++)
                            {
                                handler.AddVertex(x, y, 0, data);
                                v = Vertices[i];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                                v = Vertices[i + 1];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                            }
                        }
                        else
                        {
                            for (int i = Count - 1; i >= 2; --i)
                            {
                                handler.AddVertex(x, y, 0, data);
                                v = Vertices[i];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                                v = Vertices[i - 1];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                            }
                        }
                        handler.End(polygonData);
                        Count = 0;
                    }
                    break;

                default:
                    {
                        handler.Begin((OutputKind == OutputKind.BoundaryOnly) ?
                            PrimitiveKind.LineLoop : ((Count > 3) ? PrimitiveKind.TriangleFan : PrimitiveKind.Triangles),
                            polygonData);
                        var v = Vertices[0];
                        handler.AddVertex(v.x, v.y, 0, v.data);
                        if (sign > 0)
                        {
                            for (int i = 1; i < Count; i++)
                            {
                                v = Vertices[i];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                            }
                        }
                        else
                        {
                            for (int i = Count - 1; i >= 1; --i)
                            {
                                v = Vertices[i];
                                handler.AddVertex(v.x, v.y, 0, v.data);
                            }
                        }
                        handler.End(polygonData);
                        Count = 0;
                    }
                    break;
            }
            return true;
        }

        internal bool Move()
        {
            if (index >= Count)
            {
                return false;
            }
            if (OutputKind == OutputKind.TriangleEnumerator)
            {
                if (vertexIndex == 0)
                {
                    Vertex = Vertices[0];
                }
                else
                {
                    var i = (sign > 0) ? index + vertexIndex : Count - (index + vertexIndex);
                    Vertex = Vertices[i];
                    if (vertexIndex == 1)
                    {
                        Vertices[i] = default;
                    }
                }
                if (vertexIndex == 2)
                {
                    vertexIndex = 0;
                    index++;
                    if (index == Count - 2)
                    {
                        Vertices[0] = default;
                        index = Count;
                    }
                }
                else
                {
                    vertexIndex++;
                }
            }
            else
            {
                if (index == 0)
                {
                    Vertex = Vertices[0];
                    Vertices[0] = default;
                }
                else
                {
                    var i = (sign > 0) ? index : Count - index;
                    Vertex = Vertices[i];
                    Vertices[i] = default;
                }
                index++;
            }
            return true;
        }

        private void ComputeSign()
        {
            sign = 0;
            var (x0, y0, _) = Vertices[0];
            var (x1, y1, _) = Vertices[1];
            var xc = x1 - x0;
            var yc = y1 - y0;
            double xp;
            double yp;
            for (int i = 2; i < Count; i++)
            {
                xp = xc;
                yp = yc;
                (x1, y1, _) = Vertices[i];
                xc = x1 - x0;
                yc = y1 - y0;
                var dot = xp * yc - yp * xc;
                if (dot > 0)
                {
                    if (sign < 0)
                    {
                        sign = -2;
                        return;
                    }
                    sign = 1;
                }
                else if (dot < 0)
                {
                    if (sign > 0)
                    {
                        sign = -2;
                        return;
                    }
                    sign = -1;
                }
            }
        }
    }
}
