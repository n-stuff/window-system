namespace NStuff.Tessellation
{
    internal class ActiveRegion<TVertexData>
    {
        internal ActiveRegion<TVertexData> Above { get; set; }
        internal ActiveRegion<TVertexData> Below { get; set; }
        internal HalfEdge<TVertexData> UpperEdge { get; set; }
        internal int Winding { get; set; }
        internal bool Inside { get; set; }
        internal bool Dirty { get; set; }
        internal bool FixUpperEdge { get; set; }

        internal ActiveRegion<TVertexData> TopLeft {
            get {
                var origin = UpperEdge.OriginVertex;
                var result = this;
                do
                {
                    result = result.Above;
                }
                while (result.UpperEdge.OriginVertex == origin);
                return result;
            }
        }

        internal ActiveRegion<TVertexData> TopRight {
            get {
                var destination = UpperEdge.DestinationVertex;
                var result = this;
                do
                {
                    result = result.Above;
                }
                while (result.UpperEdge.DestinationVertex == destination);
                return result;
            }
        }

        internal void Delete()
        {
            Above.Below = Below;
            Below.Above = Above;
        }
    }
}
