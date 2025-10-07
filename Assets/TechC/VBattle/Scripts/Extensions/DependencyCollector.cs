using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Linq;

namespace TechC
{
    #region データ構造
    public class DependencyInfo
    {
        public string Owner;
        public string Referenced;
        public string FieldName;
    }
    #endregion

    #region 依存関係収集クラス
    public static class DependencyCollector
    {
        public static List<DependencyInfo> CollectSceneDependencies()
        {
            var deps = new List<DependencyInfo>();

            var allComponents = GameObject.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var comp in allComponents)
            {
                if (comp == null) continue;
                var visited = new HashSet<Type>();
                ScanFieldsRecursive(comp.GetType(), comp, deps, visited);
            }

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject)))
                    continue;

                var visited = new HashSet<Type>();
                ScanFieldsRecursive(type, null, deps, visited);
            }

            return deps;
        }

        private static void ScanFieldsRecursive(Type type, object instance, List<DependencyInfo> deps, HashSet<Type> visited)
        {
            if (visited.Contains(type)) return;
            visited.Add(type);

            if (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith("UnityEngine"))
                return;

            if (type.Name.Contains("d__") || type.Name.Contains("c__DisplayClass"))
                return;

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                if (!string.IsNullOrEmpty(fieldType.Namespace) && fieldType.Namespace.StartsWith("UnityEngine"))
                    continue;

                if (fieldType.Name.Contains("d__") || fieldType.Name.Contains("c__DisplayClass"))
                    continue;

                if (typeof(MonoBehaviour).IsAssignableFrom(fieldType) ||
                    typeof(ScriptableObject).IsAssignableFrom(fieldType) ||
                    (!fieldType.IsPrimitive && fieldType != typeof(string)))
                {
                    deps.Add(new DependencyInfo
                    {
                        Owner = type.Name,
                        Referenced = fieldType.Name,
                        FieldName = field.Name
                    });

                    ScanFieldsRecursive(fieldType, null, deps, visited);
                }
            }
        }

        private static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;");
        }

        private static Dictionary<string, int> CalculateDepths(List<DependencyInfo> deps)
        {
            var depth = new Dictionary<string, int>();
            var visiting = new HashSet<string>(); // 循環参照検出用

            int GetDepth(string name)
            {
                if (depth.ContainsKey(name)) return depth[name];
                
                // 循環参照を検出
                if (visiting.Contains(name))
                {
                    Debug.LogWarning($"⚠️ 循環参照を検出: {name}");
                    depth[name] = 0; // 循環の場合は深度0として扱う
                    return 0;
                }

                visiting.Add(name);
                
                int d = 0;
                foreach (var dep in deps)
                {
                    if (dep.Owner == name)
                    {
                        int childDepth = GetDepth(dep.Referenced) + 1;
                        if (childDepth > d) d = childDepth;
                    }
                }
                
                visiting.Remove(name);
                depth[name] = d;
                return d;
            }

            foreach (var dep in deps)
            {
                if (!depth.ContainsKey(dep.Owner))
                    GetDepth(dep.Owner);
                if (!depth.ContainsKey(dep.Referenced))
                    GetDepth(dep.Referenced);
            }

            return depth;
        }

        public static void ExportToDrawIO(string path, List<DependencyInfo> deps)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<mxfile host=\"app.diagrams.net\" modified=\"2025-10-08T00:00:00.000Z\" agent=\"Unity\" version=\"21.0.0\" type=\"device\">");
            sb.AppendLine("  <diagram id=\"1\" name=\"Unity Dependencies\">");
            sb.AppendLine("    <mxGraphModel dx=\"1200\" dy=\"800\" grid=\"1\" gridSize=\"10\" guides=\"1\" tooltips=\"1\" connect=\"1\" arrows=\"1\" fold=\"1\" page=\"1\" pageScale=\"1\" pageWidth=\"1200\" pageHeight=\"1600\" math=\"0\" shadow=\"0\">");
            sb.AppendLine("      <root>");
            sb.AppendLine("        <mxCell id=\"0\"/>");
            sb.AppendLine("        <mxCell id=\"1\" parent=\"0\"/>");

            int idCounter = 2;
            var nodeIds = new Dictionary<string, int>();

            // 重複を排除
            var uniqueNodes = new HashSet<string>();
            foreach (var dep in deps)
            {
                uniqueNodes.Add(dep.Owner);
                uniqueNodes.Add(dep.Referenced);
            }

            foreach (var node in uniqueNodes)
            {
                nodeIds[node] = idCounter++;
            }

            var depths = CalculateDepths(deps);
            var layers = new Dictionary<int, List<string>>();
            
            foreach (var node in uniqueNodes)
            {
                int d = depths.ContainsKey(node) ? depths[node] : 0;
                if (!layers.ContainsKey(d)) layers[d] = new List<string>();
                layers[d].Add(node);
            }

            // コンパクトなレイアウト設定
            int collapsedWidth = 220;
            int collapsedHeight = 30;
            int ySpacing = 50;
            int layerIdCounter = idCounter;
            var layerCells = new Dictionary<int, int>();
            
            // 各レイヤーを折りたたまれた状態で作成
            foreach (var layer in layers.OrderBy(l => l.Key))
            {
                int layerId = layerIdCounter++;
                layerCells[layer.Key] = layerId;
                int n = layer.Value.Count;
                int layerY = layer.Key * ySpacing + 50;

                // レイヤーコンテナ（折りたたまれた状態で保存）
                sb.AppendLine($"        <mxCell id=\"{layerId}\" value=\"📁 Layer {layer.Key} ({n} classes)\" style=\"swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;html=1;fillColor=#d5e8d4;strokeColor=#82b366;\" parent=\"1\" vertex=\"1\" collapsed=\"1\">");
                sb.AppendLine($"          <mxGeometry x=\"50\" y=\"{layerY}\" width=\"{collapsedWidth}\" height=\"{collapsedHeight}\" as=\"geometry\">");
                sb.AppendLine($"            <mxRectangle x=\"50\" y=\"{layerY}\" width=\"{collapsedWidth}\" height=\"{collapsedHeight}\" as=\"alternateBounds\"/>");
                sb.AppendLine("          </mxGeometry>");
                sb.AppendLine("        </mxCell>");

                // 各ノードを作成（レイヤー内に配置）
                int nodeY = 26;
                foreach (var nodeName in layer.Value)
                {
                    int id = nodeIds[nodeName];

                    sb.AppendLine($"        <mxCell id=\"{id}\" value=\"{EscapeXml(nodeName)}\" style=\"text;strokeColor=#6c8ebf;fillColor=#dae8fc;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;whiteSpace=wrap;html=1;\" parent=\"{layerId}\" vertex=\"1\">");
                    sb.AppendLine($"          <mxGeometry y=\"{nodeY}\" width=\"{collapsedWidth}\" height=\"26\" as=\"geometry\"/>");
                    sb.AppendLine("        </mxCell>");
                    nodeY += 26;
                }
            }

            // エッジ描画（レイヤー間の接続のみ表示して軽量化）
            var layerConnections = new HashSet<string>();
            foreach (var dep in deps)
            {
                if (!nodeIds.ContainsKey(dep.Owner) || !nodeIds.ContainsKey(dep.Referenced))
                    continue;

                int fromDepth = depths.ContainsKey(dep.Owner) ? depths[dep.Owner] : 0;
                int toDepth = depths.ContainsKey(dep.Referenced) ? depths[dep.Referenced] : 0;
                
                // 異なるレイヤー間の接続のみ（同一レイヤー内は省略）
                if (fromDepth != toDepth)
                {
                    string connectionKey = $"{fromDepth}-{toDepth}";
                    if (!layerConnections.Contains(connectionKey))
                    {
                        layerConnections.Add(connectionKey);
                        
                        int fromLayerId = layerCells.ContainsKey(fromDepth) ? layerCells[fromDepth] : -1;
                        int toLayerId = layerCells.ContainsKey(toDepth) ? layerCells[toDepth] : -1;

                        if (fromLayerId > 0 && toLayerId > 0)
                        {
                            sb.AppendLine($"        <mxCell id=\"{layerIdCounter++}\" value=\"dependencies\" style=\"edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;strokeColor=#666666;strokeWidth=2;fontSize=10;labelBackgroundColor=#ffffff;dashed=1;\" edge=\"1\" source=\"{fromLayerId}\" target=\"{toLayerId}\" parent=\"1\">");
                            sb.AppendLine("          <mxGeometry relative=\"1\" as=\"geometry\"/>");
                            sb.AppendLine("        </mxCell>");
                        }
                    }
                }
            }

            sb.AppendLine("      </root>");
            sb.AppendLine("    </mxGraphModel>");
            sb.AppendLine("  </diagram>");
            sb.AppendLine("</mxfile>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Debug.Log($"✅ Draw.ioファイルを書き出しました: {path}");
            Debug.Log($"📊 総ノード数: {uniqueNodes.Count}, レイヤー数: {layers.Count}");
            Debug.Log($"💡 Draw.ioで開き、レイヤーをダブルクリックで展開できます");
        }

        [MenuItem("Tools/Export/Export Dependency Graph to DrawIO")]
        public static void ExportGraph()
        {
            var deps = CollectSceneDependencies();
            if (deps.Count == 0)
            {
                Debug.LogWarning("依存関係が見つかりませんでした。");
                return;
            }

            string savePath = EditorUtility.SaveFilePanel("Export Draw.io Graph", "", "UnityDependencies.drawio", "drawio");
            if (!string.IsNullOrEmpty(savePath))
            {
                ExportToDrawIO(savePath, deps);
            }
        }
    }
    #endregion
}