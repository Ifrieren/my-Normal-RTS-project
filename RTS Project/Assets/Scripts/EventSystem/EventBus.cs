using System;
using UnityEngine;
namespace RTS.EventSystem
{
    public interface ICancelable
    {
        bool isCanceled{ set; get; }
    }

    public static class EventBus
    {
        public static void Subscribe<T>(Action<T> Callback, int Priority = 0) where T : class
        => EventBusInternal<T>.Subscribe(Callback, Priority);
        public static void UnSubscribe<T>(Action<T> Callback) where T : class
        => EventBusInternal<T>.UnSubscribe(Callback);
        public static void Publish<T>(T evt) where T : class
        => EventBusInternal<T>.Publish(evt);
    }



}
