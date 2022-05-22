using UnityEngine;

namespace Fishing
{
    [System.Serializable]
    public abstract class ReelInputReader
    {
        public abstract float ReadInput(ReelInputData reelInput, out bool isValid);
    }

    [System.Serializable]
    public class ReelMouseWheel : ReelInputReader
    {
        public override float ReadInput(ReelInputData reelInput, out bool isValid)
        {
            isValid = reelInput.wheel != 0f;
            return -reelInput.wheel;
        }
    }

    [System.Serializable]
    public class ReelMouseDrag : ReelInputReader
    {
        public override float ReadInput(ReelInputData reelInput, out bool isValid)
        {
            isValid = reelInput.button && reelInput.velocity.y < 0;
            return -reelInput.velocity.y;
        }
    }

    public struct ReelInputData
    {
        public float wheel;
        public bool button;
        public Vector2 velocity;
    }
}
