using UnityEngine;

namespace FT
{
    public class JPSPathFinder : AbsPathFinder
    {
        enum JumpType { Line, Tilted }

#if UNITY_EDITOR
        public JPSPathFinder(MonoBehaviour mono) : base(mono) { }
#endif

        public JPSPathFinder() : base() { }

        protected override bool Search(PathNode node)
        {
            node.dir = DirectionType.All;
            while (node != null)
            {
                node.status = NodeStatus.Close;
                closeList.Add(node);
#if UNITY_EDITOR
                recorder.AddRecord(node, NodeStatus.Close);
#endif
                if (node == endNode)
                    return true;
                if ((node.dir & DirectionType.Top) != DirectionType.None)
                    SearchTop(node);
                if ((node.dir & DirectionType.Right) != DirectionType.None)
                    SearchRight(node);
                if ((node.dir & DirectionType.Left) != DirectionType.None)
                    SearchLeft(node);
                if ((node.dir & DirectionType.Bottom) != DirectionType.None)
                    SearchBottom(node);
                if ((node.dir & DirectionType.TopRight) != DirectionType.None)
                    SearchTopRight(node);
                if ((node.dir & DirectionType.TopLeft) != DirectionType.None)
                    SearchTopLeft(node);
                if ((node.dir & DirectionType.BottomRight) != DirectionType.None)
                    SearchBottomRight(node);
                if ((node.dir & DirectionType.BottomLeft) != DirectionType.None)
                    SearchBottomLeft(node);
                node = TryGetOpenNode();
            }
            return false;
        }

        void SearchTop(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.top, fromNode, DirectionType.Top, JumpType.Line);
        }

