using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Acedia;

namespace Fishing
{
    public class Fishing : MonoBehaviour
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

        [field: SerializeField]
        public Bobber BobberPrefab { get; protected set; }

        [field: SerializeField]
        public Transform BobberOrigin { get; protected set; }
        #endregion

        #region Parameters
        [field: Header("Parameters")]

        #region Casting
        [field: SerializeField]
        public float CastHoldMin { get; protected set; } = .2f;

        [field: SerializeField]
        public float CastHoldMax { get; protected set; } = 2f;

        [field: SerializeField,]
        public float CastSpeedMin { get; protected set; } = 1f;

        [field: SerializeField]
        public float CastSpeedMax { get; protected set; } = 4f;

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
        #endregion // Casting

        #region Biting
        [field: Space]

        [field: SerializeField]
        public float BiteCountdownMin { get; protected set; } = 1f;

        [field: SerializeField]
        public float BiteCountdownMax { get; protected set; } = 5f;

        [field: SerializeField]
        public float BiteDurationMin { get; protected set; } = .5f;

        [field: SerializeField]
        public float BiteDurationMax { get; protected set; } = 1f;
        #endregion

        #region Fighting
        [field: Space]

        [field: SerializeField]
        public float LineLengthMin { get; protected set; }
        [field: SerializeField]
        public float LineLengthMax { get; protected set; }

        [field: SerializeField]
        public FishFightType FightType { get; set; }

        #endregion // Fighting

        [field: SerializeField, Space]
        public AnimationCurve FishGetCurve { get; protected set; }

        #endregion

        #region States
        protected HoldTracker castHold = new();
        protected bool hasCasted = false;
        protected Bobber activeBobber = null;
        protected CoroutineHandler throwCoroutine = null;

        protected bool inBite = false;
        protected WaterInfo currentWater = null;
        protected FishInfo currentFish = null;
        protected CoroutineHandler biteCoroutine = null;

        protected float lineLength = 0f;
        protected bool isFighting = false;
#if UNITY_EDITOR
        [SerializeReference]
#endif
        protected FishFight fishFight = null;
        protected CoroutineHandler fightCoroutine = null;

        protected bool noInput = false;
        #endregion

        #region Exposed States
        public float LineLength => lineLength;
        public FishInfo CaughtFish => currentFish;
        public Bobber ActiveBobber => activeBobber;
        public FishFight CurrentFishFight => fishFight;
        #endregion

        #region Values
        public class AnimHash
        {
            public static readonly int IsFishing = Animator.StringToHash("IsFishing");
            public static readonly int IsCasting = Animator.StringToHash("IsCasting");
            public static readonly int Catch = Animator.StringToHash("Catch");
        }
        #endregion

        #region Events
        public event System.Action<Fishing> OnFishingStarted;
        public event System.Action<Fishing> OnFishingFinished;

        public event System.Action<Fishing> OnCastStarted;
        public event System.Action<Fishing> OnCastFinished;

        public event System.Action<Fishing> OnFightStarted;
        public event System.Action<Fishing> OnFishReeled;
        public event System.Action<Fishing, FishInfo> OnFishLost;
        public event System.Action<Fishing, FishInfo> OnFishCaught;
        #endregion

        private void Awake()
        {
            if (GameUtilities.CheckUnassigned(FishingInput, nameof(FishingInput))) return;
            if (GameUtilities.CheckUnassigned(Animator, nameof(Animator))) return;

            FishingInput.OnFish += OnFishInput;
            FishingInput.OnReel += OnReelInput;
        }

        private void OnFishInput(bool isPressed)
        {
            if (hasCasted)
            {
                if (isPressed)
                {
                    if (!isFighting) PullFish();
                    //else ReelFish(1f);
                }
            }
            else
            {
                if (isPressed) BeginCast();
                else EndCast();
            }
        }

        private void OnReelInput(float value)
        {
            if (isFighting) ReelFish(Mathf.Max(0f, value));
        }

        #region Casting
        public float GetCastPower() => Mathf.InverseLerp(
            CastHoldMin, CastHoldMax, castHold.GetHoldDuration(Time.time));

        public void BeginCast()
        {
            if (hasCasted) return;

            Animator.SetBool(AnimHash.IsFishing, true);
            Animator.SetBool(AnimHash.IsCasting, true);

            castHold.BeginHold(Time.time);

            OnFishingStarted?.Invoke(this);
            OnCastStarted?.Invoke(this);
        }

        public void EndCast()
        {
            if (!castHold.IsHolding) return;
            Animator.SetBool(AnimHash.IsCasting, false);

            castHold.EndHold(Time.time);
            float holdDuration = castHold.GetHoldDuration();

            if (holdDuration < CastHoldMin)
            {
                Animator.SetBool(AnimHash.IsFishing, false);
                OnFishingFinished?.Invoke(this);
            }
            else
            {
                float power = Mathf.InverseLerp(CastHoldMin, CastHoldMax, holdDuration);
                ThrowCast(power);
            }

            OnCastFinished?.Invoke(this);
        }

        private void ThrowCast(float power)
        {
            Debug.Log($"Throwing cast with {power} power.");

            lineLength = Mathf.Lerp(LineLengthMin, LineLengthMax, power);
            hasCasted = true;

            float castSpeed = Mathf.Lerp(CastSpeedMin, CastSpeedMax, power);
            Vector3 castVelocity = CastDirection * castSpeed;

            if (throwCoroutine != null) throwCoroutine.StopCoroutine();
            throwCoroutine = GameUtilities.DelayedCall(this, () => ThrowBobber(castVelocity), CastThrowDelay);

            return;

            void ThrowBobber(Vector3 castVelocity)
            {
                activeBobber = Instantiate(BobberPrefab, BobberOrigin.position, Quaternion.identity);
                activeBobber.Throw(castVelocity);
                activeBobber.OnOverlapEnter += CastLand;
            }
        }

