using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Клас запиту на ресурс
    /// </summary>
    public class ResourceRequest<T> where T : UnityEngine.Object
    {
        public string Key { get; }
        public Task<T> Task { get; }
        public bool IsCompleted => Task.IsCompleted;
        
        public ResourceRequest(string key, Task<T> task)
        {
            Key = key;
            Task = task;
        }
    }
}