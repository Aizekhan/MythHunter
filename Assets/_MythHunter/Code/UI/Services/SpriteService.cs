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
            _logger.LogInfo($"🖼️ SpriteService: Запит на завантаження {path}", "SpriteService");

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("⚠️ SpriteService: Порожній шлях", "SpriteService");
                return defaultSprite;
            }

            if (_cache.TryGetValue(path, out var sprite))
            {
                _logger.LogInfo($"✅ SpriteService: Знайдено в кеші {path}", "SpriteService");
                return sprite;
            }

            try
            {
                _logger.LogInfo($"🔄 SpriteService: Завантажуємо з ResourceProvider {path}", "SpriteService");
                sprite = await _resourceProvider.LoadAsync<Sprite>(path);
                if (sprite != null)
                {
                    _cache[path] = sprite;
                    _logger.LogInfo($"✅ SpriteService: Успішно завантажено {path}", "SpriteService");
                    return sprite;
                }
                else
                {
                    _logger.LogWarning($"⚠️ SpriteService: ResourceProvider повернув null для {path}", "SpriteService");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning($"❌ SpriteService: Помилка завантаження {path}: {ex.Message}", "SpriteService");
            }

            _logger.LogInfo($"🔄 SpriteService: Повертаємо defaultSprite для {path}", "SpriteService");
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
