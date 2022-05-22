
using Acedia;
using UnityEngine;

namespace Fishing
{
    public static class GameUtilities
    {
        public static bool CheckUnassigned(object fieldValue, string fieldName)
        {
            if (fieldValue == null)
            {
                Debug.LogException(new UnassignedReferenceException($"Field '{fieldName}' is not assigned in the inspector."));
                return true;
            }

            return false;
        }

        public static CoroutineHandler DelayedCall(MonoBehaviour runner, System.Action callback, float delay, bool startsManually = false)
        {
            return new(runner, Delayed(delay, callback), startsManually);

            static System.Collections.IEnumerator Delayed(float delay, System.Action callback)
            {
                if (delay > 0f) yield return new WaitForSeconds(delay);
                callback?.Invoke();
            }
        }
    }
}
