namespace NStuff.Tessellation
{
    internal interface IEdgeCombinator<TPolygonData, TVertexData>
    {
        TVertexData CombineEdges(double x, double y,
            (TVertexData data, double weight) origin1, (TVertexData data, double weight) destination1,
            (TVertexData data, double weight) origin2, (TVertexData data, double weight) destination2,
            TPolygonData polygonData);
    }
}
