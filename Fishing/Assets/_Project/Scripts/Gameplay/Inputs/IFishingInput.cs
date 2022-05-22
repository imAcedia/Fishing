namespace Fishing
{
    public interface IFishingInput
    {
        public FishingInputState GetInputState();

        public event System.Action<bool> OnFish;
        public event System.Action<float> OnReel;
    }

    [System.Serializable]
    public struct FishingInputState
    {
        public bool fish;
        public float reel;
    }
}
