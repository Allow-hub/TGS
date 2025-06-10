using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshPro))]
public class TMP_Extrude3D : MonoBehaviour
{
    public float depth = 0.1f;

    void Start()
    {
        ExtrudeText();
    }

    void ExtrudeText()
    {
        var tmp = GetComponent<TextMeshPro>();
        tmp.ForceMeshUpdate();

        Mesh baseMesh = tmp.mesh;
        Vector3[] baseVertices = baseMesh.vertices;
        int[] baseTriangles = baseMesh.triangles;

        int count = baseVertices.Length;
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        // 表面
        newVertices.AddRange(baseVertices);
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            newTriangles.Add(baseTriangles[i]);
            newTriangles.Add(baseTriangles[i + 1]);
            newTriangles.Add(baseTriangles[i + 2]);
        }

        // 裏面
        for (int i = 0; i < count; i++)
        {
            Vector3 v = baseVertices[i];
            v.z -= depth;
            newVertices.Add(v);
        }
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            newTriangles.Add(baseTriangles[i + 2] + count);
            newTriangles.Add(baseTriangles[i + 1] + count);
            newTriangles.Add(baseTriangles[i] + count);
        }

        // 側面（エッジごとに面を作成）
        Dictionary<(int, int), int> edgeDict = new Dictionary<(int, int), int>();
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            int[] tri = { baseTriangles[i], baseTriangles[i + 1], baseTriangles[i + 2] };
            for (int j = 0; j < 3; j++)
            {
                int a = tri[j];
                int b = tri[(j + 1) % 3];
                var edge = (Mathf.Min(a, b), Mathf.Max(a, b));
                if (edgeDict.ContainsKey(edge))
                    edgeDict[edge]++;
                else
                    edgeDict[edge] = 1;
            }
        }
        // 外周エッジのみ側面を作る
        foreach (var kv in edgeDict)
        {
            if (kv.Value == 1)
            {
                int a = kv.Key.Item1;
                int b = kv.Key.Item2;
                int a2 = a + count;
                int b2 = b + count;

                // 側面の2つの三角形
                newTriangles.Add(a);
                newTriangles.Add(b);
                newTriangles.Add(b2);

                newTriangles.Add(a);
                newTriangles.Add(b2);
                newTriangles.Add(a2);
            }
        }

        // メッシュ生成
        Mesh mesh = new Mesh();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();

        GameObject meshObj = new GameObject("ExtrudedTMPMesh", typeof(MeshFilter), typeof(MeshRenderer));
        meshObj.transform.SetParent(transform, false);
        meshObj.transform.localPosition = Vector3.zero;
        meshObj.transform.localRotation = Quaternion.identity;
        meshObj.transform.localScale = Vector3.one;
        meshObj.GetComponent<MeshFilter>().mesh = mesh;
        meshObj.GetComponent<MeshRenderer>().material = tmp.fontMaterial;
    }
}