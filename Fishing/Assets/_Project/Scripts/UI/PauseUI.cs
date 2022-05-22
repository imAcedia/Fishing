using DG.Tweening;
using UnityEngine;

namespace Fishing
{
    public class Pause
    {
        public static bool IsPaused { get; protected set; } = false;
        public static event System.Action<bool> OnPauseSet;

        public static void SetPause(bool paused)
        {
            IsPaused = paused;
            OnPauseSet?.Invoke(IsPaused);
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class PauseUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup = null;
        public CanvasGroup CanvasGroup
        {
            protected set => _canvasGroup = value;
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        #region Parameters
        [field: Header("Parameters")]

        [field: SerializeField]
        public TweeningInfo OpenTweenInfo { get; protected set; }

        [field: SerializeField]
        public TweeningInfo CloseTweenInfo { get; protected set; }
        #endregion

        private void Awake()
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;

            if (Pause.IsPaused) StartPause();
        }

        private void OnEnable()
        {
            Pause.OnPauseSet += OnPauseSet;
        }

        private void OnDisable()
        {
            Pause.OnPauseSet -= OnPauseSet;
        }

        public void _ButtonPauseSet(bool isPaused)
        {
            Pause.SetPause(isPaused);
        }

        public void _ButtonQuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }

        private void OnPauseSet(bool isPaused)
        {
            if (isPaused) StartPause();
            else ClosePause();
        }

        private void StartPause()
        {
            Time.timeScale = 0f;
            Tween tween = transform.DOScale(1f, OpenTweenInfo.Duration);
            OpenTweenInfo.SetEasing(tween);
            tween.SetUpdate(true);

            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
        }

        private void ClosePause()
        {
            Tween tween = transform.DOScale(0f, CloseTweenInfo.Duration);
            CloseTweenInfo.SetEasing(tween);
            tween.onComplete = OnComplete;
            tween.SetUpdate(true);
            return;

            void OnComplete()
            {
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;

                Time.timeScale = 1f;
            }
        }
    }
}
