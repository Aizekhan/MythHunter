// Assets/_MythHunter/Code/UI/Controllers/HeroGridController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Controllers
{
    /// <summary>
    /// Контролер для управління сіткою героїв з пагінацією
    /// </summary>
    public class HeroGridController : IDisposable
    {
        private readonly IUIViewFactory _uiViewFactory;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;

        // Налаштування сітки
        private const int HEROES_PER_PAGE = 12; // 4x3 сітка
        private const int GRID_COLUMNS = 4;
        private const int GRID_ROWS = 3;

        // Дані
        private List<HeroCardModel> _allHeroes = new();
        private List<HeroCardModel> _filteredHeroes = new();
        private List<HeroCardUI> _activeHeroCards = new();

        // Стан
        private Transform _gridContainer;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _searchFilter = "";

        // Події
        public event Action<int, int> OnPageChanged; // currentPage, totalPages
        public event Action<string> OnHeroSelected;

        public HeroGridController(IUIViewFactory uiViewFactory, IMythLogger logger, IDIContainer container)
        {
            _uiViewFactory = uiViewFactory;
            _logger = logger;
            _container = container;
        }

        /// <summary>
        /// Ініціалізує контролер з контейнером сітки
        /// </summary>
        public void Initialize(Transform gridContainer)
        {
            _gridContainer = gridContainer;
            _logger.LogInfo("HeroGridController ініціалізовано", "UI");
        }

        /// <summary>
        /// Встановлює список всіх героїв
        /// </summary>
        public void SetHeroes(List<HeroCardModel> heroes)
        {
            _allHeroes = heroes ?? new List<HeroCardModel>();
            ApplyFilters();
            UpdatePagination();

            _logger.LogInfo($"Встановлено {_allHeroes.Count} героїв, після фільтрації: {_filteredHeroes.Count}", "UI");
        }

        /// <summary>
        /// Додає нового героя до списку
        /// </summary>
        public void AddHero(HeroCardModel hero)
        {
            if (hero != null && !_allHeroes.Any(h => h.ArchetypeId == hero.ArchetypeId))
            {
                _allHeroes.Add(hero);
                ApplyFilters();
                UpdatePagination();
            }
        }

        /// <summary>
        /// Оновлює героя в списку
        /// </summary>
        public void UpdateHero(HeroCardModel updatedHero)
        {
            if (updatedHero == null)
                return;

            var index = _allHeroes.FindIndex(h => h.ArchetypeId == updatedHero.ArchetypeId);
            if (index >= 0)
            {
                _allHeroes[index] = updatedHero;
                ApplyFilters();
                RefreshCurrentPage();
            }
        }

        /// <summary>
        /// Встановлює фільтр пошуку
        /// </summary>
        public void SetSearchFilter(string filter)
        {
            _searchFilter = filter?.ToLower() ?? "";
            ApplyFilters();
            GoToPage(1); // Повертаємося на першу сторінку при зміні фільтра
        }

        /// <summary>
        /// Переходить на вказану сторінку
        /// </summary>
        public void GoToPage(int page)
        {
            if (page < 1 || page > _totalPages)
                return;

            _currentPage = page;
            RefreshCurrentPage();
            OnPageChanged?.Invoke(_currentPage, _totalPages);

            _logger.LogDebug($"Перехід на сторінку {_currentPage}/{_totalPages}", "UI");
        }

        /// <summary>
        /// Переходить на наступну сторінку
        /// </summary>
        public void NextPage()
        {
            GoToPage(_currentPage + 1);
        }

        /// <summary>
        /// Переходить на попередню сторінку
        /// </summary>
        public void PreviousPage()
        {
            GoToPage(_currentPage - 1);
        }

        /// <summary>
        /// Отримує інформацію про поточну сторінку
        /// </summary>
        public (int currentPage, int totalPages) GetPageInfo()
        {
            return (_currentPage, _totalPages);
        }

        /// <summary>
        /// Отримує героїв на поточній сторінці
        /// </summary>
        public List<HeroCardModel> GetCurrentPageHeroes()
        {
            var startIndex = (_currentPage - 1) * HEROES_PER_PAGE;
            return _filteredHeroes.Skip(startIndex).Take(HEROES_PER_PAGE).ToList();
        }

        /// <summary>
        /// Застосовує фільтри до списку героїв
        /// </summary>
        private void ApplyFilters()
        {
            _filteredHeroes = _allHeroes.Where(hero => MatchesSearchFilter(hero)).ToList();
        }

        /// <summary>
        /// Перевіряє чи відповідає герой фільтру пошуку
        /// </summary>
        private bool MatchesSearchFilter(HeroCardModel hero)
        {
            if (string.IsNullOrEmpty(_searchFilter))
                return true;

            var searchLower = _searchFilter.ToLower();
            return hero.Name.ToLower().Contains(searchLower) ||
                   hero.Race.ToLower().Contains(searchLower) ||
                   hero.Class.ToLower().Contains(searchLower);
        }

        /// <summary>
        /// Оновлює пагінацію
        /// </summary>
        private void UpdatePagination()
        {
            _totalPages = Mathf.CeilToInt((float)_filteredHeroes.Count / HEROES_PER_PAGE);
            _totalPages = Mathf.Max(1, _totalPages); // Мінімум 1 сторінка

            // Якщо поточна сторінка більша за загальну кількість, переходимо на останню
            if (_currentPage > _totalPages)
            {
                _currentPage = _totalPages;
            }
        }

        /// <summary>
        /// Оновлює відображення поточної сторінки
        /// </summary>
        private async void RefreshCurrentPage()
        {
            if (_gridContainer == null)
                return;

            // Очищуємо поточні картки
            ClearCurrentCards();

            // Отримуємо героїв для поточної сторінки
            var pageHeroes = GetCurrentPageHeroes();

            // Створюємо нові картки
            foreach (var hero in pageHeroes)
            {
                await CreateHeroCard(hero);
            }

            _logger.LogDebug($"Оновлено сторінку {_currentPage}, показано {pageHeroes.Count} героїв", "UI");
        }

        /// <summary>
        /// Створює картку героя
        /// </summary>
        private async UniTask CreateHeroCard(HeroCardModel hero)
        {
            try
            {
                var view = await _uiViewFactory.CreateViewAsync(ViewId.HeroCard);
                var heroCard = view as HeroCardUI;

                if (heroCard != null)
                {
                    // Налаштовуємо картку
                    heroCard.transform.SetParent(_gridContainer, false);
                    _container.InjectDependencies(heroCard);

                    heroCard.Setup(hero);
                    heroCard.OnHeroSelected += OnHeroCardSelected;

                    _activeHeroCards.Add(heroCard);
                }
                else
                {
                    _logger.LogError($"View не є HeroCardUI: {view?.GetType().Name}", "UI");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка створення картки героя {hero.Name}: {ex.Message}", "UI", ex);
            }
        }

        /// <summary>
        /// Очищує поточні картки
        /// </summary>
        private void ClearCurrentCards()
        {
            foreach (var card in _activeHeroCards)
            {
                if (card != null)
                {
                    card.OnHeroSelected -= OnHeroCardSelected;
                    _uiViewFactory.ReturnViewToPool(ViewId.HeroCard, card);
                }
            }
            _activeHeroCards.Clear();
        }

        /// <summary>
        /// Обробник вибору героя
        /// </summary>
        private void OnHeroCardSelected(string archetypeId)
        {
            OnHeroSelected?.Invoke(archetypeId);
        }

        /// <summary>
        /// Оновлює стан карток після зміни вибору
        /// </summary>
        public void RefreshCardStates()
        {
            for (int i = 0; i < _activeHeroCards.Count; i++)
            {
                var card = _activeHeroCards[i];
                var pageHeroes = GetCurrentPageHeroes();

                if (i < pageHeroes.Count)
                {
                    card.Setup(pageHeroes[i]); // Повторно налаштовуємо з оновленими даними
                }
            }
        }

        /// <summary>
        /// Очищення ресурсів
        /// </summary>
        public void Dispose()
        {
            ClearCurrentCards();
            _allHeroes.Clear();
            _filteredHeroes.Clear();

            OnPageChanged = null;
            OnHeroSelected = null;

            _logger.LogInfo("HeroGridController очищено", "UI");
        }
    }
}
