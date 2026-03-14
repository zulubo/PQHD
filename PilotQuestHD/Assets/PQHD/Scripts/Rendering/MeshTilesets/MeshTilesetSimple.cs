using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    [CreateAssetMenu(fileName = "MeshTilesetSimple", menuName = "PQHD/MeshTileset Simple")]
    public class MeshTilesetSimple : MeshTileset
    {
        [System.Serializable]
        public class Tile
        {
            public TileBase tile2D;
            public GameObject prefab;
            public bool randomRotation = false;
        }

        public Tile[] tiles;

        public bool TryGetTile(TileBase tile2D, out Tile tile)
        {
            tile = System.Array.Find(tiles, t => t.tile2D == tile2D);
            return tile != null;
        }
    }
}