using UnityEngine;

namespace MythHunter.Resources.SceneManagement
{
    /// <summary>
    /// Клас для посилання на сцену
    /// </summary>
    [System.Serializable]
    public class SceneReference
    {
        [SerializeField] private string scenePath;
        [SerializeField] private string sceneName;
        
        public string ScenePath => scenePath;
        public string SceneName => sceneName;
        
        public SceneReference(string path)
        {
            scenePath = path;
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        
        public override string ToString()
        {
            return sceneName;
        }
    }
}