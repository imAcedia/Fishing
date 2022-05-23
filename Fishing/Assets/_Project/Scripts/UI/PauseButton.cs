using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing
{
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
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

        private CanvasGroup _canvasGroup = null;
        public CanvasGroup CanvasGroup
        {
            protected set => _canvasGroup = value;
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
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
            CanvasGroup.interactable = !isPaused;
            CanvasGroup.blocksRaycasts = !isPaused;
            CanvasGroup.alpha = !isPaused ? 1f : 0f;
        }

        public void _ButtonPressed()
        {
            Pause.SetPause(true);
        }
    }
}
