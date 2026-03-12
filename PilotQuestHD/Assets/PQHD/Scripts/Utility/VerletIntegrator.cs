using UnityEngine;

namespace PQHD
{
    /// <summary>
    /// 1D verlet integrator. Use for stable and fast simulations.
    /// You can directly set position and velocity, but for best results use the forces parameter in the Update function.
    /// </summary>
    public class VerletIntegrator
    {
        public float position;
        public float velocity;
        private float acceleration;
        public float Acceleration => acceleration;

        public void Update(float dt, float forces)
        {
            float new_pos = position + velocity * dt + acceleration * (dt * dt * 0.5f);
            float new_vel = velocity + (acceleration + forces) * (dt * 0.5f);
            position = new_pos;
            velocity = new_vel;
            acceleration = forces;
        }

        private float oldPosition;
        /// <summary> Derived velocity including direct position changes. Useful if you're clamping the motion. </summary>
        public float DerivedVelocity { get; private set; }
        /// <summary> Update the derived velocity estimate. Call after changing position directly. </summary>
        public void UpdateVelocityEstimate(float dt)
        {
            if (dt > 0)
            {
                DerivedVelocity = (position - oldPosition) / dt;
                oldPosition = position;
            }
        }
    }

    /// <summary>
    /// 2D verlet integrator. Use for stable and fast simulations.
    /// You can directly set position and velocity, but for best results use the forces parameter in the Update function.
    /// </summary>
    public class VerletIntegrator2
    {
        public Vector2 position;
        public Vector2 velocity;
        private Vector2 acceleration;
        public Vector2 Acceleration => acceleration;

        public void Update(float dt, Vector2 forces)
        {
            Vector2 new_pos = position + velocity * dt + acceleration * (dt * dt * 0.5f);
            Vector2 new_vel = velocity + (acceleration + forces) * (dt * 0.5f);
            position = new_pos;
            velocity = new_vel;
            acceleration = forces;
        }

        private Vector2 oldPosition;
        /// <summary> Derived velocity including direct position changes. Useful if you're clamping the motion. </summary>
        public Vector2 DerivedVelocity { get; private set; }
        /// <summary> Update the derived velocity estimate. Call after changing position directly. </summary>
        public void UpdateVelocityEstimate(float dt)
        {
            if (dt > 0)
            {
                DerivedVelocity = (position - oldPosition) / dt;
                oldPosition = position;
            }
        }
    }

    /// <summary>
    /// 3D verlet integrator. Use for stable and fast simulations.
    /// You can directly set position and velocity, but for best results use the forces parameter in the Update function.
    /// </summary>
    public class VerletIntegrator3
    {
        public Vector3 position;
        public Vector3 velocity;
        private Vector3 acceleration;
        public Vector3 Acceleration => acceleration;

        public void Update(float dt, Vector3 forces)
        {
            Vector3 new_pos = position + velocity * dt + acceleration * (dt * dt * 0.5f);
            Vector3 new_vel = velocity + (acceleration + forces) * (dt * 0.5f);
            position = new_pos;
            velocity = new_vel;
            acceleration = forces;
        }

        private Vector3 oldPosition;
        /// <summary> Derived velocity including direct position changes. Useful if you're clamping the motion. </summary>
        public Vector3 DerivedVelocity { get; private set; }
        /// <summary> Update the derived velocity estimate. Call after changing position directly. </summary>
        public void UpdateVelocityEstimate(float dt)
        {
            if (dt > 0)
            {
                DerivedVelocity = (position - oldPosition) / dt;
                oldPosition = position;
            }
        }
    }
}
