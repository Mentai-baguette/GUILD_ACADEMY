using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuildAcademy.MonoBehaviours.UI
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f;

        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance => _instance;

        private bool _isTransitioning;

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
                _fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionCoroutine(sceneName));
        }

        // フェードなし即時遷移。_isTransitioningを無視する（意図的）。
        // ゲームオーバーやデバッグ用途など、演出不要な場面で使用。
        public void LoadSceneImmediate(string sceneName)
        {
            _isTransitioning = false;
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator TransitionCoroutine(string sceneName)
        {
            _isTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            // Fade out
            if (_fadeCanvasGroup != null)
            {
                yield return StartCoroutine(Fade(0f, 1f));
                _fadeCanvasGroup.blocksRaycasts = true;
            }

            // Load scene
            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (asyncOp != null && !asyncOp.isDone)
                yield return null;

            // Fade in
            if (_fadeCanvasGroup != null)
            {
                yield return StartCoroutine(Fade(1f, 0f));
                _fadeCanvasGroup.blocksRaycasts = false;
            }

            _isTransitioning = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            _fadeCanvasGroup.alpha = from;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }
            _fadeCanvasGroup.alpha = to;
        }
    }
}
