using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FT
{
    public class PathRecorder
    {
#if UNITY_EDITOR
        struct Record
        {
            public int id;
            public PathNode node;
            public Color color;
        }

        MonoBehaviour mono;
        Coroutine recordCT;

        List<Record> records = new List<Record>();
        float playTime = 0.05f;
        bool debug = true;

        Color openNodeColor = Color.yellow;
        Color closeNodeColor = Color.red;

        Action<PathNode, Color> DisplayAction;
        Action OnPlayEnd;

        public PathRecorder(MonoBehaviour mono)
        {
            this.mono = mono;
        }

        public void SetDisplayAction(Action<PathNode, Color> action)
        {
            DisplayAction = action;
        }

        public void SetOnPlayEndAction(Action action)
        {
            OnPlayEnd = action;
        }

        public void AddRecord(PathNode node, NodeStatus status)
        {
            if (!debug)
            {
                return;
            }

            var c = status == NodeStatus.Close ? closeNodeColor : openNodeColor;
            var exist = false;
            if (node.status != NodeStatus.Close)
            {
                var count = records.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    if (records[i].node == node)
                    {
                        c = records[i].color;
                        exist = true;
                        break;
                    }
                }
            }

            var record = new Record { node = node };
            if (exist)
            {
                record.color = c - new Color(0.2f, 0.2f, 0, 0);
            }
            else
            {
                record.color = c;
            }
            records.Add(record);
        }

        public void Play()
        {
            if (!debug)
            {
                return;
            }
            recordCT = mono.StartCoroutine(PlayRecord());
        }

        IEnumerator PlayRecord()
        {
            var count = records.Count;
            for (int i = 0; i < count; i++)
            {
                DisplayAction?.Invoke(records[i].node, records[i].color);
                yield return new WaitForSeconds(playTime);
            }
            OnPlayEnd?.Invoke();
            recordCT = null;
        }

        public void Clear()
        {
            if (!debug)
            {
                return;
            }
            if (recordCT != null)
            {
                mono.StopCoroutine(recordCT);
            }
            records.Clear();
        }
#endif
    }
}