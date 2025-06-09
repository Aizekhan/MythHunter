using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Клас запиту на ресурс
    /// </summary>
    public class ResourceRequest<T> where T : UnityEngine.Object
    {
        public string Key { get; }
        public UniTask<T> Task { get; }
        public bool IsCompleted => Task.Status.IsCompleted();
        
        public ResourceRequest(string key, UniTask<T> task)
        {
            Key = key;
            Task = task;
        }
    }
}