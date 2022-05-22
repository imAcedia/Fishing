using UnityEngine;
using Acedia;
using DG.Tweening;

namespace Fishing
{
    [System.Serializable]
    public class TweeningInfo
    {
        [field: SerializeField]
        public float Duration { get; protected set; } = 1f;

        [SerializeField]
        protected bool useAnimationCurve = false;

        [field: SerializeField, HideIf(nameof(useAnimationCurve))]
        public Ease Easing { get; protected set; } = Ease.InOutQuad;

        [field: SerializeField, ShowIf(nameof(useAnimationCurve))]
        public AnimationCurve EasingCurve { get; protected set; }

        public void SetEasing(Tween tween)
        {
            if (useAnimationCurve) tween.SetEase(EasingCurve);
            else tween.SetEase(Easing);
        }
    }
}