        private void ClearBobber()
        {
            Debug.Log("Cleared Bobber");
            if (throwCoroutine != null) throwCoroutine.StopCoroutine();

            if (activeBobber != null)
            {
                Destroy(activeBobber.gameObject);
                activeBobber = null;
            }
        }

        private void CastLand(Water water, Collider collider)
        {
            activeBobber.OnOverlapEnter -= CastLand;

            bool onWater = water != null;
            if (!onWater || !activeBobber.LandOnWaterSurface())
            {
                Animator.SetBool(AnimHash.IsFishing, false);
                hasCasted = false;
                ClearBobber();

                OnFishingFinished?.Invoke(this);
                return;
            }

            currentWater = water.WaterInfo;

            if (currentWater == null || currentWater.AvailableFishCount <= 0) return;
            StartBiteCycle();
        }
        #endregion

        #region Biting
        public void StartBiteCycle()
        {
            if (biteCoroutine != null) biteCoroutine.StopCoroutine();
            biteCoroutine = new(this, BiteCoroutine());
            return;

            IEnumerator BiteCoroutine()
            {
                while (true)
                {
                    float wait = Random.Range(BiteCountdownMin, BiteCountdownMax);
                    if (wait > 0f) yield return new WaitForSeconds(wait);
                    
                    Debug.Log("Fish Bites!");
                    if (activeBobber != null) activeBobber.FishBite(true);
                    currentFish = currentWater.GetRandomFish();
                    inBite = true;

                    wait = Random.Range(BiteDurationMin, BiteDurationMax);
                    if (wait > 0f) yield return new WaitForSeconds(wait);

                    Debug.Log("Fish Bite Lost...");
                    if (activeBobber != null) activeBobber.FishBite(false);
                    inBite = false;
                }
            }
        }

        public void PullFish()
        {
            if (!hasCasted) return;
            if (isFighting) return;

            if (biteCoroutine != null) biteCoroutine.StopCoroutine();

            if (!inBite)
            {
                Debug.Log("Pulled Nothing");

                Animator.SetBool(AnimHash.IsFishing, false);
                hasCasted = false;
                ClearBobber();
                
                OnFishingFinished?.Invoke(this);
                return;
            }

            Debug.Log("Fish Pulled!");
            StartFighting();
        }
        #endregion

        #region Fighting
        private void StartFighting()
        {
            Animator.SetTrigger(AnimHash.Catch);

            //currentFish = currentWater.GetRandomFish();
            isFighting = true;
            fishFight = FightType switch
            {
                FishFightType.HookTension => new FishFightHookTension(currentFish, lineLength),
                _ => new FishFightSimple(currentFish, lineLength),
            };

            if (fightCoroutine != null) fightCoroutine.StopCoroutine();
            fightCoroutine = new(this, FightCoroutine());

            OnFightStarted.Invoke(this);
            return;

            IEnumerator FightCoroutine()
            {
                bool fightLoop = true;

                while (fightLoop)
                {
                    yield return null;

                    if (!fishFight.Lost && fishFight.LineLength > 0f)
                    {
                        lineLength = fishFight.LineLength;
                        fishFight.Update(Time.deltaTime);
                    }
                    else fightLoop = false;
                }

                if (fishFight.Lost) LostFish();
                else CatchFish();

                lineLength = 0f;
                fishFight = null;
            }
        }

        private void ReelFish(float power = 1f)
        {
            if (fishFight == null) return;

            fishFight.Reel(power);
            OnFishReeled?.Invoke(this);
        }

        private void CatchFish()
        {
            Animator.SetBool(AnimHash.IsFishing, false);
            Debug.Log("Fish CAUGHT!");

            if (currentFish.FishPrefab == null) EndCatch();
            else new CoroutineHandler(this, AnimateFish(currentFish));
            return;

            IEnumerator AnimateFish(FishInfo fish)
            {
                GameObject fishObject = Instantiate(fish.FishPrefab, activeBobber.transform.position, transform.rotation);
                Transform fishTransform = fishObject.transform;
                Vector3 startPos = fishTransform.position;
                Vector3 endPos = BobberOrigin.position;

                float t = 0f;
                while (t < 1f)
                {
                    Vector3 position = Vector3.Lerp(startPos, endPos, t);
                    position.y += FishGetCurve.Evaluate(t);

                    fishTransform.position = position;
                    t += Time.deltaTime * 2f;
                    yield return null;
                }

                Destroy(fishObject);
                EndCatch();
            }

            void EndCatch()
            {
                OnFishCaught?.Invoke(this, currentFish);
                //EndFighting();
            }
        }

        private void LostFish()
        {
            Animator.SetBool(AnimHash.IsFishing, false);
            Debug.Log("Fish LOST!");

            OnFishLost?.Invoke(this, currentFish);
            EndFighting();
        }

        public void EndFighting()
        {
            currentFish = null;

            inBite = false;
            isFighting = false;
            hasCasted = false;
            lineLength = 0f;

            ClearBobber();

            OnFishingFinished?.Invoke(this);
        }
        #endregion
    }
}
