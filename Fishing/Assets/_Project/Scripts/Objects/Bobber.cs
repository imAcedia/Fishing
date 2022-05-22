
using Acedia;
using UnityEngine;

namespace Fishing
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Bobber : MonoBehaviour
    {
        const QueryTriggerInteraction collideTrigger = QueryTriggerInteraction.Collide;
        const QueryTriggerInteraction ignoreTrigger = QueryTriggerInteraction.Ignore;

        #region Components
        private Rigidbody _rigidbody = null;
        public Rigidbody Rigidbody
        {
            protected set => _rigidbody = value;
            get
            {
                if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
                return _rigidbody;
            }
        }

        private SphereCollider _sphereCollider = null;
        public SphereCollider SphereCollider
        {
            protected set => _sphereCollider = value;
            get
            {
                if (_sphereCollider == null) _sphereCollider = GetComponent<SphereCollider>();
                return _sphereCollider;
            }
        }

        #endregion

        #region Object References

        #endregion

        #region Parameters
        [Header("Parameters")]

        #region Throwing
        [SerializeField]
        private bool useCustomGravity = false;

        [SerializeField, ShowIf(nameof(useCustomGravity))]
        private Vector3 _gravity = Vector3.down * 9.8f;

        protected Vector3 Gravity => useCustomGravity ? _gravity : Physics.gravity;

        [field: SerializeField]
        public LayerMask WaterMask { get; protected set; } = -1;

        [field: SerializeField]
        public LayerMask FailMask { get; protected set; } = -1;
        #endregion // Throwing

        #region Animations
        [field: SerializeField, Space]
        public AnimationCurve IdleCurve { get; protected set; }
        [field: SerializeField]
        public float IdleScale { get; protected set; } = .2f;
        [field: SerializeField]
        public float IdleSpeed { get; protected set; } = 1f;


        [field: SerializeField, Space]
        public AnimationCurve BiteCurve { get; protected set; }
        [field: SerializeField]
        public float BiteScale { get; protected set; } = .2f;
        [field: SerializeField]
        public float BiteSpeed { get; protected set; } = 1f;
        #endregion // Animations

        #endregion

        #region States
        protected bool isThrown = false;
        protected Vector3 velocity;

        protected Collider[] overlaps = new Collider[5];
        protected bool onWater = false;
        protected bool onFail = false;

        protected Vector3 surfacePosition;
        protected float curveStartTime = -1f;
        protected float curveWeight = 0f;
        protected bool inBite = false;
        #endregion

        #region Events
        public event System.Action<Water, Collider> OnOverlapEnter; 
        #endregion

        public void Throw(Vector3 velocity)
        {
            if (isThrown)
            {
                Debug.LogWarning("Bobber cannot be thrown more than once. It needs to be reset first.", this);
                return;
            }

            Debug.Log("Thrown bobber", this);
            this.velocity = velocity;
            isThrown = true;

            curveStartTime = -1f;
        }

        public bool LandOnWaterSurface()
        {
            Debug.Log("Bobber landed on water surface.");
            isThrown = false;

            Vector3 position = Rigidbody.position + SphereCollider.center;

            surfacePosition = GetSurfacePosition(position);
            curveStartTime = Time.time;
            return true;

            Vector3 GetSurfacePosition(Vector3 position, int iteration = 0)
            {
                Ray ray = new(position, Vector3.down);
                float radius = SphereCollider.radius;

                if (!Physics.SphereCast(ray, radius, out RaycastHit hit, radius * 2f, WaterMask, collideTrigger))
                {
                    if (iteration > 99) return Rigidbody.position;
                    return GetSurfacePosition(position + Vector3.up * radius, ++iteration);
                }

                return hit.point;
            }
        }

        public void FishBite(bool inBite)
        {
            this.inBite = inBite;
            curveStartTime = Time.time;
        }

        private void Update()
        {
            if (curveStartTime <= 0f) return;

            if (inBite) curveWeight = 1f;
            else curveWeight = Mathf.MoveTowards(curveWeight, 0f, 2f * Time.deltaTime);

            float t = Time.time - curveStartTime;
            t *= Mathf.Lerp(IdleSpeed, BiteSpeed, curveWeight);
            float scale = Mathf.Lerp(IdleScale, BiteScale, curveWeight);

            float biteY = BiteCurve.Evaluate(inBite ? t : 1f);
            float idleY = IdleCurve.Evaluate(t);
            float finalY = Mathf.Lerp(idleY, biteY, curveWeight) * scale;

            Vector3 position = surfacePosition;
            position.y += finalY;
            transform.position = position;
        }

        private void FixedUpdate()
        {
            if (isThrown) UpdateThrow();
            return;

            void UpdateThrow()
            {
                Vector3 position = Rigidbody.position;
                Vector3 acceleration = Gravity;

                position += velocity * Time.deltaTime;
                velocity += acceleration * Time.deltaTime;

                Rigidbody.MovePosition(position);
                CheckOverlap();
                return;

                bool CheckOverlap()
                {
                    Vector3 pos = Rigidbody.position + SphereCollider.center;
                    LayerMask mask = WaterMask | FailMask;
                    float radius = SphereCollider.radius;

                    bool onWaterPrev = onWater;
                    bool onFailPrev = onFail;

                    int count = Physics.OverlapSphereNonAlloc(pos, radius, overlaps, mask, collideTrigger);
                    for (int i = 0; i < count; i++)
                    {
                        Collider collider = overlaps[i];
                        if (collider.attachedRigidbody == Rigidbody) continue;

                        onWater = collider.TryGetComponent(out Water water);
                        if (onWater)
                        {
                            Debug.Log($"Overlap Water: {collider.name}");
                            if (!onWaterPrev)
                            {
                                onWaterPrev = onWater;
                                OnOverlapEnter?.Invoke(water, collider);
                            }
                        }
                        else
                        {
                            Debug.Log($"Overlap Fail: {collider.name}");
                            if (!onFailPrev)
                            {
                                onFailPrev = onFail;
                                OnOverlapEnter?.Invoke(null, collider);
                            }
                        }
                    }

                    return false;
                }

                bool _OldCheckOverlap()
                {
                    Vector3 pos = Rigidbody.position + SphereCollider.center;
                    float radius = SphereCollider.radius;

                    bool onWaterPrev = onWater;
                    onWater = OverlapSphere(Rigidbody, pos, radius, overlaps, WaterMask, collideTrigger);
                    if (!onWaterPrev && onWater) OnOverlapEnter?.Invoke(null/*true*/, null);

                    bool onFailPrev = onFail;
                    onFail = OverlapSphere(Rigidbody, pos, radius, overlaps, FailMask, ignoreTrigger);
                    if (!onFailPrev && onFail) OnOverlapEnter?.Invoke(null/*false*/, null);

                    return onWater || onFail;

                    static bool OverlapSphere(Rigidbody ignoredBody, Vector3 position, float radius, Collider[] results, int mask, QueryTriggerInteraction triggerInteraction)
                    {
                        int count = Physics.OverlapSphereNonAlloc(position, radius, results, mask, triggerInteraction);

                        for (int i = 0; i < count; i++)
                        {
                            if (results[i].attachedRigidbody == ignoredBody)
                                continue;

                            return true;
                        }

                        return false;
                    }
                }
            }
        }
    }
}
