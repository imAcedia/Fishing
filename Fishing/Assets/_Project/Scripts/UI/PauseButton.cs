using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    [RequireComponent(typeof(Button))]
    public class PauseButton : MonoBehaviour
    {
        private Button _button = null;
        public Button Button
        {
            protected set => _button = value;
            get
            {
                if (_button == null) _button = GetComponent<Button>();
                return _button;
            }
        }

        private void OnEnable()
        {
            Pause.OnPauseSet += OnPauseSet;
        }

        private void OnDisable()
        {
            Pause.OnPauseSet -= OnPauseSet;
        }

        private void OnPauseSet(bool isPaused)
        {
            Button.interactable = !isPaused;
        }

        public void _ButtonPressed()
        {
            Pause.SetPause(true);
        }
    }
}
