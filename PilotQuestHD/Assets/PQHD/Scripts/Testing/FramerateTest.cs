using System;
using UnityEngine;

namespace PQHD
{
    public class FramerateTest : MonoBehaviour
    {
        public int targetFrameRate = 60;

        private void Update()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
