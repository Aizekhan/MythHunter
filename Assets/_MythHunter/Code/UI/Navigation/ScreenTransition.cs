// Шлях: Assets/_MythHunter/Code/UI/Navigation/ScreenTransition.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using System;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Реалізація анімацій переходу між екранами
    /// </summary>
    public class ScreenTransition : IScreenTransition
    {
        private readonly IMythLogger _logger;

        [Inject]
        public ScreenTransition(IMythLogger logger)
        {
            _logger = logger;
        }

        public async UniTask PlayTransitionAsync(GameObject fromScreen, GameObject toScreen, TransitionType type, bool isForward)
        {
            try
            {
                switch (type)
                {
                    case TransitionType.None:
                        // Миттєвий перехід без анімації
                        if (fromScreen != null)
                            fromScreen.SetActive(false);
                        if (toScreen != null)
                            toScreen.SetActive(true);
                        break;

                    case TransitionType.Fade:
                        await PlayFadeTransitionAsync(fromScreen, toScreen, isForward);
                        break;

                    case TransitionType.SlideLeft:
                        await PlaySlideTransitionAsync(fromScreen, toScreen, isForward, new Vector2(-1, 0));
                        break;

                    case TransitionType.SlideRight:
                        await PlaySlideTransitionAsync(fromScreen, toScreen, isForward, new Vector2(1, 0));
                        break;

                    case TransitionType.SlideUp:
                        await PlaySlideTransitionAsync(fromScreen, toScreen, isForward, new Vector2(0, 1));
                        break;

                    case TransitionType.SlideDown:
                        await PlaySlideTransitionAsync(fromScreen, toScreen, isForward, new Vector2(0, -1));
                        break;

                    case TransitionType.Scale:
                        await PlayScaleTransitionAsync(fromScreen, toScreen, isForward);
                        break;

                    case TransitionType.Default:
                    default:
                        // За замовчуванням - затухання
                        await PlayFadeTransitionAsync(fromScreen, toScreen, isForward);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка під час анімації переходу: {ex.Message}", "Navigation", ex);

                // У випадку помилки - просто активуємо потрібні екрани
                if (fromScreen != null)
                    fromScreen.SetActive(false);
                if (toScreen != null)
                    toScreen.SetActive(true);
            }
        }

        private async UniTask PlayFadeTransitionAsync(GameObject fromScreen, GameObject toScreen, bool isForward)
        {
            const float duration = 0.3f;

            // Отримуємо CanvasGroup для анімації прозорості
            var fromCanvasGroup = fromScreen?.GetComponent<CanvasGroup>();
            if (fromCanvasGroup == null && fromScreen != null)
                fromCanvasGroup = fromScreen.AddComponent<CanvasGroup>();

            var toCanvasGroup = toScreen?.GetComponent<CanvasGroup>();
            if (toCanvasGroup == null && toScreen != null)
                toCanvasGroup = toScreen.AddComponent<CanvasGroup>();

            // Налаштовуємо початковий стан
            if (toScreen != null)
            {
                toCanvasGroup.alpha = 0;
                toScreen.SetActive(true);
            }

            // Анімуємо затухання першого екрану
            float startTime = Time.time;
            while (Time.time - startTime < duration && fromCanvasGroup != null)
            {
                float t = (Time.time - startTime) / duration;
                fromCanvasGroup.alpha = 1 - t;
                await UniTask.Yield();
            }

            if (fromScreen != null)
            {
                fromCanvasGroup.alpha = 0;
                fromScreen.SetActive(false);
            }

            // Анімуємо появу другого екрану
            startTime = Time.time;
            while (Time.time - startTime < duration && toCanvasGroup != null)
            {
                float t = (Time.time - startTime) / duration;
                toCanvasGroup.alpha = t;
                await UniTask.Yield();
            }

            if (toScreen != null)
            {
                toCanvasGroup.alpha = 1;
            }
        }

        private async UniTask PlaySlideTransitionAsync(GameObject fromScreen, GameObject toScreen, bool isForward, Vector2 direction)
        {
            const float duration = 0.3f;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            RectTransform fromRect = fromScreen?.GetComponent<RectTransform>();
            RectTransform toRect = toScreen?.GetComponent<RectTransform>();

            // Налаштовуємо початковий стан
            if (toScreen != null)
            {
                Vector2 startPos = new Vector2(
                    direction.x * screenWidth * (isForward ? 1 : -1),
                    direction.y * screenHeight * (isForward ? 1 : -1)
                );
                toRect.anchoredPosition = startPos;
                toScreen.SetActive(true);
            }

            Vector2 fromStartPos = fromRect != null ? fromRect.anchoredPosition : Vector2.zero;
            Vector2 fromEndPos = new Vector2(
                fromStartPos.x - direction.x * screenWidth * (isForward ? 1 : -1),
                fromStartPos.y - direction.y * screenHeight * (isForward ? 1 : -1)
            );

            Vector2 toStartPos = toRect != null ? toRect.anchoredPosition : Vector2.zero;
            Vector2 toEndPos = Vector2.zero;

            // Анімуємо рух екранів
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;

                // Застосовуємо плавну інтерполяцію (Smooth Step)
                float smoothT = t * t * (3f - 2f * t);

                if (fromRect != null)
                    fromRect.anchoredPosition = Vector2.Lerp(fromStartPos, fromEndPos, smoothT);

                if (toRect != null)
                    toRect.anchoredPosition = Vector2.Lerp(toStartPos, toEndPos, smoothT);

                await UniTask.Yield();
            }

            // Встановлюємо кінцеві позиції
            if (fromRect != null)
                fromRect.anchoredPosition = fromEndPos;

            if (toRect != null)
                toRect.anchoredPosition = toEndPos;

            if (fromScreen != null)
                fromScreen.SetActive(false);
        }

        private async UniTask PlayScaleTransitionAsync(GameObject fromScreen, GameObject toScreen, bool isForward)
        {
            const float duration = 0.3f;

            RectTransform fromRect = fromScreen?.GetComponent<RectTransform>();
            RectTransform toRect = toScreen?.GetComponent<RectTransform>();

            // Налаштовуємо початковий стан
            if (toScreen != null)
            {
                toRect.localScale = new Vector3(0.5f, 0.5f, 1f);
                var toCanvasGroup = toScreen.GetComponent<CanvasGroup>();
                if (toCanvasGroup == null)
                    toCanvasGroup = toScreen.AddComponent<CanvasGroup>();
                toCanvasGroup.alpha = 0;
                toScreen.SetActive(true);
            }

            // Анімуємо масштабування
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;
                float smoothT = t * t * (3f - 2f * t);

                if (fromRect != null)
                {
                    fromRect.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.5f, 1.5f, 1f), smoothT);
                    var fromCanvasGroup = fromScreen.GetComponent<CanvasGroup>();
                    if (fromCanvasGroup == null)
                        fromCanvasGroup = fromScreen.AddComponent<CanvasGroup>();
                    fromCanvasGroup.alpha = 1 - smoothT;
                }

                if (toRect != null)
                {
                    toRect.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1f), Vector3.one, smoothT);
                    var toCanvasGroup = toScreen.GetComponent<CanvasGroup>();
                    toCanvasGroup.alpha = smoothT;
                }

                await UniTask.Yield();
            }

            // Встановлюємо кінцеві значення
            if (fromRect != null)
            {
                fromRect.localScale = Vector3.one;
                var fromCanvasGroup = fromScreen.GetComponent<CanvasGroup>();
                fromCanvasGroup.alpha = 1;
                fromScreen.SetActive(false);
            }

            if (toRect != null)
            {
                toRect.localScale = Vector3.one;
                var toCanvasGroup = toScreen.GetComponent<CanvasGroup>();
                toCanvasGroup.alpha = 1;
            }
        }
    }
}
