using System;

namespace FT
{
    [Flags]
    public enum DirectionType
    {
        None = 0,
        Top = 1,
        Left = 2,
        Bottom = 4,
        Right = 8,
        TopLeft = 16,
        TopRight = 32,
        BottomLeft = 64,
        BottomRight = 128,
        All = 255,
    }

    public enum NodeStatus
    {
        Untest,
        Open,
        Close,
    }
}