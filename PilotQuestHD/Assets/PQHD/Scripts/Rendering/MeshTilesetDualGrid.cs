using UnityEngine;
using UnityEngine.Tilemaps;

namespace PQHD
{
    [CreateAssetMenu(fileName = "MeshTileset", menuName = "PQHD/MeshTileset Dual Grid")]
    public class MeshTilesetDualGrid : MeshTileset
    {
        public TileBase tile2D;
        
        [System.Serializable]
        public class Tile
        {
            public GameObject prefab;

            [System.Serializable]
            public class Decoration
            {
                public GameObject prefab;
                public float weight;
            }

            public bool rotateDecorations = true;
            public Decoration[] decorations;

            public GameObject GetRandomDecoration(int seed)
            {
                if (decorations == null || decorations.Length == 0) return null;
                
                float sum = 0;
                for (int i = 0; i < decorations.Length; i++)
                {
                    sum += decorations[i].weight;
                }
                
                Random.InitState(seed);

                float rng = Random.value * sum;
                for (int i = 0; i < decorations.Length; i++)
                {
                    if (decorations[i].weight > rng)
                    {
                        return decorations[i].prefab;
                    }
                    else
                    {
                        rng -= decorations[i].weight;
                    }
                }

                return decorations[^1].prefab;
            }
        }

        [SerializeField] private Tile tile_1111;
        [SerializeField] private Tile tile_1101;
        [SerializeField] private Tile tile_1010;
        [SerializeField] private Tile tile_1001;
        [SerializeField] private Tile tile_1000;

        public struct TilePlacement
        {
            public Tile tile;
            public int rotation;

            public TilePlacement(Tile tile, int rotation)
            {
                this.tile = tile;
                this.rotation = rotation;
            }
        }

        public bool GetTilePlacement(byte neighbors, out TilePlacement tile)
        {
            for (int r = 0; r < 4; r++)
            {
                byte rotatedNeighbors = (byte)((neighbors >> r) | (neighbors << (4 - r)) & 0b00001111);
                if (rotatedNeighbors == 0b0000_1111)
                {
                    tile = new TilePlacement(tile_1111, r);
                    return true;
                }
                else if (rotatedNeighbors == 0b0000_1101)
                {
                    tile = new TilePlacement(tile_1101, r);
                    return true;
                }
                else if (rotatedNeighbors == 0b0000_1010)
                {
                    tile = new TilePlacement(tile_1010, r);
                    return true;
                }
                else if (rotatedNeighbors == 0b0000_1001)
                {
                    tile = new TilePlacement(tile_1001, r);
                    return true;
                }
                else if (rotatedNeighbors == 0b0000_1000)
                {
                    tile = new TilePlacement(tile_1000, r);
                    return true;
                }
            }
            
            tile = default;
            return false;
        }

        /// <summary>
        /// Get a tile. 
        /// </summary>
        /// <param name="neighbors">Bits of neighbor byte are 0000_(NW)(NE)(SE)(SW)</param>
        public bool CreateTile(byte neighbors, int seed, out GameObject tile)
        {
            if (GetTilePlacement(neighbors, out TilePlacement placement) && placement.tile != null && placement.tile.prefab != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    tile = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(placement.tile.prefab);
                }
                else
#endif
                {
                    tile = Instantiate(placement.tile.prefab);
                }
                tile.transform.localRotation = Quaternion.Euler(0, placement.rotation * -90 + 180, 0);
                GameObject decoration = placement.tile.GetRandomDecoration(seed);
                if (decoration)
                {
                    GameObject decoInst;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        decoInst = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(decoration, tile.transform);
                    }
                    else
#endif
                    {
                        decoInst = Instantiate(decoration, tile.transform);
                    }
                    decoInst.transform.localPosition = Vector3.zero;
                    if (placement.tile.rotateDecorations) decoInst.transform.localRotation = Quaternion.identity;
                    else decoInst.transform.localRotation = Quaternion.Inverse(tile.transform.localRotation);
                }

                return true;
            }

            tile = null;
            return false;
        }
    }
}