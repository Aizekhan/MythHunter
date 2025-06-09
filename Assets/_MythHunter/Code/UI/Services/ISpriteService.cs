// Шлях: Assets/_MythHunter/Code/UI/Services/ISpriteService.cs
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.UI.Services
{
    public interface ISpriteService
    {
        UniTask<Sprite> GetSpriteAsync(string path, Sprite defaultSprite = null);
        void PreloadSprites(string[] paths);
        void ClearCache();
    }
}
