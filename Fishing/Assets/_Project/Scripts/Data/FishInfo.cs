using UnityEngine;

namespace Fishing
{
    [CreateAssetMenu(menuName = "Game Data/Fish Info", order = AssetMenu.order)]
    public class FishInfo : ScriptableObject
    {
        [field: SerializeField]
        public string FishName { get; protected set; } = "Fish";

        [field: SerializeField]
        public Sprite FishIcon { get; protected set; }

        [field: SerializeField]
        public GameObject FishPrefab { get; protected set; }

        [field: Space]
        [field: SerializeField]
        public float WeightMin { get; protected set; } = 3f;

        [field: SerializeField]
        public float WeightMax { get; protected set; } = 5.5f;

        [field: SerializeField]
        public AnimationCurve WeightDistribution { get; protected set; }

        [field: Space]
        [field: SerializeField]
        public float ScaleMin { get; protected set; } = 1f;

        [field: SerializeField]
        public float ScaleMax { get; protected set; } = 1.2f;

        public float GetRandomWeight(int decimalCount = 1)
        {
            float t = WeightDistribution.Evaluate(Random.value);
            float w = Mathf.LerpUnclamped(WeightMin, WeightMax, t);

            float p = 1f;
            for (int i = 0; i < decimalCount; i++) p *= 10f;
            w = Mathf.Floor(w * p) / p;

            return w;
        }

        public float WeightToScale(float weight)
        {
            float d = WeightMax - WeightMin;
            if (d == 0) return 1f;

            float t = (weight - WeightMin) / d;
            return Mathf.LerpUnclamped(ScaleMin, ScaleMax, t);
        }
    }
}
