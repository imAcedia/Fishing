
using UnityEngine;

namespace Fishing
{
    public class FishingCameraTarget : MonoBehaviour
    {
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        [field: SerializeField]
        public Transform PlayerCameraRoot { get; protected set; }

        [field: SerializeField, Range(0f, 1f)]
        public float BobberPriority { get; protected set; } = .5f;

        [field: SerializeField]
        public float BlendSpeed { get; protected set; }

        private Vector3 bobberPosition = Vector3.zero;
        private float blending = 0f;

        private void Awake()
        {
            bobberPosition = PlayerCameraRoot.position;
        }

        private void LateUpdate()
        {
            if (Fishing.ActiveBobber != null)
                bobberPosition = Fishing.ActiveBobber.transform.position;

            UpdateBlending();
            transform.position = GetBlendedPosition();
        }
        private void UpdateBlending()
        {
            bool isFishing = Fishing.ActiveBobber != null && Fishing.LineLength > 0f;
            float target = isFishing ? 1f : 0f;

            if (Mathf.Approximately(blending, target)) blending = target;
            else blending = Mathf.Lerp(blending, target, BlendSpeed * Time.deltaTime);
        }

        private Vector3 GetBlendedPosition()
        {
            Vector3 rootPos = PlayerCameraRoot.position;
            Vector3 targetPos = Vector3.Lerp(rootPos, bobberPosition, BobberPriority);
            targetPos.y = rootPos.y;

            return Vector3.Lerp(rootPos, targetPos, blending);
        }
    }
}
