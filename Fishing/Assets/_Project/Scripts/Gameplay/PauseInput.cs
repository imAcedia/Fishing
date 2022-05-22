using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fishing
{
    public class PauseInput : MonoBehaviour
    {
		public void OnPause(InputValue value)
        {
            Pause.SetPause(!Pause.IsPaused);
        }
    }
}
