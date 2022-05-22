using System.Collections;

using UnityEngine;

using Acedia;

namespace Fishing
{
    public class FishingPlayer : MonoBehaviour
    {
        #region Components
        [Header("Components")]

        [SerializeField, MustInherit(typeof(IFishingInput))]
        private Object _fishingInput = default;
        public IFishingInput FishingInput => _fishingInput as IFishingInput;

        [field: SerializeField]
        public Animator Animator { get; protected set; }
        #endregion

        #region Object References
        [field: Header("Object References")]
        public FishingBobber bobber;
        #endregion

        #region Parameters
        [field: Header("Parameters")]

        // Casting
        [field: SerializeField]
        public float CastHoldMin { get; protected set; } = .2f;

        [field: SerializeField]
        public float CastHoldMax { get; protected set; } = 2f;

        [field: SerializeField,]
        public float CastSpeedMin { get; protected set; }

        [field: SerializeField]
        public float CastSpeedMax { get; protected set; }

        [field: SerializeField, Range(0f, 90f)]
        public float CastAngle { get; protected set; } = 45f;
        protected float CastAngleRad => CastAngle * Mathf.Deg2Rad;

        // Should be cached
        protected Vector3 CastDirection =>
            Quaternion.AngleAxis(CastAngle, -transform.right) * transform.forward;

        [field: SerializeField, Min(0f)]
        public float CastThrowDelay { get; protected set; } = 20f / 30f;

        [field: SerializeField, Min(.01f)]
        public float CastMaxThrowDuration { get; protected set; } = 3f;
        #endregion

        #region States
        // Casting
        protected float castStartTime = 0f;
        protected bool isCasting = false;
        protected bool hasCasted = false;

        // Fighting
        protected bool inBite = false;
        protected float lineLength = 0f;

        private CoroutineHandler throwCastCoroutine;
        #endregion

        #region Values
        public class AnimHash
        {
            public static readonly int IsFishing = Animator.StringToHash("IsFishing");
            public static readonly int IsCasting = Animator.StringToHash("IsCasting");
            public static readonly int Bite = Animator.StringToHash("Bite");
        }
        #endregion

        private void Awake()
        {
            if (FishingInput == null)
            {
                Debug.LogException(UnassignedEx(nameof(FishingInput)));
                return;
            }
            if (Animator == null)
            {
                Debug.LogException(UnassignedEx(nameof(Animator)));
                return;
            }

            FishingInput.OnFishActuated += OnFishInput;

            static UnassignedReferenceException UnassignedEx(string fieldName) => new UnassignedReferenceException($"{fieldName} field is not assigned in the inspector.");
        }

        private void OnFishInput(bool isPressed)
        {
            //Debug.Log($"On Fish Input {isPressed}");

            if (isPressed)
            {
                if (hasCasted) PullFish();
                else StartCast();
            }
            else ThrowCast();
        }

        protected void StartCast()
        {
            Animator.SetBool(AnimHash.IsFishing, true);
            Animator.SetBool(AnimHash.IsCasting, true);

            castStartTime = Time.time;
            isCasting = true;
        }

        protected void CancelCast()
        {
            Animator.SetBool(AnimHash.IsFishing, false);
            Animator.SetBool(AnimHash.IsCasting, false);

            if (throwCastCoroutine != null) throwCastCoroutine.StopCoroutine();
            isCasting = false;
            hasCasted = false;
            bobber.Hide();
        }

        protected void ThrowCast()
        {
            if (!isCasting) return;

            float castHold = Time.time - castStartTime;
            if (castHold < CastHoldMin)
            {
                CancelCast();
                return;
            }

            hasCasted = true;
            float castPower = Mathf.InverseLerp(CastHoldMin, CastHoldMax, castHold);

            if (throwCastCoroutine != null) throwCastCoroutine.StopCoroutine();
            throwCastCoroutine = new(this, ThrowCastCoroutine(castPower));

            return;

            #region Local Methods
            IEnumerator ThrowCastCoroutine(float castPower)
            {
                Animator.SetBool(AnimHash.IsCasting, false);
                if (CastThrowDelay > 0f) yield return new WaitForSeconds(CastThrowDelay);

                float castSpeed = Mathf.Lerp(CastSpeedMin, CastSpeedMax, castPower);
                Vector3 castVelocity = CastDirection * castSpeed;

                bobber.Show();
                bobber.Throw(castVelocity);
                bobber.OnThrowSucceed += OnThrowSucceed;
                bobber.OnThrowFailed += OnThrowFailed;

                yield return new WaitForSeconds(CastMaxThrowDuration);
                if (!isCasting) yield break;

                bobber.FailThrow();
            }
            #endregion
        }

        protected void OnThrowSucceed()
        {
            bobber.OnThrowSucceed -= OnThrowSucceed;
            bobber.OnThrowFailed -= OnThrowFailed;

            isCasting = false;

            Debug.Log($"Throw Succeeded");
        }

        protected void OnThrowFailed(Collider hitCollider)
        {
            bobber.OnThrowSucceed -= OnThrowSucceed;
            bobber.OnThrowFailed -= OnThrowFailed;

            Animator.SetBool(AnimHash.IsFishing, false);

            bobber.Hide();
            bobber.ResetBobber();
            isCasting = false;
            hasCasted = false;

            string hitName = hitCollider == null ? "Nothing" : hitCollider.name;
            Debug.Log($"Throw Failed, hit {hitName}");
        }

        protected void PullFish()
        {
            if (isCasting)
            {
                CancelCast();
                return;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (bobber == null) return;

            UnityEditor.Handles.matrix = bobber.transform.localToWorldMatrix;
            UnityEditor.Handles.DrawLine(Vector3.zero, CastDirection);
        }
#endif
    }
}
