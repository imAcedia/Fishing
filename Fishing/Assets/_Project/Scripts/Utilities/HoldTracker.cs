
namespace Fishing
{
    public class HoldTracker
    {
        public const float NaN = float.NaN;

        public float HoldStartTime { get; protected set; } = NaN;
        public float HoldEndTime { get; protected set; } = NaN;

        public float GetHoldDuration() => GetHoldDuration(HoldEndTime);
        public float GetHoldDuration(float time)
        {
            float start = HoldStartTime;
            //UnityEngine.Debug.Log($"s: {start}");
            if (float.IsNaN(start)) return NaN;

            float end = !float.IsNaN(HoldEndTime) ? HoldEndTime : time;
            //UnityEngine.Debug.Log($"e: {HoldEndTime} | {time}");
            if (float.IsNaN(end)) return NaN;

            return end - start;
        }

        public bool IsHolding { get; protected set; } = false;

        public event System.Action<HoldTracker> OnHoldStarted;
        public event System.Action<HoldTracker> OnHoldCanceled;
        public event System.Action<HoldTracker> OnHoldFinished;

        public bool BeginHold(float time, bool overrideHold = false)
        {
            if (IsHolding && overrideHold) return false;

            HoldStartTime = time;
            HoldEndTime = NaN;
            IsHolding = true;

            OnHoldStarted?.Invoke(this);
            return true;
        }

        public bool CancelHold()
        {
            if (!IsHolding) return false;
            
            IsHolding = false;

            OnHoldCanceled?.Invoke(this);
            return true;
        }

        public bool EndHold(float time)
        {
            if (!IsHolding) return false;

            HoldEndTime = time;
            IsHolding = false;

            OnHoldFinished?.Invoke(this);
            return true;
        }
    }
}
