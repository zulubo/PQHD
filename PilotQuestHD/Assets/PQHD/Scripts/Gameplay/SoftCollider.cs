using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands.Import;
using UnityEngine;
using UnityEngine.AI;

namespace PQHD
{
    /// <summary>
    /// Softly collides with other SoftColliders.
    /// Operates in top down 2D
    /// Would be nice to add height checks for cylindrical colliders
    /// </summary>
    public class SoftCollider : MonoBehaviour
    {
        public static Dictionary<Vector2Int, List<SoftCollider>> grid = new();
        private const float GridCellSize = 10;

        [SerializeField] private float radius = 0.5f;
        //[SerializeField] private float height = 1;
        [SerializeField] private float selfStrengthMul = 1;
        [SerializeField] private float otherStrengthMul = 1;
        private const float StrengthConst = 5;

        private Vector2Int inCell;
        
        /// <summary>
        /// Read these forces for custom moving
        /// </summary>
        public Vector3 Forces { get; private set; }

        enum SelfMoveMode
        {
            None,
            Kinematic,
            NavMeshAgent
        }
        
        [SerializeField] private SelfMoveMode selfMoveMode = SelfMoveMode.None;

        private Rigidbody rigidbody;
        private NavMeshAgent navMeshAgent;


        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            ChangeCell(GetInCell());
        }

        private void OnDisable()
        {
            RemoveFromCell(inCell);
        }

        private void Update()
        {
            // update location in grid
            Vector2Int newCell = GetInCell();
            if (newCell != inCell) ChangeCell(newCell);
            
            // calculate forces
            // collide with other colliders in nearby cells
            Forces = Vector3.zero;
            for (int x = inCell.x - 1; x <= inCell.x + 1; x++)
            for (int y = inCell.y - 1; y <= inCell.y + 1; y++)
            {
                if (grid.TryGetValue(new Vector2Int(x, y), out List<SoftCollider> colliders))
                {
                    foreach (SoftCollider col in colliders)
                    {
                        if (col == this) continue;
                        
                        Vector2 offset = new Vector2(transform.position.x, transform.position.z)
                                         - new Vector2(col.transform.position.x, col.transform.position.z);
                        float distSqr = offset.sqrMagnitude;
                        float collideDist = radius + col.radius;
                        if (distSqr < collideDist * collideDist)
                        {
                            // colliding
                            float dist = Mathf.Sqrt(distSqr);
                            float overlap = collideDist - dist;
                            float force = (overlap / radius) * selfStrengthMul * col.otherStrengthMul * StrengthConst;
                            Vector2 dir = offset / dist;
                            Vector2 forces = dir * force;
                            Forces += new Vector3(forces.x, 0, forces.y);
                        }
                    }
                }
            }            
            
            // apply self move
            if (selfMoveMode == SelfMoveMode.NavMeshAgent)
            {
                navMeshAgent.nextPosition += Forces * Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (selfMoveMode == SelfMoveMode.Kinematic)
            {
                rigidbody.MovePosition(rigidbody.position + Forces * Time.fixedDeltaTime);
            }
        }

        private Vector2Int GetInCell()
        {
            return new Vector2Int(Mathf.RoundToInt(transform.position.x / GridCellSize), Mathf.RoundToInt(transform.position.y / GridCellSize));
        }

        private void ChangeCell(Vector2Int newCell)
        {
            Vector2Int oldCell = inCell;
            inCell = newCell;
            RemoveFromCell(oldCell);
            AddToCell(inCell);
        }

        private void RemoveFromCell(Vector2Int cell)
        {
            if (grid.TryGetValue(cell, out List<SoftCollider> list))
            {
                if(list.Contains(this)) list.Remove(this);
            }
        }

        private void AddToCell(Vector2Int cell)
        {
            if (!grid.TryGetValue(cell, out List<SoftCollider> list))
            {
                list = new List<SoftCollider>();
                grid[cell] = list;
            }
            if(!list.Contains(this)) list.Add(this);
        }
    }
}
