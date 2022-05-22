using UnityEngine;

namespace Fishing
{
    [System.Serializable]
    public abstract class FishFight
    {
        public FishInfo Fish { get; protected set; }
        public float InitialLineLength { get; protected set; }
        public float LineLength { get; protected set; }
        public bool Lost { get; protected set; } = false;

        #region Event
        public event System.Action OnWin;
        public event System.Action OnLost;

        protected void RaiseOnWin() => OnWin?.Invoke();
        protected void RaiseOnLost() => OnLost?.Invoke();
        #endregion

        public FishFight(FishInfo fish, float lineLength)
        {
            Fish = fish;
            LineLength = lineLength;
            InitialLineLength = lineLength;
        }

        public abstract void Reel(float power);
        public abstract void Update(float deltaTime);
    }

    public class FishFightSimple : FishFight
    {
        [field: SerializeField]
        public float ReelDifficulty { get; protected set; } = 1f;

        public FishFightSimple(FishInfo fish, float lineLength) : base(fish, lineLength) { }

        public override void Reel(float power)
        {
            if (LineLength <= 0f) return;
            
            LineLength -= power;
            if (LineLength <= 0f) RaiseOnWin();
        }

        public override void Update(float deltaTime)
        {
            LineLength += ReelDifficulty * deltaTime;
            if (LineLength > InitialLineLength)
                LineLength = InitialLineLength;
        }
    }

    public class FishFightHookTension : FishFight
    {
        [field: SerializeField, Min(0f)]
        public float ReelDifficulty { get; protected set; } = 1f;

        #region Hook Difficulties
        [field: SerializeField, Min(0f)]
        public float HookIncrease { get; protected set; } = .2f;

        [field: SerializeField, Min(0f)]
        public float HookDecrease { get; protected set; } = .15f;
        #endregion

        #region Tension Difficulties
        [field: SerializeField, Min(0f)]
        public float TensionIncrease { get; protected set; } = .1f;

        [field: SerializeField, Min(0f)]
        public float TensionDecrease { get; protected set; } = .2f;
        #endregion

        [field: SerializeField, Acedia.Slider(0f, 1f), Acedia.DisableInInspector]
        public float Hook { get; protected set; } = .8f;
        [field: SerializeField, Acedia.Slider(0f, 1f), Acedia.DisableInInspector]
        public float Tension { get; protected set; } = .5f;

        public FishFightHookTension(FishInfo fish, float lineLength) : base(fish, lineLength) { }

        public override void Reel(float power)
        {
            LineLength -= power;

            //float diffPower = power / InitialLineLength;
            Hook += HookIncrease * power;
            Tension += TensionIncrease * power;

            if (Hook > 1f) Hook = 1f;
            if (Tension > 1f)
            {
                Lost = true;
                RaiseOnLost();
            }
        }

        public override void Update(float deltaTime)
        {
            LineLength += ReelDifficulty * deltaTime;
            if (LineLength > InitialLineLength)
                LineLength = InitialLineLength;

            Hook -= HookDecrease * deltaTime;
            Tension -= TensionDecrease * deltaTime;

            if (Tension < 0f) Tension = 0f;
            if (Hook < 0f)
            {
                Lost = true;
                RaiseOnLost();
            }
        }
    }

    public enum FishFightType { Simple, HookTension }
}
