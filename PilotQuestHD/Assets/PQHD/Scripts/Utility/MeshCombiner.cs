using System.Collections.Generic;
using UnityEngine;

// ===============================================================
// Methods for thouroughly and efficiently combining multiple meshes into one for performance.
// Properly handles multiple materials
// ===============================================================
public class MeshCombiner
{
    [System.Serializable]
    public class MeshData
    {
        public Mesh mesh;
        public Material[] materials;
        public Matrix4x4 transform = Matrix4x4.identity;
        public MeshData(Mesh mesh, Material[] materials, Matrix4x4 transform)
        {
            this.mesh = mesh;
            this.materials = materials;
            this.transform = transform;
        }
    }

    static Mesh CombineMeshes(List<CombineInstance> combiners, bool mergeSubmeshes)
    {
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combiners.ToArray(), mergeSubmeshes);
        return mesh;
    }

    /// <summary>
    /// Get mesh data from gameobjects for combining
    /// </summary>
    public static MeshData[] GetMeshData(GameObject[] gameObjects)
    {
        List<MeshData> data = new List<MeshData>();
        List<MeshRenderer> scannedRenderers = new List<MeshRenderer>();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            MeshRenderer[] childRenderers = gameObjects[i].GetComponentsInChildren<MeshRenderer>();
            for (int c = 0; c < childRenderers.Length; c++)
            {
                MeshRenderer mr = childRenderers[c];
                if (mr == null || scannedRenderers.Contains(mr)) continue;
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                if (mf == null) continue;
                data.Add(new MeshData(mf.sharedMesh, mr.sharedMaterials, mf.transform.localToWorldMatrix));
                scannedRenderers.Add(mr);
            }
        }

        return data.ToArray();
    }

    /// <summary>
    /// Combines meshes
    /// </summary>
    public static Mesh CombineMeshes(MeshData[] meshes, out List<Material> materials, bool ignoreTransparentMaterials = false)
    {
        var mesh = new Mesh();
        materials = null;

        if (meshes.Length == 0)
            return mesh;

        // get shared materials so we can merge them
        materials = new List<Material>();
        for (int i = 0; i < meshes.Length; i++)
        {
            for (int m = 0; m < meshes[i].materials.Length; m++)
            {
                if (!materials.Contains(meshes[i].materials[m]))
                {
                    materials.Add(meshes[i].materials[m]);
                }
            }
        }

        List<Mesh> submeshes = new List<Mesh>();
        for (int i = 0; i < materials.Count; i++)
        {
            if(ignoreTransparentMaterials && materials[i].renderQueue > (int)UnityEngine.Rendering.RenderQueue.GeometryLast)
            {
                continue;
            }

            List<CombineInstance> combiners = new List<CombineInstance>();
            List<UnityEngine.Rendering.SubMeshDescriptor> subDescs = new List<UnityEngine.Rendering.SubMeshDescriptor>();
            for (int m = 0; m < meshes.Length; m++)
            {
                for (int mat = 0; mat < meshes[m].materials.Length; mat++)
                {
                    if (meshes[m].materials[mat] != materials[i])
                        continue;

                    CombineInstance ci = new CombineInstance();
                    ci.mesh = meshes[m].mesh;
                    ci.subMeshIndex = mat;
                    ci.transform = meshes[m].transform;
                    combiners.Add(ci);
                    subDescs.Add(meshes[m].mesh.GetSubMesh(mat));
                }
            }

            Mesh subMesh = CombineMeshes(combiners, true);

            submeshes.Add(subMesh);
        }

        List<CombineInstance> finalcombiners = new List<CombineInstance>();
        for (int i = 0; i < submeshes.Count; i++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = submeshes[i];
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalcombiners.Add(ci);
        }

        mesh = CombineMeshes(finalcombiners, false);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    public static UnityEngine.Rendering.SubMeshDescriptor GetSubmesh(CombineInstance ci)
    {
        return ci.mesh.GetSubMesh(ci.subMeshIndex);
    }
}
