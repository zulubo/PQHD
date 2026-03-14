using System;
using System.Collections.Generic;
using PQHD;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    [ExecuteInEditMode]
    [SelectionBase]
    public abstract class MeshTilesetRenderer<T> : MonoBehaviour where T : MeshTileset
    {
        public Tilemap tilemap;

        public T tileset;

        public bool enableChunkMeshes;
        public int chunkSize = 16;

        [System.Serializable]
        protected class Chunk
        {
            public Vector2Int coord;
            public GameObject gameObject;
            public BoxCollider[] colliders;

            public Chunk(Vector2Int coord)
            {
                this.coord = coord;
                this.gameObject = null;
            }

            public void Destroy()
            {
                if (gameObject)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(gameObject, "Remove Chunk");
#endif
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }
        }

        [SerializeField, HideInInspector] protected List<Chunk> chunks = new List<Chunk>();


        private void OnEnable()
        {
#if UNITY_EDITOR
            Tilemap.tilemapTileChanged += TilemapTileChanged;
            Tilemap.tilemapPositionsChanged += TilemapPositionChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            Tilemap.tilemapTileChanged -= TilemapTileChanged;
            Tilemap.tilemapPositionsChanged -= TilemapPositionChanged;
#endif
        }

        protected abstract bool DependsOnAdjacentChunks { get; }


        public void UpdateAllChunks()
        {
            for (int c = 0; c < chunks.Count; c++)
            {
                chunks[c].Destroy();
            }

            chunks.Clear();

            // how many chunks in world
            Vector2Int chunkMin = new Vector2Int(Mathf.FloorToInt(tilemap.cellBounds.min.x / (float)chunkSize),
                Mathf.FloorToInt(tilemap.cellBounds.min.y / (float)chunkSize));
            Vector2Int chunkMax = new Vector2Int(Mathf.CeilToInt(tilemap.cellBounds.max.x / (float)chunkSize),
                Mathf.CeilToInt(tilemap.cellBounds.max.y / (float)chunkSize));

            for (int cx = chunkMin.x; cx < chunkMax.x; cx++)
            for (int cy = chunkMin.y; cy < chunkMax.y; cy++)
            {
                UpdateChunk(new Vector2Int(cx, cy));
            }
        }

        Vector2Int TileToChunk(Vector2Int tile)
        {
            return new Vector2Int(Mathf.FloorToInt(tile.x / (float)chunkSize),
                Mathf.FloorToInt(tile.y / (float)chunkSize));
        }

        private void TilemapTileChanged(Tilemap tilemap, Tilemap.SyncTile[] sync)
        {
            if (tilemap != this.tilemap) return;

            HashSet<Vector2Int> modifiedChunks = new HashSet<Vector2Int>();
            for (int s = 0; s < sync.Length; s++)
            {
                modifiedChunks.Add(TileToChunk(new Vector2Int(sync[s].position.x, sync[s].position.y)));
            }

            UpdateChunks(modifiedChunks);
        }

        private void TilemapPositionChanged(Tilemap tilemap, NativeArray<Vector3Int> positions)
        {
            if (tilemap != this.tilemap) return;

            HashSet<Vector2Int> modifiedChunks = new HashSet<Vector2Int>();
            for (int s = 0; s < positions.Length; s++)
            {
                modifiedChunks.Add(TileToChunk(new Vector2Int(positions[s].x, positions[s].y)));
            }

            UpdateChunks(modifiedChunks);
        }

        void UpdateChunks(HashSet<Vector2Int> modifiedChunks)
        {
            if (DependsOnAdjacentChunks)
            {
                HashSet<Vector2Int> originalChunks = new HashSet<Vector2Int>(modifiedChunks);
                foreach (Vector2Int c in originalChunks)
                {
                    if (DependsOnAdjacentChunks)
                    {
                        // modify adjacent chunks
                        modifiedChunks.Add(c + new Vector2Int(0, 1));
                        modifiedChunks.Add(c + new Vector2Int(1, 1));
                        modifiedChunks.Add(c + new Vector2Int(1, 0));
                        modifiedChunks.Add(c + new Vector2Int(1, -1));
                        modifiedChunks.Add(c + new Vector2Int(0, -1));
                        modifiedChunks.Add(c + new Vector2Int(-1, -1));
                        modifiedChunks.Add(c + new Vector2Int(-1, 0));
                        modifiedChunks.Add(c + new Vector2Int(-1, 1));
                    }
                }
            }

#if UNITY_EDITOR
            UnityEditor.Undo.SetCurrentGroupName("Update tile renderer");
            int undoGroup = UnityEditor.Undo.GetCurrentGroup();
            UnityEditor.Undo.RecordObject(this, "Update Chunks");
#endif

            foreach (Vector2Int chunk in modifiedChunks)
            {
                UpdateChunk(chunk);
            }

#if UNITY_EDITOR
            UnityEditor.Undo.CollapseUndoOperations(undoGroup);
#endif
        }


        public void UpdateChunk(Vector2Int chunkCoord)
        {
            // get tile bounds of chunk
            Vector2Int tileMin = new Vector2Int(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize);
            Vector2Int tileMax = tileMin + new Vector2Int(chunkSize, chunkSize);
            // clamp to tilemap bounds
            if (tileMin.x <= tilemap.cellBounds.min.x) tileMin.x = tilemap.cellBounds.min.x + 1;
            if (tileMin.y <= tilemap.cellBounds.min.y) tileMin.y = tilemap.cellBounds.min.y + 1;
            if (tileMax.x >= tilemap.cellBounds.max.x) tileMax.x = tilemap.cellBounds.max.x - 1;
            if (tileMax.y >= tilemap.cellBounds.max.y) tileMax.y = tilemap.cellBounds.max.y - 1;

            Chunk chunk = chunks.Find(c => c.coord == chunkCoord);
            if (chunk == null)
            {
                chunk = new Chunk(chunkCoord);
                chunks.Add(chunk);
            }

            chunk.Destroy();

            List<GameObject> instTiles = CreateChunkTiles(tileMin, tileMax);

            if (instTiles.Count == 0)
            {
                // empty chunk
                chunk.Destroy();
                chunks.Remove(chunk);
                return;
            }

            // create chunk object
            if (chunk.gameObject == null) chunk.gameObject = new GameObject($"chunk {chunk.coord.x} {chunk.coord.y}");
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(chunk.gameObject, "Create Chunk");
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(chunk.gameObject,
                UnityEditor.GameObjectUtility.GetStaticEditorFlags(gameObject));
#endif
            chunk.gameObject.layer = gameObject.layer;
            chunk.gameObject.transform.parent = transform;
            chunk.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (enableChunkMeshes)
            {
                Mesh chunkMesh = MeshCombiner.CombineMeshes(MeshCombiner.GetMeshData(instTiles.ToArray()),
                    out List<Material> materials);
                if (!chunk.gameObject.TryGetComponent(out MeshFilter mf))
                    mf = chunk.gameObject.AddComponent<MeshFilter>();
                if (!chunk.gameObject.TryGetComponent(out MeshRenderer mr))
                    mr = chunk.gameObject.AddComponent<MeshRenderer>();
                mf.sharedMesh = chunkMesh;
                mr.sharedMaterials = materials.ToArray();
#if UNITY_EDITOR
                mr.receiveGI = ReceiveGI.LightProbes;
#endif
                // clean up tiles
                for (int t = 0; t < instTiles.Count; t++)
                {
                    DestroyImmediate(instTiles[t].gameObject);
                }
            }
            else
            {
                for (int t = 0; t < instTiles.Count; t++)
                {
                    instTiles[t].transform.SetParent(chunk.gameObject.transform, false);
                }
            }

#if UNITY_EDITOR
            //UnityEditor.SceneVisibilityManager.instance.DisablePicking(chunk.gameObject, true);
#endif
        }

        protected abstract List<GameObject> CreateChunkTiles(Vector2Int min, Vector2Int max);

    }
}