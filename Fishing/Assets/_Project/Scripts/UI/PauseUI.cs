using DG.Tweening;
using UnityEngine;

using Text = TMPro.TextMeshProUGUI;

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
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        private CanvasGroup _pauseGroup = null;
        public CanvasGroup CanvasGroup
        {
            protected set => _pauseGroup = value;
            get
            {
                if (_pauseGroup == null) _pauseGroup = GetComponent<CanvasGroup>();
                return _pauseGroup;
            }
        }

        #region Parameters
        [field: Header("Parameters")]

        [field: SerializeField]
        public TweeningInfo OpenTweenInfo { get; protected set; }

        [field: SerializeField]
        public TweeningInfo CloseTweenInfo { get; protected set; }
        #endregion

        #region Object References
        [field: Space]

        [field: SerializeField]
        public Text FightTypeText { get; protected set; }

        [SerializeField]
        protected Text[] ReelInputTexts;
        #endregion

        private void Awake()
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;

            if (Pause.IsPaused) StartPause();
            else transform.localScale = Vector3.zero;
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

        public void _ButtonToggleFight()
        {
            Fishing.FightType = Fishing.FightType switch
            {
                FishFightType.Simple => FishFightType.HookTension,
                FishFightType.HookTension => FishFightType.Simple,
                _ => FishFightType.Simple,
            };

            UpdateOptions();
        }

        public void _ButtonQuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }

        public void _ButtonSetReel(int index)
        {
            if (Fishing.FishingInput is not FishingInput input) return;
            //Debug.Log($"{(int)input.ReelInputs} ^ {1 << index} = {(int)input.ReelInputs ^ (1 << index)}");
            input.ReelInputs ^= (FishingInput.ReelFlags)(1 << index);
            input.ReelInputs &= FishingInput.ReelFlags.All;

            UpdateOptions();
        }

        private void UpdateOptions()
        {
            FightTypeText.text = Fishing.FightType.ToString();

            if (Fishing.FishingInput is not FishingInput input) return;
            for (int i = 0; i < ReelInputTexts.Length; i++)
            {
                float a = (input.ReelInputs & (FishingInput.ReelFlags)(1 << i)) != 0 ?
                    1f : .5f;
                ReelInputTexts[i].alpha = a;
            }
        }

        private void OnPauseSet(bool isPaused)
        {
            if (isPaused) StartPause();
            else ClosePause();
        }

        private void StartPause()
        {
            UpdateOptions();

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
