using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace FT
{
    public abstract class AbsPathFinder
    {
        protected int width;
        protected int height;

        protected PathNode[,] nodes;
        protected PathNode startNode;
        protected PathNode endNode;

        protected FTBinaryHeap<PathNode> openHeap = new FTBinaryHeap<PathNode>();
        protected List<PathNode> closeList = new List<PathNode>();
        protected List<PathNode> path = new List<PathNode>();

        Func<int, int, Vector3> Node2Pos;

#if UNITY_EDITOR
        public PathRecorder recorder;       
#endif

#if UNITY_EDITOR
        public AbsPathFinder(MonoBehaviour mono)
        {
            recorder = new PathRecorder(mono);
        }
#endif

        public AbsPathFinder() { }

        public void SetNode2Pos(Func<int, int, Vector3> func)
        {
            Node2Pos = func;
        }

        public void InitMap(int width, int height)
        {
            this.width = width;
            this.height = height;
            nodes = new PathNode[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pos = Node2Pos.Invoke(x, y);
                    nodes[x, y] = new PathNode(x, y, pos, true);
                }
            }

            InitNeighbor();
        }

        void InitNeighbor()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    nodes[x, y].SetNeighbor(GetNeighbor(x, y));
        }

        Neighbor GetNeighbor(int x, int y)
        {
            Neighbor neighbor = new Neighbor();

            //上
            int ytemp = y + 1;
            if (ytemp < height)
                neighbor.top = nodes[x, ytemp];

            //下
            ytemp = y - 1;
            if (ytemp >= 0)
                neighbor.bottom = nodes[x, ytemp];

            //左
            int xtemp = x - 1;
            if (xtemp >= 0)
                neighbor.left = nodes[xtemp, y];

            //右
            xtemp = x + 1;
            if (xtemp < width)
                neighbor.right = nodes[xtemp, y];

            //左上
            xtemp = x - 1; ytemp = y + 1;
            if (xtemp >= 0 && ytemp < height)
                neighbor.topLeft = nodes[xtemp, ytemp];

            //左下
            xtemp = x - 1; ytemp = y - 1;
            if (xtemp >= 0 && ytemp >= 0)
                neighbor.bottomLeft = nodes[xtemp, ytemp];

            //右上
            xtemp = x + 1; ytemp = y + 1;
            if (xtemp < width && ytemp < height)
                neighbor.topRight = nodes[xtemp, ytemp];

            //右下
            xtemp = x + 1; ytemp = y - 1;
            if (xtemp < width && ytemp >= 0)
                neighbor.bottomRight = nodes[xtemp, ytemp];

            return neighbor;
        }

        public void RefreshWalkable(int x, int y, bool walkable)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            nodes[x, y].walkable = walkable;
        }

        public void SetStartNode(int x, int y)
        {
            startNode = nodes[x, y];
        }

        public void SetEndNode(int x, int y)
        {
            endNode = nodes[x, y];
        }

        public List<PathNode> FindPath()
        {
            Profiler.BeginSample("pathfind");
            var time = Time.realtimeSinceStartup;

            Clear();
            var success = Search(startNode);
            if (success)
            {
                var node = endNode;
                while (node.parent != null)
                {
                    path.Add(node);
                    node = node.parent;
                }
                path.Reverse();
            }
            var time2 = Time.realtimeSinceStartup;

            Debug.Log("use time:  " + (time2 - time).ToString());
            Profiler.EndSample();
#if UNITY_EDITOR
            recorder.Play();
#endif
            return path;
        }

        /// <summary>
        /// 子类寻找openList的策略不同
        /// </summary>
        protected abstract bool Search(PathNode node);

        protected PathNode TryGetOpenNode()
        {
            if (openHeap.Count > 0)
                return openHeap.Dequeue();
            return null;
        }

        public void Clear()
        {
#if UNITY_EDITOR
            recorder.Clear();
#endif
            path.Clear();
            while (openHeap.Count > 0)
            {
                var node = openHeap.RemoveAtEnd();
                node.Clear();
            }

            var count = closeList.Count;
            for (int i = 0; i < count; i++)
            {
                closeList[i].Clear();
            }
            closeList.Clear();
        }
    }
}