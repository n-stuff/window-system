namespace NStuff.Tessellation
{
    /// <summary>
    /// Lists the rule to be used by the tessellator to determine if a part of the polygon is inside or outside the polygon.
    /// </summary>
    public enum WindingRule
    {
        /// <summary>
        /// A part of the polygon is inside if the winding number is non-zero.
        /// </summary>
        NonZero,

        /// <summary>
        /// A part of the polygon is inside if the winding number is odd.
        /// </summary>
        Odd,
        
        /// <summary>
        /// A part of the polygon is inside if the winding number is positive.
        /// </summary>
        Positive,
        
        /// <summary>
        /// A part of the polygon is inside if the winding number is negative.
        /// </summary>
        Negative,
        
        /// <summary>
        /// A part of the polygon is inside if the absolute value of the winding number is more than two.
        /// </summary>
        AbsGreaterOrEqual2
    }
}
