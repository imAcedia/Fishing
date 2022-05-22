
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fishing
{
    public class FishingInput : MonoBehaviour, IFishingInput
    {
        #region Player Inputs
        [field: SerializeField]
        public PlayerInput PlayerInput { get; protected set; }

        [SerializeField, Acedia.DisableInInspector]
        protected InputAction fishAction;
        [SerializeField, Acedia.DisableInInspector]
        protected InputAction reelWheelAction;
        [SerializeField, Acedia.DisableInInspector]
        protected InputAction reelButtonAction;
        [SerializeField, Acedia.DisableInInspector]
        protected InputAction reelVelocityAction;

        protected void InitializePlayerInput(PlayerInput playerInput)
        {
            PlayerInput = playerInput;

            fishAction = PlayerInput.actions["Fish"];
            reelWheelAction = PlayerInput.actions["ReelWheel"];
            reelButtonAction = PlayerInput.actions["ReelButton"];
            reelVelocityAction = PlayerInput.actions["ReelVelocity"];
        }
        #endregion

        #region Parameters
        [field: Header("Parameters")]

        [field: SerializeField]
        public float WheelMultiplier { get; protected set; } = 1f / 120f;

        [field: SerializeField]
        public float ReelMultiplier { get; protected set; } = 1f;
        #endregion

        #region Input States
        [SerializeField, Space, Acedia.DisableInInspector]
        protected FishingInputState state;

        public FishingInputState GetInputState() => state;
        public void ChangeInputState(FishingInputState newState)
        {
            FishingInputState oldState = state;
            state = newState;

            if (oldState.fish == newState.fish) OnFish?.Invoke(newState.fish);
            if (oldState.reel == newState.reel) OnReel?.Invoke(newState.reel);
            OnStateChanged?.Invoke();
        }
        #endregion

        #region Reel Readers
        public ReelInputReader wheelReader = new ReelMouseWheel();
        public ReelInputReader dragReader = new ReelMouseDrag();
        #endregion

        #region Events
        public event System.Action<bool> OnFish;
        public event System.Action<float> OnReel;
        public event System.Action OnStateChanged;
        #endregion

        private void Awake()
        {
            InitializePlayerInput(PlayerInput);
        }

        private void OnEnable()
        {
            fishAction.performed += OnFishAction;
            fishAction.canceled += OnFishAction;
            reelWheelAction.performed += OnReelWheelAction;
            reelVelocityAction.performed += OnReelVelocityAction;
        }

        private void OnDisable()
        {
            fishAction.performed -= OnFishAction;
            fishAction.canceled -= OnFishAction;
            reelWheelAction.performed -= OnReelWheelAction;
            reelVelocityAction.performed -= OnReelVelocityAction;
        }

        private bool ShouldCancelPointerEvent()
        {
            // HACK: Need to find better solution for checking used ui event
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        private void OnFishAction(InputAction.CallbackContext ctx)
        {
            bool value = ctx.ReadValueAsButton();
            if (value && ShouldCancelPointerEvent()) return;

            state.fish = value;
            OnFish?.Invoke(state.fish);
        }

        private void OnReelWheelAction(InputAction.CallbackContext ctx)
        {
            ReadReelData(wheelReader, new()
            {
                wheel = ctx.ReadValue<float>()
            });
        }

        private void OnReelVelocityAction(InputAction.CallbackContext ctx)
        {
            if (ShouldCancelPointerEvent()) return;

            ReadReelData(dragReader, new()
            {
                button = reelButtonAction.IsPressed(),
                velocity = ctx.ReadValue<Vector2>()
            });
        }

        private void ReadReelData(ReelInputReader reader, ReelInputData reelData)
        {
            float reel = reader.ReadInput(reelData, out bool isValid);
            if (isValid)
            {
                state.reel = reel * ReelMultiplier;
                OnReel?.Invoke(state.reel);
            }
        }
    }
}
