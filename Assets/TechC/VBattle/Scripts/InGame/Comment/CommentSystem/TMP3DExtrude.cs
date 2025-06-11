using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TextMeshPro))]
public class TMP_Extrude3D : MonoBehaviour
{
    [Header("Extrusion Settings")]
    public float depth = 0.1f;
    
    [Header("Material Settings")]
    public Material sideMaterial; // 側面用のマテリアル（オプション）
    
    void Start()
    {
        StartCoroutine(ExtrudeTextCoroutine());
    }
    
    IEnumerator ExtrudeTextCoroutine()
    {
        var tmp = GetComponent<TextMeshPro>();
        
        // TextMeshProの初期化を待つ
        yield return null;
        tmp.ForceMeshUpdate();
        yield return null;
        
        if (tmp.mesh == null || tmp.mesh.vertexCount == 0)
        {
            Debug.LogError("TextMeshPro mesh is null or empty.");
            yield break;
        }
        
        ExtrudeText();
    }
    
    void ExtrudeText()
    {
        var tmp = GetComponent<TextMeshPro>();
        Mesh baseMesh = tmp.mesh;
        
        Vector3[] baseVertices = baseMesh.vertices;
        int[] baseTriangles = baseMesh.triangles;
        Vector2[] baseUVs = baseMesh.uv;
        
        Debug.Log($"Base mesh: {baseVertices.Length} vertices, {baseTriangles.Length/3} triangles");
        
        // 外周を検出
        List<Vector2> outline = GetOutline(baseVertices, baseTriangles);
        Debug.Log($"Outline points: {outline.Count}");
        
        CreateExtrudedMesh(baseVertices, baseTriangles, baseUVs, outline, tmp.fontMaterial);
    }
    
    List<Vector2> GetOutline(Vector3[] vertices, int[] triangles)
    {
        // エッジの使用回数をカウント
        Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int[] tri = { triangles[i], triangles[i + 1], triangles[i + 2] };
            
            for (int j = 0; j < 3; j++)
            {
                int a = tri[j];
                int b = tri[(j + 1) % 3];
                var edge = (Mathf.Min(a, b), Mathf.Max(a, b));
                
                edgeCount[edge] = edgeCount.ContainsKey(edge) ? edgeCount[edge] + 1 : 1;
            }
        }
        
        // 外周エッジ（使用回数が1回のエッジ）を取得
        List<(int, int)> outlineEdges = edgeCount.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).ToList();
        
        // エッジを接続して外周ポリゴンを作成
        List<Vector2> outline = new List<Vector2>();
        
        if (outlineEdges.Count > 0)
        {
            // 最初のエッジから始める
            var currentEdge = outlineEdges[0];
            outline.Add(new Vector2(vertices[currentEdge.Item1].x, vertices[currentEdge.Item1].y));
            outline.Add(new Vector2(vertices[currentEdge.Item2].x, vertices[currentEdge.Item2].y));
            
            int currentVertex = currentEdge.Item2;
            outlineEdges.RemoveAt(0);
            
            // 次のエッジを順番に繋げる
            while (outlineEdges.Count > 0)
            {
                bool found = false;
                for (int i = 0; i < outlineEdges.Count; i++)
                {
                    var edge = outlineEdges[i];
                    if (edge.Item1 == currentVertex)
                    {
                        currentVertex = edge.Item2;
                        outline.Add(new Vector2(vertices[currentVertex].x, vertices[currentVertex].y));
                        outlineEdges.RemoveAt(i);
                        found = true;
                        break;
                    }
                    else if (edge.Item2 == currentVertex)
                    {
                        currentVertex = edge.Item1;
                        outline.Add(new Vector2(vertices[currentVertex].x, vertices[currentVertex].y));
                        outlineEdges.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                
                if (!found) break; // 接続できない場合は終了
            }
        }
        
        return outline;
    }
    
    void CreateExtrudedMesh(Vector3[] baseVertices, int[] baseTriangles, Vector2[] baseUVs, List<Vector2> outline, Material material)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int vertexCount = baseVertices.Length;
        
        // 前面の頂点とUV
        vertices.AddRange(baseVertices);
        uvs.AddRange(baseUVs);
        
        // 前面の三角形
        triangles.AddRange(baseTriangles);
        
        // 背面の頂点とUV
        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 v = baseVertices[i];
            v.z -= depth;
            vertices.Add(v);
            uvs.Add(baseUVs[i]);
        }
        
        // 背面の三角形（法線を反転）
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            triangles.Add(baseTriangles[i + 2] + vertexCount);
            triangles.Add(baseTriangles[i + 1] + vertexCount);
            triangles.Add(baseTriangles[i] + vertexCount);
        }
        
        // 側面を作成
        if (outline.Count > 2)
        {
            for (int i = 0; i < outline.Count; i++)
            {
                int next = (i + 1) % outline.Count;
                
                // 前面と背面の対応する頂点を見つける
                int frontIndex1 = FindVertexIndex(baseVertices, outline[i]);
                int frontIndex2 = FindVertexIndex(baseVertices, outline[next]);
                
                if (frontIndex1 != -1 && frontIndex2 != -1)
                {
                    int backIndex1 = frontIndex1 + vertexCount;
                    int backIndex2 = frontIndex2 + vertexCount;
                    
                    // 側面の四角形を2つの三角形で作成
                    // 三角形1
                    triangles.Add(frontIndex1);
                    triangles.Add(backIndex1);
                    triangles.Add(backIndex2);
                    
                    // 三角形2
                    triangles.Add(frontIndex1);
                    triangles.Add(backIndex2);
                    triangles.Add(frontIndex2);
                }
            }
        }
        
        // メッシュを作成
        Mesh extrudedMesh = new Mesh();
        extrudedMesh.name = "ExtrudedTextMesh";
        extrudedMesh.vertices = vertices.ToArray();
        extrudedMesh.triangles = triangles.ToArray();
        extrudedMesh.uv = uvs.ToArray();
        extrudedMesh.RecalculateNormals();
        extrudedMesh.RecalculateBounds();
        
        // GameObjectに適用
        GameObject extrudedObj = new GameObject("ExtrudedText");
        extrudedObj.transform.SetParent(transform, false);
        extrudedObj.transform.localPosition = Vector3.zero;
        extrudedObj.transform.localRotation = Quaternion.identity;
        extrudedObj.transform.localScale = Vector3.one;
        
        var meshFilter = extrudedObj.AddComponent<MeshFilter>();
        var meshRenderer = extrudedObj.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = extrudedMesh;
        meshRenderer.material = sideMaterial != null ? sideMaterial : material;
        
        Debug.Log($"Created extruded mesh: {vertices.Count} vertices, {triangles.Count/3} triangles");
        
        // 元のTextMeshProを非表示
        GetComponent<TextMeshPro>().enabled = false;
    }
    
    int FindVertexIndex(Vector3[] vertices, Vector2 point)
    {
        float minDistance = float.MaxValue;
        int closestIndex = -1;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector2.Distance(new Vector2(vertices[i].x, vertices[i].y), point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        
        return minDistance < 0.001f ? closestIndex : -1; // 許容誤差内の場合のみ有効
    }
}