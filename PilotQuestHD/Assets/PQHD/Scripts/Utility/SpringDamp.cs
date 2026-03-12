using UnityEngine;

namespace PQHD
{
    /// <summary>
    /// Simple 1D spring damp sim
    /// </summary>
    [System.Serializable]
    public class SpringDamp
    {
        public float spring = 600;
        public float damp = 25;
        
        
        public VerletIntegrator integrator = new();

        public SpringDamp(float startPos)
        {
            integrator.position = startPos;
        }
        
        public SpringDamp(float startPos, float spring, float damp)
        {
            integrator.position = startPos;
            this.spring = spring;
            this.damp = damp;
        }

        // max timestep for stability
        private const float MaxDT = 0.01666f;
        public void Update(float setPoint, float deltaTime)
        {
            float forces = (setPoint - integrator.position) * spring;
            forces -= integrator.velocity * damp;
            integrator.Update(Mathf.Min(deltaTime, MaxDT), forces);
        }

        public void UpdateSubstepped(float setPoint, float deltaTime, int substeps)
        {
            deltaTime = Mathf.Min(deltaTime, MaxDT);
            deltaTime /= substeps;
            for(int i = 0; i < substeps; i++)
            {
                Update(setPoint, deltaTime);
            }
        }

        public void Bump(float velocity)
        {
            Velocity += velocity;
        }

        public float Position
        {
            get => integrator.position;
            set => integrator.position = value;
        }

        public float Velocity
        {
            get => integrator.velocity;
            set => integrator.velocity = value;
        }
    }
    
    /// <summary>
    /// Simple 2D spring damp sim
    /// </summary>
    [System.Serializable]
    public class SpringDamp2
    {
        public Vector2 spring = new Vector2(600, 600);
        public Vector2 damp = new Vector2(25, 25);
        
        public VerletIntegrator2 integrator = new();

        public SpringDamp2(Vector2 startPos)
        {
            integrator.position = startPos;
        }
        
        public SpringDamp2(Vector2 startPos, Vector2 spring, Vector2 damp)
        {
            integrator.position = startPos;
            this.spring = spring;
            this.damp = damp;
        }

        // max timestep for stability
        private const float MaxDT = 0.01666f;
        public void Update(Vector2 setPoint, float deltaTime)
        {
            Vector2 forces = Vector2.Scale(setPoint - integrator.position, spring);
            forces -=  Vector2.Scale(integrator.velocity, damp);
            integrator.Update(Mathf.Min(deltaTime, MaxDT), forces);
        }

        public void UpdateSubstepped(Vector2 setPoint, float deltaTime, int substeps)
        {
            deltaTime = Mathf.Min(deltaTime, MaxDT);
            deltaTime /= substeps;
            for(int i = 0; i < substeps; i++)
            {
                Update(setPoint, deltaTime);
            }
        }

        public void Bump(Vector2 velocity)
        {
            Velocity += velocity;
        }

        public Vector2 Position
        {
            get => integrator.position;
            set => integrator.position = value;
        }

        public Vector2 Velocity
        {
            get => integrator.velocity;
            set => integrator.velocity = value;
        }
    }
    
    /// <summary>
    /// Simple 3D spring damp sim
    /// </summary>
    [System.Serializable]
    public class SpringDamp3
    {
        public Vector3 spring = new Vector3(600, 600, 600);
        public Vector3 damp = new Vector3(25, 25, 25);
        
        public VerletIntegrator3 integrator = new();

        public SpringDamp3(Vector3 startPos)
        {
            integrator.position = startPos;
        }
        
        public SpringDamp3(Vector3 startPos, Vector3 spring, Vector3 damp)
        {
            integrator.position = startPos;
            this.spring = spring;
            this.damp = damp;
        }

        // max timestep for stability
        private const float MaxDT = 0.01666f;
        public void Update(Vector3 setPoint, float deltaTime)
        {
            Vector3 forces = Vector3.Scale(setPoint - integrator.position, spring);
            forces -=  Vector3.Scale(integrator.velocity, damp);
            integrator.Update(Mathf.Min(deltaTime, MaxDT), forces);
        }

        public void UpdateSubstepped(Vector3 setPoint, float deltaTime, int substeps)
        {
            deltaTime = Mathf.Min(deltaTime, MaxDT);
            deltaTime /= substeps;
            for(int i = 0; i < substeps; i++)
            {
                Update(setPoint, deltaTime);
            }
        }

        public void Bump(Vector3 velocity)
        {
            Velocity += velocity;
        }

        public Vector3 Position
        {
            get => integrator.position;
            set => integrator.position = value;
        }

        public Vector3 Velocity
        {
            get => integrator.velocity;
            set => integrator.velocity = value;
        }
    }
}
