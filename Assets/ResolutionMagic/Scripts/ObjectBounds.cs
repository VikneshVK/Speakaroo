using UnityEngine;

namespace ResolutionMagic
{
    public struct ObjectsBounds
    {
        public Vector2 TopLeft;
        public Vector2 BottomRight;
        public Vector2 Centre
        {
            get
            {
                if (OverrideCentre) return CentreFocus;
                return new Vector2((TopLeft.x + BottomRight.x) / 2f, (TopLeft.y + BottomRight.y) / 2f);
            }
        }

        public bool OverrideCentre;
        public Vector2 CentreFocus;

        public override string ToString()
        {
            return $"Bounds: TopLeft: {TopLeft}, BottomRight: {BottomRight}, Centre: {Centre}";
        }

        public void AddMargin(Vector2 margin)
        {
            TopLeft.x -= margin.x;
            TopLeft.y += margin.y;
            BottomRight.x += margin.x;
            BottomRight.y -= margin.y;
        }

        public bool Invalid;
    }
}