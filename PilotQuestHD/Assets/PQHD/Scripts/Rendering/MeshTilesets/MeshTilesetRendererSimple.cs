using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace PQHD
{
    public class MeshTilesetRendererSimple : MeshTilesetRenderer<MeshTilesetSimple>
    {
        protected override bool DependsOnAdjacentChunks => false;

        protected override List<GameObject> CreateChunkTiles(Vector2Int min, Vector2Int max)
        {
            List<GameObject> instTiles = new();
            for (int x = min.x; x < max.x; x++)
            for (int y = min.y; y < max.y; y++)
            {
                Vector3Int coord = new Vector3Int(x, y, 0);
                TileBase tile2D = tilemap.GetTile(coord);
                if (tileset.TryGetTile(tile2D, out MeshTilesetSimple.Tile tile))
                {
                    GameObject inst;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        inst = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tile.prefab, transform);
                    }
                    else
#endif
                    {
                        inst = Instantiate(tile.prefab, transform);
                    }
                    inst.transform.localPosition = new Vector3((x + 0.5f) * tilemap.cellSize.x, 0, (y + 0.5f) * tilemap.cellSize.y);
                    Random.InitState(coord.GetHashCode());
                    inst.transform.localRotation = tile.randomRotation ? Quaternion.Euler(0, Random.Range(0, 4) * 90, 0) :  Quaternion.Euler(-90,180,0) * Quaternion.Inverse(tilemap.GetTransformMatrix(coord).rotation) * Quaternion.Euler(90,0,0);
                    instTiles.Add(inst);
                }
            }

            return instTiles;
        }

        [ContextMenu("Update all chunks")]
        void UpdateEditor()
        {
            UpdateAllChunks();
        }
    }
}