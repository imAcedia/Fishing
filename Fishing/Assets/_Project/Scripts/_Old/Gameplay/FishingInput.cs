using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Fishing
{
    public class FishingInput : MonoBehaviour, IFishingInput
    {
        protected FishingInputState state;

        public event System.Action OnInputStateChanged;
        public event System.Action<bool> OnFishActuated;
        public event System.Action<float> OnReelActuated;

        public FishingInputState GetInputState() => state;
        public void ChangeInputState(FishingInputState newState)
        {
            FishingInputState oldState = state;
            state = newState;

            if (oldState.fish == newState.fish) OnFishActuated?.Invoke(newState.fish);
            if (oldState.reel == newState.reel) OnReelActuated?.Invoke(newState.reel);
            OnInputStateChanged?.Invoke();
        }

#if ENABLE_INPUT_SYSTEM
        public void OnFish(InputValue value)
        {
            state.fish = value.isPressed;
            OnFishActuated?.Invoke(state.fish);
        }

        public void OnReelWheel(InputValue value)
        {
            state.reel = value.Get<float>();
            OnReelActuated?.Invoke(state.reel);
        }
#endif
    }

    public interface IFishingInput
    {
        public FishingInputState GetInputState();

        public event System.Action<bool> OnFishActuated;
        public event System.Action<float> OnReelActuated;
    }

    public struct FishingInputState
    {
        public bool fish;
        public float reel;
    }
}
