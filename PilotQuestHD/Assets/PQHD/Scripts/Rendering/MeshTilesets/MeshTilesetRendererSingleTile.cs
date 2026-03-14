using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    public class MeshTilesetRendererSingleTile : MeshTilesetRenderer<MeshTilesetSingleTile>
    {
        protected override bool DependsOnAdjacentChunks => tileset && tileset.expandOneTile;

        private TileBase[] tileBuffer = new TileBase[9];
        protected override List<GameObject> CreateChunkTiles(Vector2Int min, Vector2Int max)
        {
            List<GameObject> instTiles = new();
            for (int x = min.x; x < max.x; x++)
            for (int y = min.y; y < max.y; y++)
            {
                bool filled = false;
                if (tileset.expandOneTile)
                {
                    tilemap.GetTilesBlockNonAlloc(new BoundsInt(x - 1, y - 1, 0, 3, 3, 1), tileBuffer);
                    for (int t = 0; t < 9; t++)
                    {
                        if (tileBuffer[t] != null && tileBuffer[t] == tileset.tile2D)
                        {
                            filled = true;
                            break;
                        }
                    }
                }
                else
                {
                    TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                    if (tile != null && tile == tileset.tile2D)
                    {
                        filled = true;
                    }
                }

                if (filled)
                {
                    GameObject inst;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        inst = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tileset.Tile, transform);
                    }
                    else
#endif
                    {
                        inst = Instantiate(tileset.Tile, transform);
                    }
                    inst.transform.localPosition = new Vector3((x + 0.5f) * tilemap.cellSize.x, 0, (y + 0.5f) * tilemap.cellSize.y);
                    inst.transform.localRotation = Quaternion.identity;
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