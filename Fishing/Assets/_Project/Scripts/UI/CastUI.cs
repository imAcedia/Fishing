using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    public class CastUI : MonoBehaviour
    {
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        [field: SerializeField]
        public UIShakerSimple Shaker { get; protected set; }

        [field: SerializeField]
        public Slider PowerSlider { get; protected set; }

        [field: SerializeField]
        public Image PowerImage { get; protected set; }

        #region Parameters
        [field: SerializeField, Space]
        public AnimationCurve ShakeMagCurve { get; protected set; }

        [field: SerializeField]
        public Gradient PowerGradient { get; protected set; }
        #endregion

        protected bool ActiveUI => Shaker.gameObject.activeInHierarchy;

        private void Awake()
        {
            Shaker.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Fishing.OnCastStarted += OnCastStarted;
            Fishing.OnCastFinished += OnCastFinshed;
        }

        private void OnDisable()
        {
            Fishing.OnCastStarted -= OnCastStarted;
            Fishing.OnCastFinished -= OnCastFinshed;
        }

        private void OnCastStarted(Fishing fishing)
        {
            Shaker.gameObject.SetActive(true);
        }

        private void OnCastFinshed(Fishing fishing)
        {
            Shaker.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!ActiveUI) return;

            float power = Fishing.GetCastPower();
            PowerSlider.value = power;

            Color color = PowerGradient.Evaluate(power);
            PowerImage.color = color;

            float shake = ShakeMagCurve.Evaluate(power);
            Shaker.Magnitude = shake;
        }
    }
}
