using System.Collections.Generic;
using System;
using UnityEngine;
using FT;

public enum SelectType
{
    Block,
    Start,
    End,
}

public class PathFinderTest : MonoBehaviour
{
    [NonSerialized]
    public SelectType type;
    [NonSerialized]
    public TestTile startNode;
    [NonSerialized]
    public TestTile endNode;

    TestTile[,] tiles;
    List<TestTile> displayList = new List<TestTile>();
    List<PathNode> path = new List<PathNode>();

    const int WIDTH = 20;
    const int HEIGHT = 12;

    public TestTile node;
    public GameObject blockSelectObj;
    public GameObject startSelectObj;
    public GameObject endSelectObj;

    AbsPathFinder aStarPathFinder;
    AbsPathFinder jpsPathFinder;

    void Awake()
    {
        aStarPathFinder = new AStarPathFinder(this);
        aStarPathFinder.SetNode2Pos(Node2Pos);
        aStarPathFinder.InitMap(WIDTH, HEIGHT);
        aStarPathFinder.recorder.SetDisplayAction(DisplayRecord);
        aStarPathFinder.recorder.SetOnPlayEndAction(OnPlayEnd);

        jpsPathFinder = new JPSPathFinder(this);
        jpsPathFinder.SetNode2Pos(Node2Pos);
        jpsPathFinder.InitMap(WIDTH, HEIGHT);
        jpsPathFinder.recorder.SetDisplayAction(DisplayRecord);
        jpsPathFinder.recorder.SetOnPlayEndAction(OnPlayEnd);

        InitMap();
        ClickBlock();
    }

    void InitMap()
    {
        tiles = new TestTile[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                var node = Instantiate(this.node, this.node.transform.parent);
                var t = node.transform as RectTransform;
                t.anchoredPosition3D = Node2Pos(i, j);
                node.gameObject.SetActive(true);
                node.x = i;
                node.y = j;
                tiles[i, j] = node;

                node.button.onClick.AddListener(() =>
                {
                    OnNodeClick(node);
                });
            }
        }
    }

    Vector3 Node2Pos(int i, int j)
    {
        return new Vector3(i * 45, j * 45, 0);
    }

    public void ClickBlock()
    {
        type = SelectType.Block;
        blockSelectObj.SetActive(true);
        startSelectObj.SetActive(false);
        endSelectObj.SetActive(false);
    }

    public void ClickStart()
    {
        type = SelectType.Start;
        blockSelectObj.SetActive(false);
        startSelectObj.SetActive(true);
        endSelectObj.SetActive(false);
    }

    public void ClickEnd()
    {
        type = SelectType.End;
        blockSelectObj.SetActive(false);
        startSelectObj.SetActive(false);
        endSelectObj.SetActive(true);
    }

    public void OnClickAStarFind()
    {
        if (startNode != null && endNode != null)
        {
            OnClickClear();
            aStarPathFinder.SetStartNode(startNode.x, startNode.y);
            aStarPathFinder.SetEndNode(endNode.x, endNode.y);
            path = aStarPathFinder.FindPath();
        }
    }

    public void OnClickJpsFind()
    {
        if (startNode != null && endNode != null)
        {
            OnClickClear();
            jpsPathFinder.SetStartNode(startNode.x, startNode.y);
            jpsPathFinder.SetEndNode(endNode.x, endNode.y);
            path = jpsPathFinder.FindPath();
        }
    }

    public void OnClickClear()
    {
        aStarPathFinder.Clear();
        jpsPathFinder.Clear();

        foreach (var node in displayList)
        {
            node.img.gameObject.SetActive(false);
        }
        displayList.Clear();
    }

    public void OnNodeClick(TestTile node)
    {
        if (type == SelectType.Block)
        {
            SetNodeBlock(node);
        }
        else if (type == SelectType.Start)
        {
            SetStartNode(node);
        }
        else
        {
            SetEndNode(node);
        }
    }

    public void SetStartNode(TestTile node)
    {
        if (startNode != null)
        {
            startNode.img.gameObject.SetActive(false);
            startNode.button.interactable = true;
        }
        startNode = node;
        startNode.img.gameObject.SetActive(true);
        startNode.img.color = Color.green;
        startNode.button.interactable = false;
    }

    public void SetEndNode(TestTile node)
    {
        if (endNode != null)
        {
            endNode.img.gameObject.SetActive(false);
            endNode.button.interactable = true;
        }
        endNode = node;
        endNode.img.gameObject.SetActive(true);
        endNode.img.color = Color.red;
        endNode.button.interactable = false;
    }

    public void SetNodeBlock(TestTile node)
    {
        if (node.img.gameObject.activeSelf)
        {
            node.img.gameObject.SetActive(false);

            aStarPathFinder.RefreshWalkable(node.x, node.y, true);
            jpsPathFinder.RefreshWalkable(node.x, node.y, true);
        }
        else
        {
            node.img.gameObject.SetActive(true);
            node.img.color = Color.black;

            aStarPathFinder.RefreshWalkable(node.x, node.y, false);
            jpsPathFinder.RefreshWalkable(node.x, node.y, false);
        }
    }

    void DisplayRecord(PathNode node, Color color)
    {
        var n = tiles[node.x, node.y];
        if (n == startNode || n == endNode)
        {
            return;
        }

        if (!displayList.Contains(n))
        {
            displayList.Add(n);
        }
        n.img.gameObject.SetActive(true);
        n.img.color = color;
    }

    void OnPlayEnd()
    {
        foreach (var node in path)
        {
            var n = tiles[node.x, node.y];
            n.img.gameObject.SetActive(true);
            n.img.color = Color.green;
        }
    }
}
