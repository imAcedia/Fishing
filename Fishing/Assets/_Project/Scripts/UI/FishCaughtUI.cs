using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using Text = TMPro.TextMeshProUGUI;

namespace Fishing
{

    public class FishCaughtUI : MonoBehaviour
    {
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        #region Components
        [field: Header("Components")]
        
        [field: SerializeField]
        public Image FishIcon { get; protected set; }

        [field: SerializeField]
        public Text FishInfo { get; protected set; }
        #endregion

        #region Parameters
        [field: Header("Parameters")]
        
        [field: SerializeField]
        public TweeningInfo OpenTweenInfo { get; protected set; }

        [field: SerializeField]
        public TweeningInfo CloseTweenInfo { get; protected set; }
        #endregion

        private void Awake()
        {
            transform.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            Fishing.OnFishCaught += OnFishCaught;
        }

        private void OnDisable()
        {
            Fishing.OnFishCaught -= OnFishCaught;
        }

        private void OnFishCaught(Fishing fishing, FishInfo fish)
        {
            OpenPanel(fish);
        }

        public void OpenPanel(FishInfo fish)
        {
            FishIcon.sprite = fish.FishIcon;
            FishIcon.SetNativeSize();

            FishInfo.text = $"{fish.FishName}, {fish.GetRandomWeight():#,#.##}kg";

            Tween tween = transform.DOScale(1f, OpenTweenInfo.Duration);
            OpenTweenInfo.SetEasing(tween);
        }

        public void ClosePanel()
        {
            Tween tween = transform.DOScale(0f, CloseTweenInfo.Duration);
            CloseTweenInfo.SetEasing(tween);
            tween.onComplete = OnComplete;
            return;

            void OnComplete()
            {
                Fishing.EndFighting();
            }
        }

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Test Close")]
        private void TestClose()
        {
            Tween tween = transform.DOScale(0f, CloseTweenInfo.Duration);
            CloseTweenInfo.SetEasing(tween);
            tween.SetDelay(1f);
        }

        [ContextMenu("Test Open")]
        private void TestOpen()
        {
            Tween tween = transform.DOScale(1f, OpenTweenInfo.Duration);
            OpenTweenInfo.SetEasing(tween);
            tween.SetDelay(1f);
        }
#endif
        #endregion
    }
}
