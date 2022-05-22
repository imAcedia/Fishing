
using Acedia;
using UnityEngine;

namespace Fishing
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class FishingBobber : MonoBehaviour
    {
        #region Components
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
        #endregion

        #region Object References
        [field: Header("Object References")]

        [field: SerializeField]
        public Transform Origin { get; protected set; }

        [field: SerializeField]
        public GameObject Graphics { get; protected set; }
        #endregion

        #region Parameters
        [Header("Parameters")]

        [SerializeField]
        private bool useCustomGravity = false;

        [SerializeField, ShowIf(nameof(useCustomGravity))]
        private Vector3 _gravity = Vector3.down * 9.8f;

        protected Vector3 Gravity => useCustomGravity ? _gravity : Physics.gravity;

        [field: SerializeField]
        public string WaterTag { get; protected set; } = "Water";

        [field: SerializeField]
        public LayerMask FailMask { get; protected set; } = -1;

        [field: SerializeField]
        public float MaxThrowDuration { get; protected set; } = 3f;
        #endregion

        #region States
        protected bool resetPosition = false;

        protected float throwStartTime = -1f;
        protected bool IsThrowing { get; set; } = false;

        protected Vector3 velocity;
        protected Collider[] failColliders = new Collider[1];
        #endregion

        #region Events
        public event System.Action OnThrowSucceed;
        public event System.Action<Collider> OnThrowFailed;
        #endregion

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            enabled = false;
            Hide();
        }

        public void Show()
        {
            Graphics.SetActive(true);
        }

        public void Hide()
        {
            transform.localPosition = Origin.TransformPoint(Vector3.zero);
            Graphics.SetActive(false);
        }

        public void ResetBobber()
        {
            resetPosition = true;
            IsThrowing = false;
        }

        public void Throw(Vector3 velocity)
        {
            ResetBobber();

            enabled = true;
            this.velocity = velocity;

            transform.SetParent(null);
            IsThrowing = true;
        }

        public void FailThrow(Collider collider = null)
        {
            if (!IsThrowing) return;

            enabled = false;
            IsThrowing = false;

            OnThrowFailed?.Invoke(collider);
        }

        private void FixedUpdate()
        {
            //CheckCollision();
            UpdateBody();

            void CheckCollision()
            {
                Vector3 position = transform.TransformPoint(SphereCollider.center);
                if (Physics.OverlapSphereNonAlloc(position, SphereCollider.radius, failColliders, FailMask) > 0)
                    FailThrow(failColliders[0]);
            }

            void UpdateBody()
            {
                if (resetPosition)
                {
                    Rigidbody.MovePosition(Origin.TransformPoint(Vector3.zero));
                    resetPosition = false;
                    return;
                }

                if (!enabled) return;

                Vector3 position = Rigidbody.position;
                Vector3 acceleration = Gravity;

                position += velocity * Time.deltaTime;
                velocity += acceleration * Time.deltaTime;

                Rigidbody.MovePosition(position);
            }
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (!IsThrowing) return;
        //    if (!other.CompareTag(WaterTag)) return;

        //    // Success
        //    Debug.Log("Throw Success");
        //}
    }
}
