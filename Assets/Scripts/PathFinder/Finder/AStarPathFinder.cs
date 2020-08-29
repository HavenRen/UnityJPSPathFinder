using UnityEngine;

namespace FT
{
    public class AStarPathFinder : AbsPathFinder
    {
#if UNITY_EDITOR
        public AStarPathFinder(MonoBehaviour mono) : base(mono) { }
#endif

        public AStarPathFinder() : base() { }

        protected override bool Search(PathNode node)
        {
            while (node != null)
            {
                closeList.Add(node);
                node.status = NodeStatus.Close;
#if UNITY_EDITOR
                recorder.AddRecord(node, NodeStatus.Close);
#endif
                if (node == endNode)
                    return true;
                SearchOpenNode(node.neighbor.top, node, DirectionType.Top);
                SearchOpenNode(node.neighbor.right, node, DirectionType.Right);
                SearchOpenNode(node.neighbor.bottom, node, DirectionType.Bottom);
                SearchOpenNode(node.neighbor.left, node, DirectionType.Left);
                SearchOpenNode(node.neighbor.topRight, node, DirectionType.TopRight);
                SearchOpenNode(node.neighbor.bottomRight, node, DirectionType.BottomRight);
                SearchOpenNode(node.neighbor.topLeft, node, DirectionType.TopLeft);
                SearchOpenNode(node.neighbor.bottomLeft, node, DirectionType.BottomLeft);
                node = TryGetOpenNode();
            }
            return false;
        }

        void SearchOpenNode(PathNode toCheck, PathNode fromNode, DirectionType dir = DirectionType.None)
        {
            if (toCheck == null || !toCheck.walkable || toCheck.status == NodeStatus.Close)
                return;
            if (dir == DirectionType.TopRight && (!fromNode.neighbor.top.walkable || !fromNode.neighbor.right.walkable))
                return;
            if (dir == DirectionType.TopLeft && (!fromNode.neighbor.top.walkable || !fromNode.neighbor.left.walkable))
                return;
            if (dir == DirectionType.BottomRight && (!fromNode.neighbor.bottom.walkable || !fromNode.neighbor.right.walkable))
                return;
            if (dir == DirectionType.BottomLeft && (!fromNode.neighbor.bottom.walkable || !fromNode.neighbor.left.walkable))
                return;
            if (toCheck.status == NodeStatus.Open)
            {
                var cost = PathNode.ComputeGForAStar(dir);
                var gTemp = fromNode.g + cost;
                if (gTemp < toCheck.g)
                {
                    toCheck.parent = fromNode;
                    toCheck.g = gTemp;
                    toCheck.f = gTemp + toCheck.h;
                }
#if UNITY_EDITOR
                recorder.AddRecord(toCheck, NodeStatus.Open);
#endif
                openHeap.TryUpAdjust(toCheck);
                return;
            }
            toCheck.parent = fromNode;
            toCheck.g = fromNode.g + PathNode.ComputeGForAStar(dir);
            toCheck.h = PathNode.ComputeH(toCheck, endNode);
            toCheck.f = toCheck.g + toCheck.h;
            toCheck.status = NodeStatus.Open;
            openHeap.Enqueue(toCheck);
#if UNITY_EDITOR
            recorder.AddRecord(toCheck, NodeStatus.Open);
#endif
        }
    }
}
