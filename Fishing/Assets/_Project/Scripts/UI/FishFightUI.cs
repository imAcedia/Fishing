using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Acedia;
using DG.Tweening;

using Text = TMPro.TextMeshProUGUI;
using System;

namespace Fishing
{
    public class FishFightUI : MonoBehaviour
    {
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        [field: SerializeField]
        public SimpleUI SimpleFight { get; protected set; } = new();

        [field: SerializeField]
        public HookTensionUI HookTensionFight { get; protected set; } = new();

        #region Fight Notif
        [field: Header("Fight Notif")]

        [field: SerializeField]
        public RectTransform FightNotif { get; protected set; }

        [field: SerializeField]
        public Text FightNotifText { get; protected set; }

        [field: SerializeField]
        public float FightNotifDuration { get; protected set; } = 1f;

        [field: SerializeField]
        public TweeningInfo FishBiteShowTween { get; protected set; }

        [field: SerializeField]
        public TweeningInfo FishBiteHideTween { get; protected set; }

        protected Tween fishBiteTween = null;
        #endregion

        private void Awake()
        {
            SimpleFight.Container.SetActive(false);
            HookTensionFight.Container.SetActive(false);

            Vector2 pivot = FightNotif.pivot; pivot.y = 1f;
            FightNotif.pivot = pivot;
            FightNotif.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Fishing.OnFightStarted += OnFightStarted;
            Fishing.OnFishCaught += OnFishCaught;
            Fishing.OnFishLost += OnFishLost;
            Fishing.OnFishReeled += OnFishReeled;
        }

        private void OnDisable()
        {
            // TODO: Fix Caught-Lost method names
            Fishing.OnFightStarted -= OnFightStarted;
            Fishing.OnFishCaught -= OnFishCaught;
            Fishing.OnFishLost -= OnFishLost;
            Fishing.OnFishReeled -= OnFishReeled;
        }

        private void OnFightStarted(Fishing fishing)
        {
            SimpleFight.StartFight(fishing);
            HookTensionFight.StartFight(fishing);
            ShowFightNotif("Fish Bites!");
        }

        private void OnFishReeled(Fishing obj)
        {
            SimpleFight.DamageOverlay.alpha = 1f;
            HookTensionFight.DamageOverlay.alpha = 1f;
        }

        private void OnFishCaught(Fishing fishing, FishInfo fish)
        {
            SimpleFight.EndFight(fishing);
            HookTensionFight.EndFight(fishing);
        }

        private void OnFishLost(Fishing fishing, FishInfo fish)
        {
            SimpleFight.EndFight(fishing);
            HookTensionFight.EndFight(fishing);

            ShowFightNotif("Fish Lost...");
        }

        private void ShowFightNotif(string text)
        {
            FightNotifText.text = text;
            FightNotif.gameObject.SetActive(true);
            if (fishBiteTween != null) fishBiteTween.Kill(true);

            Vector2 pivot = FightNotif.pivot; pivot.y = 1f;
            FightNotif.pivot = pivot;

            fishBiteTween = FightNotif.DOPivotY(0f, .5f);
            FishBiteShowTween.SetEasing(fishBiteTween);

            fishBiteTween.onComplete = () 
                => GameUtilities.DelayedCall(this, Hide, FightNotifDuration);
            return;

            void Hide()
            {
                fishBiteTween = FightNotif.DOPivotY(1f, .5f);
                FishBiteHideTween.SetEasing(fishBiteTween);
                fishBiteTween.onComplete = () => FightNotif.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            SimpleFight.Update(Fishing);
            HookTensionFight.Update(Fishing);
        }

        [System.Serializable]
        public class SimpleUI
        {
            [field: SerializeField]
            public GameObject Container { get; protected set; }

            [field: SerializeField]
            public Slider LineSlider { get; protected set; }

            [field: SerializeField]
            public CanvasGroup DamageOverlay { get; protected set; }

            public bool IsActive => Container.activeInHierarchy;

            public void StartFight(Fishing fishing)
            {
                if (fishing.CurrentFishFight is not FishFightSimple)
                    return;

                Container.SetActive(true);
                LineSlider.maxValue = fishing.LineLength;
                DamageOverlay.alpha = 1f;
            }

            public void EndFight(Fishing fishing)
            {
                if (!IsActive) return;

                Container.SetActive(false);
            }

            public void Update(Fishing fishing)
            {
                if (!IsActive) return;

                LineSlider.value = fishing.LineLength;
                DamageOverlay.alpha -= Time.deltaTime;
            }
        }

        [System.Serializable]
        public class HookTensionUI
        {
            [field: SerializeField]
            public GameObject Container { get; protected set; }

            [field: SerializeField]
            public Text LineText { get; protected set; }

            [field: SerializeField]
            public Slider HookSlider { get; protected set; }

            [field: SerializeField]
            public Slider TensionSlider { get; protected set; }

            [field: SerializeField]
            public CanvasGroup DamageOverlay { get; protected set; }

            public bool IsActive => Container.activeInHierarchy;

            public void StartFight(Fishing fishing)
            {
                if (fishing.CurrentFishFight is not FishFightHookTension)
                    return;

                Container.SetActive(true);
                DamageOverlay.alpha = 1f;
            }

            public void EndFight(Fishing fishing)
            {
                if (!IsActive) return;

                Container.SetActive(false);
            }

            public void Update(Fishing fishing)
            {
                if (!IsActive) return;
                if (fishing.CurrentFishFight is not FishFightHookTension fight)
                    return;

                float lineLength = Mathf.Max(0f, fight.LineLength);
                LineText.text = $"{lineLength:#,##0.00}";
                TensionSlider.value = fight.Tension;
                HookSlider.value = fight.Hook;

                DamageOverlay.alpha -= Time.deltaTime;
            }
        }
    }
}
