// Assets/_MythHunter/Code/Services/Heroes/LocalHeroDataService.cs

using Cysharp.Threading.Tasks;
using MythHunter.Entities.Heroes;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace MythHunter.Services.Heroes
{
    /// <summary>
    /// Локальна реалізація сервісу даних героїв для тестування
    /// </summary>
    public class LocalHeroDataService : IHeroDataService
    {
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, HeroDataModel> _heroCache = new Dictionary<string, HeroDataModel>();
        private readonly string _localStoragePath;

        [Inject]
        public LocalHeroDataService(IMythLogger logger)
        {
            _logger = logger;
            _localStoragePath = Path.Combine(Application.persistentDataPath, "HeroData");

            // Створюємо директорію, якщо вона не існує
            if (!Directory.Exists(_localStoragePath))
            {
                Directory.CreateDirectory(_localStoragePath);
            }

            // Завантажуємо існуючих героїв у кеш
            LoadAllHeroesFromDisk();
        }

        public UniTask<HeroDataModel> GetHeroDataAsync(string heroId)
        {
            if (_heroCache.TryGetValue(heroId, out var hero))
            {
                return UniTask.FromResult(hero);
            }

            // Спроба завантаження з диску
            var filePath = GetHeroFilePath(heroId);
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var heroData = JsonUtility.FromJson<HeroDataModel>(json);
                    _heroCache[heroId] = heroData;
                    return UniTask.FromResult(heroData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to load hero data: {ex.Message}", "HeroDataService", ex);
                    return UniTask.FromResult<HeroDataModel>(null);
                }
            }

            return UniTask.FromResult<HeroDataModel>(null);
        }

        public async UniTask SaveHeroDataAsync(HeroDataModel heroData)
        {
            try
            {
                var json = JsonUtility.ToJson(heroData, true);
                var filePath = GetHeroFilePath(heroData.HeroID);

                // Асинхронний запис на диск
                using (var writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(json);
                }

                // Оновлення кешу
                _heroCache[heroData.HeroID] = heroData;

                _logger.LogInfo($"Hero {heroData.Name} (ID: {heroData.HeroID}) saved successfully", "HeroDataService");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save hero data: {ex.Message}", "HeroDataService", ex);
                throw;
            }
        }

        public async UniTask DeleteHeroAsync(string heroId)
        {
            try
            {
                var filePath = GetHeroFilePath(heroId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                if (_heroCache.ContainsKey(heroId))
                {
                    _heroCache.Remove(heroId);
                }

                _logger.LogInfo($"Hero (ID: {heroId}) deleted successfully", "HeroDataService");
                await UniTask.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete hero: {ex.Message}", "HeroDataService", ex);
                throw;
            }
        }

        public UniTask<string[]> GetUserHeroIdsAsync(string userId)
        {
            try
            {
                var heroFiles = Directory.GetFiles(_localStoragePath, "*.json");
                var heroIds = new List<string>();

                foreach (var file in heroFiles)
                {
                    var heroId = Path.GetFileNameWithoutExtension(file);
                    heroIds.Add(heroId);
                }

                return UniTask.FromResult(heroIds.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user hero IDs: {ex.Message}", "HeroDataService", ex);
                return UniTask.FromResult(new string[0]);
            }
        }

        private string GetHeroFilePath(string heroId)
        {
            return Path.Combine(_localStoragePath, $"{heroId}.json");
        }

        private void LoadAllHeroesFromDisk()
        {
            try
            {
                if (!Directory.Exists(_localStoragePath))
                {
                    return;
                }

                var heroFiles = Directory.GetFiles(_localStoragePath, "*.json");
                foreach (var file in heroFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var heroData = JsonUtility.FromJson<HeroDataModel>(json);
                        _heroCache[heroData.HeroID] = heroData;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to load hero from file {file}: {ex.Message}", "HeroDataService");
                    }
                }

                _logger.LogInfo($"Loaded {_heroCache.Count} heroes from disk", "HeroDataService");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load heroes from disk: {ex.Message}", "HeroDataService", ex);
            }
        }
    }
}
