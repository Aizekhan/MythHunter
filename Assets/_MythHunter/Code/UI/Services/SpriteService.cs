// Шлях: Assets/_MythHunter/Code/UI/Services/SpriteService.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Services
{
    public class SpriteService : ISpriteService
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        [Inject]
        public SpriteService(IResourceProvider resourceProvider, IMythLogger logger)
        {
            _resourceProvider = resourceProvider;
            _logger = logger;
        }

        public async UniTask<Sprite> GetSpriteAsync(string path, Sprite defaultSprite = null)
        {
            if (string.IsNullOrEmpty(path))
                return defaultSprite;

            if (_cache.TryGetValue(path, out var sprite))
                return sprite;

            try
            {
                sprite = await _resourceProvider.LoadAsync<Sprite>(path);
                if (sprite != null)
                {
                    _cache[path] = sprite;
                    return sprite;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning($"Failed to load sprite from {path}: {ex.Message}", "SpriteService");
            }

            return defaultSprite;
        }

        public void PreloadSprites(string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return;

            foreach (var path in paths)
            {
                UniTask.Create(async () => {
                    await GetSpriteAsync(path);
                });
            }
        }

        // Додатковий метод для очищення кешу
        public void ClearCache()
        {
            _cache.Clear();
            _logger.LogInfo("Sprite cache cleared", "SpriteService");
        }
    }
}
