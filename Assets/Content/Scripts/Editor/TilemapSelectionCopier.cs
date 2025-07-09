using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TilemapSelectionCopier : EditorWindow
{
    private Tilemap sourceTilemap;
    private Tilemap targetTilemap;

    private Vector3Int selectionPosition;
    private Vector3Int selectionSize = new Vector3Int(1, 1, 1);
    private Vector3Int pasteOffset = Vector3Int.zero;

    [MenuItem("Tools/Tilemap/Copy Selected Region")]
    static void Init()
    {
        TilemapSelectionCopier window = GetWindow<TilemapSelectionCopier>("Tilemap Copier");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Tilemap Region Copier", EditorStyles.boldLabel);

        sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", sourceTilemap, typeof(Tilemap), true);
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", targetTilemap, typeof(Tilemap), true);

        GUILayout.Space(10);
        GUILayout.Label("Selection Area", EditorStyles.label);
        GUILayout.Label("For XZY Tilemap: X=Width, Y=Layers, Z=Height", EditorStyles.helpBox);
        selectionPosition = EditorGUILayout.Vector3IntField("Start Position (X,Y,Z)", selectionPosition);
        selectionSize = EditorGUILayout.Vector3IntField("Size (Width,Layers,Height)", selectionSize);
        
        // Предупреждение если размеры некорректные
        if (selectionSize.x <= 0 || selectionSize.y <= 0 || selectionSize.z <= 0)
        {
            EditorGUILayout.HelpBox("All size values must be greater than 0!", MessageType.Warning);
        }

        GUILayout.Space(10);
        GUILayout.Label("Paste Offset", EditorStyles.label);
        pasteOffset = EditorGUILayout.Vector3IntField("Offset", pasteOffset);

        GUILayout.Space(10);
        if (GUILayout.Button("Copy Region"))
        {
            CopyRegion();
        }

        // Добавим кнопку для отладки - показать границы области
        if (GUILayout.Button("Debug Selection Bounds"))
        {
            DebugSelectionBounds();
        }
    }

    private void CopyRegion()
    {
        if (sourceTilemap == null || targetTilemap == null)
        {
            Debug.LogError("Source and Target Tilemaps must be assigned.");
            return;
        }

        // Исправленное вычисление границ
        Vector3Int startPos = selectionPosition;
        Vector3Int endPos = selectionPosition + selectionSize;

        int copiedTiles = 0;

        // Для XZY тайлмапа: X - ширина, Z - высота (вертикаль), Y - глубина/слой
        for (int x = startPos.x; x < endPos.x; x++)
        {
            for (int z = startPos.z; z < endPos.z; z++)  // Z - вертикальная ось
            {
                for (int y = startPos.y; y < endPos.y; y++)  // Y - слои
                {
                    Vector3Int sourcePos = new Vector3Int(x, y, z);
                    TileBase tile = sourceTilemap.GetTile(sourcePos);
                    
                    if (tile != null)
                    {
                        Vector3Int targetPos = sourcePos + pasteOffset;
                        targetTilemap.SetTile(targetPos, tile);
                        
                        // Копируем дополнительные свойства тайла
                        targetTilemap.SetTransformMatrix(targetPos, sourceTilemap.GetTransformMatrix(sourcePos));
                        targetTilemap.SetColor(targetPos, sourceTilemap.GetColor(sourcePos));
                        
                        copiedTiles++;
                    }
                }
            }
        }

        // Принудительно обновляем тайлмап
        targetTilemap.CompressBounds();
        EditorUtility.SetDirty(targetTilemap);
        
        Debug.Log($"Copied {copiedTiles} tiles from region {startPos} to {endPos} with offset {pasteOffset}");
    }

    private void DebugSelectionBounds()
    {
        Vector3Int startPos = selectionPosition;
        Vector3Int endPos = selectionPosition + selectionSize;
        
        Debug.Log($"Selection bounds: Start {startPos}, End {endPos}, Size {selectionSize}");
        
        if (sourceTilemap != null)
        {
            BoundsInt bounds = sourceTilemap.cellBounds;
            Debug.Log($"Source tilemap bounds: {bounds}");
            
            // Проверим несколько конкретных позиций
            Debug.Log("Checking specific positions:");
            for (int testX = startPos.x; testX < Mathf.Min(startPos.x + 3, endPos.x); testX++)
            {
                for (int testZ = startPos.z; testZ < Mathf.Min(startPos.z + 3, endPos.z); testZ++)
                {
                    for (int testY = startPos.y; testY < Mathf.Min(startPos.y + 3, endPos.y); testY++)
                    {
                        Vector3Int testPos = new Vector3Int(testX, testY, testZ);
                        TileBase tile = sourceTilemap.GetTile(testPos);
                        if (tile != null)
                        {
                            Debug.Log($"Found tile at {testPos}: {tile.name}");
                        }
                        else
                        {
                            Debug.Log($"No tile at {testPos}");
                        }
                    }
                }
            }
            
            int tilesInSelection = 0;
            for (int x = startPos.x; x < endPos.x; x++)
            {
                for (int z = startPos.z; z < endPos.z; z++)
                {
                    for (int y = startPos.y; y < endPos.y; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        if (sourceTilemap.GetTile(pos) != null)
                        {
                            tilesInSelection++;
                        }
                    }
                }
            }
            Debug.Log($"Found {tilesInSelection} tiles in selection area");
        }
    }
}