        void SearchRight(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.right, fromNode, DirectionType.Right, JumpType.Line);
        }

        void SearchLeft(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.left, fromNode, DirectionType.Left, JumpType.Line);
        }

        void SearchBottom(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.bottom, fromNode, DirectionType.Bottom, JumpType.Line);
        }

        void SearchTopRight(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.topRight, fromNode, DirectionType.TopRight, JumpType.Tilted);
        }

        void SearchTopLeft(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.topLeft, fromNode, DirectionType.TopLeft, JumpType.Tilted);
        }

        void SearchBottomRight(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.bottomRight, fromNode, DirectionType.BottomRight, JumpType.Tilted);
        }

        void SearchBottomLeft(PathNode fromNode)
        {
            var node = fromNode;
            while (node != null && node.walkable)
                node = GetNext(node.neighbor.bottomLeft, fromNode, DirectionType.BottomLeft, JumpType.Tilted);
        }

        PathNode GetNext(PathNode toCheck, PathNode fromNode, DirectionType dir, JumpType jumpType)
        {
            if (toCheck == null || toCheck.walkable == false || toCheck.status == NodeStatus.Close)
            {
                return null;
            }

            DirectionType jumpNodeDir;
            var tempValue = jumpType == JumpType.Line ? IsLineJumpNode(toCheck, dir, out jumpNodeDir) : IsTitleJumpNode(toCheck, dir, out jumpNodeDir);
            if (tempValue)  // toCheck是跳点
            {
                if (toCheck.status == NodeStatus.Open) // 已经在openlist里了 判断是否更新G值 更新parent
                {
                    var cost = PathNode.ComputeGForJPS(dir, fromNode, toCheck);
                    var gTemp = fromNode.g + cost;
                    if (gTemp < toCheck.g)
                    {
                        var oldDir = GetDirection(toCheck.parent, toCheck);
                        toCheck.dir = (toCheck.dir ^ oldDir) | jumpNodeDir; // 去掉旧的父亲方向 保留自身方向 添加新的方向 
                        toCheck.parent = fromNode;
                        toCheck.g = gTemp;
                        toCheck.f = gTemp + toCheck.h;
                    }
#if UNITY_EDITOR
                    recorder.AddRecord(toCheck, NodeStatus.Open);
#endif
                    openHeap.TryUpAdjust(toCheck);
                    return null;
                }
                //加入openlist
                toCheck.parent = fromNode;
                toCheck.g = fromNode.g + PathNode.ComputeGForJPS(dir, toCheck, fromNode);
                toCheck.h = PathNode.ComputeH(toCheck, endNode);
                toCheck.f = toCheck.g + toCheck.h;
                toCheck.status = NodeStatus.Open;
                toCheck.dir = jumpNodeDir;
                openHeap.Enqueue(toCheck);
#if UNITY_EDITOR
                recorder.AddRecord(toCheck, NodeStatus.Open);
#endif
                return null;
            }
            return toCheck;
        }

        bool IsLineJumpNode(PathNode toCheck, DirectionType dir, out DirectionType jumpNodeDir)
        {
            jumpNodeDir = dir;
            if (toCheck == null || toCheck.walkable == false)
                return false;
            if (dir == DirectionType.Right)
                return IsRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.Left)
                return IsLeftJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.Top)
                return IsTopJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.Bottom)
                return IsBottomJumpNode(toCheck, ref jumpNodeDir);
            return false;
        }

        bool IsRightJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            if (toCheck == endNode)
                return true;
            var result = false;
            if (toCheck.neighbor.right != null && toCheck.neighbor.right.walkable)
            {
                var up = toCheck.neighbor.top;
                var down = toCheck.neighbor.bottom;
                var topRight = toCheck.neighbor.topRight;
                var bottomRight = toCheck.neighbor.bottomRight;
                if (up != null && !up.walkable && topRight != null && topRight.walkable)
                {
                    jumpNodeDir |= DirectionType.TopRight;
                    result = true;
                }
                if (down != null && !down.walkable && bottomRight != null && bottomRight.walkable)
                {
                    jumpNodeDir |= DirectionType.BottomRight;
                    result = true;
                }
            }
            return result;
        }

        bool IsLeftJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            if (toCheck == endNode)
                return true;
            var result = false;
            if (toCheck.neighbor.left != null && toCheck.neighbor.left.walkable)
            {
                var top = toCheck.neighbor.top;
                var bottom = toCheck.neighbor.bottom;
                var topLeft = toCheck.neighbor.topLeft;
                var bottomLeft = toCheck.neighbor.bottomLeft;
                if (top != null && !top.walkable && topLeft != null && topLeft.walkable)
                {
                    jumpNodeDir |= DirectionType.TopLeft;
                    result = true;
                }
                if (bottom != null && !bottom.walkable && bottomLeft != null && bottomLeft.walkable)
                {
                    jumpNodeDir |= DirectionType.BottomLeft;
                    result = true;
                }
            }
            return result;
        }

        bool IsTopJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            if (toCheck == endNode)
                return true;
            var result = false;
            if (toCheck.neighbor.top != null && toCheck.neighbor.top.walkable)
            {
                var left = toCheck.neighbor.left;
                var right = toCheck.neighbor.right;
                var topLeft = toCheck.neighbor.topLeft;
                var topRight = toCheck.neighbor.topRight;
                if (left != null && !left.walkable && topLeft != null && topLeft.walkable)
                {
                    jumpNodeDir |= DirectionType.TopLeft;
                    result = true;
                }
                if (right != null && !right.walkable && topRight != null && topRight.walkable)
                {
                    jumpNodeDir |= DirectionType.TopRight;
                    result = true;
                }
            }
            return result;
        }

        bool IsBottomJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            if (toCheck == endNode)
                return true;
            var result = false;
            if (toCheck.neighbor.bottom != null && toCheck.neighbor.bottom.walkable)
            {
                var left = toCheck.neighbor.left;
                var right = toCheck.neighbor.right;
                var bottomLeft = toCheck.neighbor.bottomLeft;
                var bottomRight = toCheck.neighbor.bottomRight;
                if (left != null && !left.walkable && bottomLeft != null && bottomLeft.walkable)
                {
                    jumpNodeDir |= DirectionType.BottomLeft;
                    result = true;
                }
                if (right != null && !right.walkable && bottomRight != null && bottomRight.walkable)
                {
                    jumpNodeDir |= DirectionType.BottomRight;
                    result = true;
                }
            }
            return result;
        }

        bool IsTitleJumpNode(PathNode toCheck, DirectionType dir, out DirectionType jumpNodeDir)  //是否是斜方向的跳点
        {
            jumpNodeDir = dir;
            if (toCheck == endNode || toCheck == null || toCheck.walkable == false)
                return true;
            if (dir == DirectionType.TopRight)
                return IsTopRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.TopLeft)
                return IsTopLeftJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.BottomRight)
                return IsBottomRightJumpNode(toCheck, ref jumpNodeDir);
            else if (dir == DirectionType.BottomLeft)
                return IsBottomLeftJumpNode(toCheck, ref jumpNodeDir);
            return false;
        }

        bool IsTopRightJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            var result = false;
            result |= IsTopJumpNode(toCheck, ref jumpNodeDir);
            result |= IsRightJumpNode(toCheck, ref jumpNodeDir);  // 先检查自身是否符合line跳点, 是的话追加方向到jumpNodeDir
            if (!result) // 自身不符合line跳点 检查line方向有无跳点
            {
                var temp = DirectionType.None;
                var node = toCheck.neighbor.top;
                while (node != null && node.walkable)
                {
                    if (IsTopJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.top;
                }
                node = toCheck.neighbor.right;
                while (node != null && node.walkable)
                {
                    if (IsRightJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.right;
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionType.Top;
                jumpNodeDir |= DirectionType.Right;
            }
            return result;
        }

        bool IsTopLeftJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            var result = false;
            result |= IsTopJumpNode(toCheck, ref jumpNodeDir);
            result |= IsLeftJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionType.None;
                var node = toCheck.neighbor.top;
                while (node != null && node.walkable)
                {
                    if (IsTopJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.top;
                }
                node = toCheck.neighbor.left;
                while (node != null && node.walkable)
                {
                    if (IsLeftJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.left;
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionType.Top;
                jumpNodeDir |= DirectionType.Left;
            }
            return result;
        }

        bool IsBottomRightJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            var result = false;
            result |= IsBottomJumpNode(toCheck, ref jumpNodeDir);
            result |= IsRightJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionType.None;
                var node = toCheck.neighbor.bottom;
                while (node != null && node.walkable)
                {
                    if (IsBottomJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.bottom;
                }
                node = toCheck.neighbor.right;
                while (node != null && node.walkable)
                {
                    if (IsRightJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.right;
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionType.Bottom;
                jumpNodeDir |= DirectionType.Right;
            }
            return result;
        }

        bool IsBottomLeftJumpNode(PathNode toCheck, ref DirectionType jumpNodeDir)
        {
            var result = false;
            result |= IsBottomJumpNode(toCheck, ref jumpNodeDir);
            result |= IsLeftJumpNode(toCheck, ref jumpNodeDir);
            if (!result)
            {
                var temp = DirectionType.None;
                var node = toCheck.neighbor.bottom;
                while (node != null && node.walkable)
                {
                    if (IsBottomJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.bottom;
                }
                node = toCheck.neighbor.left;
                while (node != null && node.walkable)
                {
                    if (IsLeftJumpNode(node, ref temp))
                    {
                        result = true;
                        break;
                    }
                    node = node.neighbor.left;
                }
            }
            if (result)
            {
                jumpNodeDir |= DirectionType.Bottom;
                jumpNodeDir |= DirectionType.Left;
            }
            return result;
        }

        DirectionType GetDirection(PathNode ori, PathNode dest)
        {
            int xDelta = dest.x - ori.x, yDelta = dest.y - ori.y;
            if (xDelta > 0 && yDelta > 0)
                return DirectionType.TopRight;
            if (xDelta > 0 && yDelta == 0)
                return DirectionType.Right;
            if (xDelta > 0 && yDelta < 0)
                return DirectionType.BottomRight;
            if (xDelta < 0 && yDelta > 0)
                return DirectionType.TopLeft;
            if (xDelta < 0 && yDelta == 0)
                return DirectionType.Left;
            if (xDelta < 0 && yDelta < 0)
                return DirectionType.BottomLeft;
            if (xDelta == 0 && yDelta > 0)
                return DirectionType.Top;
            if (xDelta == 0 && yDelta < 0)
                return DirectionType.Bottom;
            return DirectionType.None;
        }
    }
}
