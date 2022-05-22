using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing
{
    public class FishingBehaviourDisabler : MonoBehaviour
    {
        [field: SerializeField]
        public Fishing Fishing { get; protected set; }

        [SerializeField]
        protected List<Behaviour> ManagedBehaviour = new();

        private void OnEnable()
        {
            Fishing.OnFishingStarted += DisableComponent;
            Fishing.OnFishingFinished += EnableComponent;
        }

        private void OnDisable()
        {
            Fishing.OnFishingStarted -= DisableComponent;
            Fishing.OnFishingFinished -= EnableComponent;
        }

        private void EnableComponent(Fishing obj)
        {
            SetBehaviours(true);
        }

        private void DisableComponent(Fishing fishing)
        {
            SetBehaviours(false);
        }

        private void SetBehaviours(bool enabled)
        {
            foreach (Behaviour behaviour in ManagedBehaviour)
                behaviour.enabled = enabled;
        }
    }
}
