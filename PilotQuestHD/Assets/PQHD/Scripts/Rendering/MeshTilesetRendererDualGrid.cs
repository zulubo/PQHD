using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    public class MeshTilesetRendererDualGrid : MeshTilesetRenderer<MeshTilesetDualGrid>
    {
        protected override bool DependsOnAdjacentChunks => true;

        private readonly TileBase[] tileBuffer = new TileBase[4];

        protected override List<GameObject> CreateChunkTiles(Vector2Int min, Vector2Int max)
        {
            List<GameObject> instTiles = new();
            for (int x = min.x; x < max.x; x++)
            for (int y = min.y; y < max.y; y++)
            {
                tilemap.GetTilesBlockNonAlloc(new BoundsInt(x - 1, y - 1, 0, 2, 2, 1), tileBuffer);

                byte neighbors = 0;
                if (tileBuffer[0] != null && tileBuffer[0] == tileset.tile2D) neighbors |= 0b0000_0001;
                if (tileBuffer[1] != null && tileBuffer[1] == tileset.tile2D) neighbors |= 0b0000_0010;
                if (tileBuffer[2] != null && tileBuffer[2] == tileset.tile2D) neighbors |= 0b0000_1000;
                if (tileBuffer[3] != null && tileBuffer[3] == tileset.tile2D) neighbors |= 0b0000_0100;

                if (tileset.CreateTile(neighbors, new Vector2Int(x,y).GetHashCode(), out GameObject tileInst))
                {
                    tileInst.transform.localPosition = new Vector3((x) * tilemap.cellSize.x, 0, (y) * tilemap.cellSize.y);
                    instTiles.Add(tileInst);
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