using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    [CreateAssetMenu(fileName = "MeshTileset", menuName = "PQHD/MeshTileset Single Tile")]
    public class MeshTilesetSingleTile : MeshTileset
    {
        public TileBase tile2D;

        public bool expandOneTile;
        
        [SerializeField] private GameObject tile;
        public GameObject Tile => tile;
    }
}