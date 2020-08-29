using System;
using UnityEngine;

namespace FT
{
    public class PathNode : IComparable<PathNode>
    {
        const int Line = 10;
        const int Tilted = 14;

        public int x;

        public int y;

        public bool walkable;

        public Vector3 location;

        public PathNode parent;

        public int g; // 起点到节点代价

        public int h; // 节点到终点代价 估值

        public int f;

        public Neighbor neighbor;

        public NodeStatus status;

        public DirectionType dir; // 用于跳点搜索 跳点速度方向(父给的方向 + 自身带的方向)

        public PathNode(int x, int y, Vector3 location, bool isWalkable)
        {
            this.x = x;
            this.y = y;
            this.location = location;
            walkable = isWalkable;
            g = 0;
            h = 0;
            f = 0;
            status = NodeStatus.Untest;
            dir = DirectionType.None;
        }

        public void SetNeighbor(Neighbor neighbor)
        {
            this.neighbor = neighbor;
        }

        public void Refresh(bool isWalkable)
        {
            parent = null;
            walkable = isWalkable;
            g = 0;
            h = 0;
            f = 0;
            status = NodeStatus.Untest;
            dir = DirectionType.None;
        }

        public void Clear()
        {
            parent = null;
            g = 0;
            h = 0;
            f = 0;
            status = NodeStatus.Untest;
            dir = DirectionType.None;
        }

        public int CompareTo(PathNode refrence)
        {
            return f.CompareTo(refrence.f);
        }

        public static int ComputeH(PathNode ori, PathNode dest)
        {
            var xDelta = dest.x > ori.x ? dest.x - ori.x : ori.x - dest.x;
            var yDelta = dest.y > ori.y ? dest.y - ori.y : ori.y - dest.y;
            return (xDelta + yDelta) * 10;
        }

        /// <summary>
        /// a*的g值计算方式
        /// </summary>
        public static int ComputeGForAStar(DirectionType direction)
        {
            switch (direction)
            {
                case DirectionType.Bottom:
                case DirectionType.Top:
                case DirectionType.Left:
                case DirectionType.Right:
                    return Line;
                default:
                    return Tilted;
            }
        }

        /// <summary>
        /// jps的g值计算方式
        /// </summary>
        public static int ComputeGForJPS(DirectionType direction, PathNode ori, PathNode dest)
        {
            int xDelta, yDelta;
            switch (direction)
            {
                case DirectionType.Bottom:
                case DirectionType.Top:
                    yDelta = dest.y > ori.y ? dest.y - ori.y : ori.y - dest.y;
                    return yDelta * Line;
                case DirectionType.Left:
                case DirectionType.Right:
                    xDelta = dest.x > ori.x ? dest.x - ori.x : ori.x - dest.x;
                    return xDelta * Line;
                default:
                    xDelta = dest.x > ori.x ? dest.x - ori.x : ori.x - dest.x;
                    return xDelta * Tilted;
            }
        }
    }
}