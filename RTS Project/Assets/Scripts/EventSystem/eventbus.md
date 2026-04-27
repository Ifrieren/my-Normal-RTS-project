
下面是一份**《高性能 EventBus 核心方法论与设计指南》。这份文档不贴完整的代码，而是拆解核心架构思路和每个关键方法的执行流**。
📘 高性能 EventBus 核心方法论与设计指南
一、 核心架构：放弃字典，拥抱“泛型静态缓存”
【传统做法的痛点】
传统的 EventBus 喜欢用 Dictionary<Type, List<Delegate>>。每次触发事件，都需要经历：计算事件类型的 Hash 值 -> 查找字典 -> 将 Delegate 向下转型。这带来了无谓的 CPU 开销。
【高性能方案设计】
利用 C# 的底层特性：带有静态字段的泛型类，在编译后，每一种确定的泛型类型都会拥有一块独立且互不干扰的内存空间。
我们设计一个对外的壳 public static class EventBus 负责暴露 API。
内部使用 internal static class EventBusInternal<T> 负责实际干活。
当调用 EventBus.Publish(new MoveEvent()) 时，实际上访问的是 EventBusInternal<MoveEvent>.listeners 这个列表。它在物理上和 DamageEvent 的列表是绝对隔离的。
（直接内存访问）。
二、 核心机制：状态锁与“墓碑模式” (Tombstone)
在事件分发过程中（即 for 循环遍历 List 时），如果某个监听者在自己的函数里调用了 Subscribe 或 Unsubscribe，直接修改 List 会导致索引错乱或抛出异常。
为了解决并发修改问题，且不使用耗费内存的 .ToArray() 拷贝，我们需要设计三个状态和两个容器：
容器：
主列表 (listeners)：存放当前真正在监听的对象。
缓冲池 (pendingAdds)：临时存放派发事件期间“新来”的监听者。
状态标记：
isPublishing：当前是否正在遍历派发事件？
needsCleanup：当前主列表里是否有需要被清理的“死”节点？
needsSort：当前主列表是否需要重新排序？
三、 方法逻辑拆解
1. Subscribe (订阅事件) 的执行流
目标：安全地将监听者加入队列，并处理去重和排序请求。
Step 1：遍历查重。 遍历 listeners，如果发现传入的 callback 已经存在：
检查它是不是被打了“墓碑标记”（准备被删，但还没死透）。如果是，直接“复活”它，更新它的优先级，标记 needsSort = true，然后退出。
如果没有打标记，说明是纯粹的重复订阅，直接 return 忽略。
Step 2：判断环境状态。 检查 isPublishing 的值：
如果为 True（正在派发中）： 绝对不能直接插入主列表！把新节点放入备用的 pendingAdds 缓冲池中。
如果为 False（平时）： 安全地加入 listeners 主列表，并标记 needsSort = true。
设计哲学：延迟排序（Lazy Sorting）。订阅时不立刻排序，而是打个标记，因为可能同一帧会连续订阅 10 个函数，立刻排序会浪费 9 次 CPU 算力。
1. Unsubscribe (取消订阅) 的执行流
目标：安全地移除监听者，绝不破坏正在进行的循环。
Step 1：寻找目标。 遍历 listeners 找到对应的 callback。
Step 2：判断环境状态。 检查 isPublishing 的值：
如果为 True（正在派发中）： 绝对不能调用 List.RemoveAt()！因为如果删除了第 3 个元素，原本第 4 个元素会前移变成第 3 个，导致当前的 for 循环直接跳过一个元素！
解决方案（墓碑模式）： 给这个节点打上 IsPendingRemoval = true 的标记（立个墓碑），并设置全局状态 needsCleanup = true。
如果为 False（平时）： 安全地调用 List.RemoveAt() 物理删除。
1. Publish (发布事件) 的核心执行流
目标：按优先级安全、稳定地将事件传递给每一个监听者。
Step 1：处理延迟排序。 检查 needsSort，如果为 True，调用 List.Sort() 进行降序排列，然后把标记设为 False。
Step 2：加锁。 将 isPublishing 设为 true。
Step 3：核心遍历 (Try-Catch 包裹)。 开启 for 循环（比 foreach 快且无迭代器开销）：
拿出当前节点。检查它的“墓碑标记”，如果 IsPendingRemoval == true，直接 continue 跳过。
使用 try-catch 包裹 callback.Invoke()。这样即使 10 个监听者里有 1 个抛出了空指针异常，也只会在控制台报错，剩下 9 个监听者依然能正常收到事件（隔离爆炸半径）。
检查事件是否实现了 IEventCancellable 且 IsCancelled 为 True。如果是，立刻 break，中断整个循环，不再向低优先级的函数广播。
Step 4：解锁与善后 (Finally 包裹)。
即使发生严重异常跳出，finally 块也能保证 isPublishing = false 必定执行（解锁）。
检查 pendingAdds.Count > 0 或 needsCleanup == true，如果满足，调用专门的善后方法。
1. ProcessPendingModifications (善后清理) 的执行流
目标：在事件彻底派发完毕后，安全地合并新增节点并物理销毁死亡节点。
清理死亡节点（关键细节）：
如果 needsCleanup 为 True，必须使用倒序遍历（从 Count - 1 循环到 0）。
为什么要倒序？ 因为正序删除时（比如删了索引 2），后面的索引会全部错位；而倒序删除时，删掉索引 8，对前面的 7、6、5 的索引没有任何影响，极其安全高效。
找到带墓碑标记的节点，执行物理的 RemoveAt。
合并新增节点：
如果 pendingAdds 有数据，使用 listeners.AddRange() 将缓冲池的内容一次性倒进主列表。
清空缓冲池。
标记 needsSort = true（因为加入了新兄弟，下次派发前需要重新排座次）。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventSystem
{
    // 拦截器接口
    public interface IEventCancellable
    {
        bool IsCancelled { get; set; }
    }

    /// <summary>
    /// 全局事件总线 API 入口
    /// </summary>
    public static class EventBus
    {
        // 强制约束 where T : class，防止误传 struct 导致隐式装箱 GC
        public static void Subscribe<T>(Action<T> callback, int priority = 0) where T : class 
            => EventBusInternal<T>.Subscribe(callback, priority);

        public static void Unsubscribe<T>(Action<T> callback) where T : class 
            => EventBusInternal<T>.Unsubscribe(callback);

        public static void Publish<T>(T evt) where T : class 
            => EventBusInternal<T>.Publish(evt);

        public static void ClearAll<T>() where T : class 
            => EventBusInternal<T>.Clear();
    }

    /// <summary>
    /// 泛型静态内部类：利用泛型特性，自动为每种事件分配独立的内存空间，彻底消灭 Dictionary
    /// </summary>
    internal static class EventBusInternal<T> where T : class
    {
        private class Node : IComparable<Node>
        {
            public int Priority;
            public Action<T> Callback;
            public bool IsPendingRemoval; // 墓碑标记：用于防并发修改

            public int CompareTo(Node other)
            {
                return other.Priority.CompareTo(this.Priority); // 降序
            }
        }

        // 核心监听者列表
        private static readonly List<Node> listeners = new List<Node>(16);
        // 缓冲池：用于处理在事件派发过程中新增的监听者
        private static readonly List<Node> pendingAdds = new List<Node>();

        private static bool isPublishing = false;
        private static bool needsCleanup = false;
        private static bool needsSort = false;

        public static void Subscribe(Action<T> callback, int priority)
        {
            // 防重复订阅
            foreach (var node in listeners)
            {
                if (node.Callback == callback)
                {
                    if (node.IsPendingRemoval)
                    {
                        node.IsPendingRemoval = false;
                        node.Priority = priority;
                        needsSort = true;
                    }
                    return;
                }
            }

            var newNode = new Node { Callback = callback, Priority = priority };

            if (isPublishing)
            {
                pendingAdds.Add(newNode); // 派发中，先放缓冲池
            }
            else
            {
                listeners.Add(newNode);
                needsSort = true;
            }
        }

        public static void Unsubscribe(Action<T> callback)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].Callback == callback)
                {
                    if (isPublishing)
                    {
                        listeners[i].IsPendingRemoval = true; // 派发中，打上墓碑标记延迟删除
                        needsCleanup = true;
                    }
                    else
                    {
                        listeners.RemoveAt(i); // 非派发中，直接物理删除
                    }
                    return;
                }
            }
        }

        public static void Publish(T evt)
        {
            // 延迟排序：只在发布前需要时才排序
            if (needsSort)
            {
                listeners.Sort();
                needsSort = false;
            }

            isPublishing = true;
            try
            {
                // 用 for 循环遍历，比 foreach 略快，且不会产生迭代器开销
                for (int i = 0; i < listeners.Count; i++)
                {
                    var node = listeners[i];
                    
                    if (node.IsPendingRemoval) continue; // 跳过被标记注销的

                    try
                    {
                        node.Callback(evt);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] 回调执行报错，事件类型 {typeof(T).Name}: {e}");
                    }

                    // 处理取消逻辑
                    if (evt is IEventCancellable cancellable && cancellable.IsCancelled)
                    {
                        break; // 停止向低优先级派发
                    }
                }
            }
            finally
            {
                isPublishing = false;
                
                // 循环彻底结束后，统一清理增删操作
                if (pendingAdds.Count > 0 || needsCleanup)
                {
                    ProcessPendingModifications();
                }
            }
        }

        private static void ProcessPendingModifications()
        {
            if (needsCleanup)
            {
                // 倒序删除，防止索引错乱
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    if (listeners[i].IsPendingRemoval)
                    {
                        listeners.RemoveAt(i);
                    }
                }
                needsCleanup = false;
            }

            if (pendingAdds.Count > 0)
            {
                listeners.AddRange(pendingAdds);
                pendingAdds.Clear();
                needsSort = true;
            }
        }

        public static void Clear()
        {
            listeners.Clear();
            pendingAdds.Clear();
            isPublishing = needsCleanup = needsSort = false;
        }
    }
}