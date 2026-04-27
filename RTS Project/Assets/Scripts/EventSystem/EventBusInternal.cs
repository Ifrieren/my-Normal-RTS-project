using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace RTS.EventSystem
{
    internal static class EventBusInternal<T> where T : class
    {
        /// <summary>
        /// 订阅特定类型的事件。
        /// </summary>
        /// <typeparam name="T">事件的 Class 类型。</typeparam>
        /// <param name="callback">事件触发时执行的委托。</param>
        /// <param name="priority">优先级，数值越大越先执行。</param>
        private class Node : IComparable<Node>
        {
            public int Priority;
            public Action<T> Callback;
            public bool isPendingRemoval;
            public int CompareTo(Node other)
            => other.Priority.CompareTo(this.Priority);
        }
        private static readonly List<Node> listeners = new();
        private static readonly List<Node> PendingAdds = new();
        private static bool isPublishing;
        private static bool needCleaningUp;
        private static bool needSort;
        public static void Subscribe(Action<T> Callback, int Priority)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].Callback == Callback)
                {
                    return;// 检查是否已经有重复的callback事件
                }
                if (listeners[i].isPendingRemoval)
                {
                    // 检查该事件是否处于悬垂删除状态
                    listeners[i].isPendingRemoval = false;
                    listeners[i].Priority = Priority;
                    needSort = true;
                    return;
                }

            }
            for (int i = 0; i < PendingAdds.Count; i++)
            {
                if (PendingAdds[i].Callback == Callback)
                {
                    // 如果缓冲池里已经有这个待加入的任务，更新优先级即可
                    PendingAdds[i].Priority = Priority;
                    return;
                }
            }
            Node newNode = new Node
            {
                Priority = Priority,
                Callback = Callback,
                isPendingRemoval = false,
            };
            // 判断是否正在发布事件
            if (isPublishing)
            {
                PendingAdds.Add(newNode);
            }
            else
            {
                listeners.Add(newNode);
                needSort = true;
            }
        }
        public static void UnSubscribe(Action<T> Callback)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].Callback == Callback)
                {
                    if (isPublishing)
                    {
                        listeners[i].isPendingRemoval = true;
                        needCleaningUp = true;
                    }
                    else
                    {
                        listeners.RemoveAt(i);
                    }
                }
            }
        }
        public static void Publish(T evt)
        {
            if (needSort)
            {
                listeners.Sort();
                needSort = false;
            }
            isPublishing = true;
            try
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (listeners[i].isPendingRemoval == true) { continue; }
                    if (evt is ICancelable a && a.isCanceled)
                    {
                        break;
                    }
                    try
                    {
                        listeners[i].Callback?.Invoke(evt);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"there's error with {evt} , error : {e} ");
                    }
                }
            }
            finally
            {
                isPublishing = false;
                ProcessPendingModifications();
            }
        }
        private static void ProcessPendingModifications()
        {
            if (needCleaningUp)
            {
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    if (listeners[i].isPendingRemoval)
                    {
                        listeners.RemoveAt(i);
                    }
                }
                needCleaningUp = false; // 重置
            }

            if (PendingAdds.Count > 0)
            {
                for(int i = 0;i<PendingAdds.Count; i++)
                {
                    bool exists = false;
                    // 在加入前，先检查主列表中是否已经存在该回调
                    for(int j = 0;j < listeners.Count; j++)
                    {
                        if (listeners[j].Callback == PendingAdds[i].Callback)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        listeners.Add(PendingAdds[i]);
                        needSort = true; // 确实有新成员才触发排序
                    }
                }
                PendingAdds.Clear();
            }
        }
    }
}