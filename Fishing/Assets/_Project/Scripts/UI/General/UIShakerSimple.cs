using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing
{
    public class UIShakerSimple : MonoBehaviour
    {
        [field: SerializeField, Min(0f)]
        public float Magnitude { get; set; } = 1f;

        [field: SerializeField]
        public bool IsActive { get; set; } = false;

        private void Update()
        {
            if (!IsActive) return;
            if (Magnitude < 0f) Magnitude = 0f;

            Vector2 position = transform.localPosition;
            position = Random.insideUnitCircle * Magnitude;
            transform.localPosition = position;
        }
    }
}
