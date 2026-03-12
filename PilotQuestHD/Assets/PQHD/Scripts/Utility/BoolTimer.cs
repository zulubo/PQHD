using UnityEngine;

namespace PQHD
{
    /// <summary>
    /// Sets a bool to true for a certain amount of time, then resets
    /// </summary>
    public struct BoolTimer
    {
        float resetTime;

        public bool unscaledtime;
        private float currentTime => unscaledtime ? Time.unscaledTime : Time.time;

        /// <summary>
        /// Set the value to true for time seconds
        /// </summary>
        public void Set(float time)
        {
            resetTime = Mathf.Max(resetTime, currentTime + time);
        }

        /// <summary>
        /// Set the value to true for time seconds
        /// </summary>
        public void Set(float time, bool overwrite)
        {
            if (overwrite)
                resetTime = currentTime + time;
            else
                resetTime = Mathf.Max(resetTime, currentTime + time);
        }

        /// <summary>
        /// Set the value to false and reset timer
        /// </summary>
        public void Reset()
        {
            resetTime = currentTime - 1;
        }

        public bool Value { get { return currentTime < resetTime; } }

        public static implicit operator bool(BoolTimer bt)
        {
            return bt.Value;
        }
    }
}
