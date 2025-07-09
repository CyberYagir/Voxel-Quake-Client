using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelPlaneAdder : MonoBehaviour
    {
        public enum EMode
        {
            Add,
            Replace,
            Clear,
            Fill,
            FloodFill
        }
        [SerializeField] VoxelVolume voxelVolume;
        [SerializeField] private EMode mode;
        
        
        [SerializeField, ShowIf("@mode == EMode.Add || mode == EMode.Fill || mode == EMode.FloodFill")] private int blockType = 0;
        [SerializeField, ShowIf("@mode == EMode.Add || mode == EMode.Replace || mode == EMode.Clear || mode == EMode.Fill || mode == EMode.FloodFill")] private int materialId;
        [SerializeField, ShowIf("@mode == EMode.Replace")] private int materialIdToReplace;
        
        [SerializeField] private float length;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0.5f, length / 2, 0.5f), new Vector3(1, length, 1));

            if (mode == EMode.Add)
            {
                Gizmos.DrawLine(Vector3.zero, Vector3.up * length);
            }
            else if (mode == EMode.FloodFill)
            {
                // Рисуем точку начала FloodFill
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(Vector3.zero, voxelVolume != null ? voxelVolume.VoxelSize * 0.5f : 0.5f);
            }
        }

        [Button]
        public void Active()
        {
            if (mode == EMode.Add)
            {
                Add();
            }
            else if (mode == EMode.Replace)
            {
                Replace();
            }
            else if (mode == EMode.Clear)
            {
                Clear();
            }
            else if (mode == EMode.Fill)
            {
                Fill();
            }
            else if (mode == EMode.FloodFill)
            {
                FloodFill();
            }
        }

        private void FloodFill()
        {
            if (voxelVolume == null)
            {
                Debug.LogError("VoxelVolume is not assigned!");
                return;
            }

            Vector3 startPoint = transform.position;
            
            // Используем BFS для FloodFill
            Queue<Vector3> positions = new Queue<Vector3>();
            HashSet<Vector3> visited = new HashSet<Vector3>();
            
            positions.Enqueue(startPoint);
            visited.Add(GetVoxelGridPosition(startPoint));

            // Направления для поиска соседних блоков (6-connectivity)
            Vector3[] directions = new Vector3[]
            {
                Vector3.right * voxelVolume.VoxelSize,
                Vector3.left * voxelVolume.VoxelSize,
                Vector3.up * voxelVolume.VoxelSize,
                Vector3.down * voxelVolume.VoxelSize,
                Vector3.forward * voxelVolume.VoxelSize,
                Vector3.back * voxelVolume.VoxelSize
            };

            while (positions.Count > 0)
            {
                Vector3 currentPos = positions.Dequeue();
                
                // Проверяем текущую позицию
                var blockIndex = voxelVolume.GetBlock(currentPos, out var chunk);
                if (blockIndex != -1)
                {
                    // Если блок пустой (type == 0), заполняем его
                    if (chunk.BlocksData[blockIndex].type == 0)
                    {
                        chunk.BlocksData[blockIndex].type = (byte)blockType;
                        chunk.BlocksData[blockIndex].materialId = (byte)materialId;
                        voxelVolume.ModifyChunk(chunk);
                        
                        // Добавляем соседние позиции для проверки
                        foreach (var direction in directions)
                        {
                            Vector3 neighborPos = currentPos + direction;
                            Vector3 gridPos = GetVoxelGridPosition(neighborPos);
                            
                            if (!visited.Contains(gridPos))
                            {
                                visited.Add(gridPos);
                                positions.Enqueue(neighborPos);
                            }
                        }
                    }
                }
            }

            voxelVolume.ModifiedChunksDispose();
        }

        private Vector3 GetVoxelGridPosition(Vector3 worldPos)
        {
            // Приводим позицию к сетке вокселей для правильного отслеживания посещенных блоков
            float voxelSize = voxelVolume.VoxelSize;
            return new Vector3(
                Mathf.Round(worldPos.x / voxelSize) * voxelSize,
                Mathf.Round(worldPos.y / voxelSize) * voxelSize,
                Mathf.Round(worldPos.z / voxelSize) * voxelSize
            );
        }

        private void Clear()
        {
            for (float x = 0; x < transform.localScale.x; x += voxelVolume.VoxelSize/2f)
            {
                for (float z = 0; z < transform.localScale.z; z += voxelVolume.VoxelSize/2f)
                {
                    var localPos = new Vector3(x / transform.localScale.x, 0, z / transform.localScale.z);
                    var pos = transform.TransformPoint(localPos);

                    for (float y = length; y >= 0; y -= voxelVolume.VoxelSize/2f)
                    {
                        localPos = new Vector3(x / transform.localScale.x, y, z / transform.localScale.z);
                        pos = transform.TransformPoint(localPos);

                        var blockIndex = voxelVolume.GetBlock(pos, out var chunk);
                        if (blockIndex != -1)
                        {
                            if (chunk.BlocksData[blockIndex].type != 0)
                            {
                                if (chunk.BlocksData[blockIndex].materialId == materialId || materialId == -1)
                                {
                                    chunk.BlocksData[blockIndex].type = 0;
                                    voxelVolume.ModifyChunk(chunk);
                                }
                            }
                        }
                        // Debug.DrawRay(pos, transform.up * length, Color.red, 5);
                    }
                }
            }

            voxelVolume.ModifiedChunksDispose();
        }
        
        private void Fill()
        {
            for (float x = 0; x < transform.localScale.x; x += voxelVolume.VoxelSize/2f)
            {
                for (float z = 0; z < transform.localScale.z; z += voxelVolume.VoxelSize/2f)
                {
                    var localPos = new Vector3(x / transform.localScale.x, 0, z / transform.localScale.z);
                    var pos = transform.TransformPoint(localPos);

                    for (float y = length; y >= 0; y -= voxelVolume.VoxelSize/2f)
                    {
                        localPos = new Vector3(x / transform.localScale.x, y, z / transform.localScale.z);
                        pos = transform.TransformPoint(localPos);

                        var blockIndex = voxelVolume.GetBlock(pos, out var chunk);
                        if (blockIndex != -1)
                        {
                            chunk.BlocksData[blockIndex].type = (byte)blockType;
                            chunk.BlocksData[blockIndex].materialId = (byte)materialId;
                            voxelVolume.ModifyChunk(chunk);
                        }
                        // Debug.DrawRay(pos, transform.up * length, Color.red, 5);
                    }
                }
            }

            voxelVolume.ModifiedChunksDispose();
        }

        private void Replace()
        {
            for (float x = 0; x < transform.localScale.x; x += voxelVolume.VoxelSize/2f)
            {
                for (float z = 0; z < transform.localScale.z; z += voxelVolume.VoxelSize/2f)
                {
                    var localPos = new Vector3(x / transform.localScale.x, 0, z / transform.localScale.z);
                    var pos = transform.TransformPoint(localPos);

                    for (float y = length; y >= 0; y -= voxelVolume.VoxelSize/2f)
                    {
                        localPos = new Vector3(x / transform.localScale.x, y, z / transform.localScale.z);
                        pos = transform.TransformPoint(localPos);

                        var blockIndex = voxelVolume.GetBlock(pos, out var chunk);
                        if (blockIndex != -1)
                        {
                            if (chunk.BlocksData[blockIndex].type != 0)
                            {
                                if (chunk.BlocksData[blockIndex].materialId == materialId)
                                {
                                    chunk.BlocksData[blockIndex].materialId = (byte)materialIdToReplace;
                                    voxelVolume.ModifyChunk(chunk);
                                }
                            }
                        }
                        // Debug.DrawRay(pos, transform.up * length, Color.red, 5);
                    }
                }
            }

            voxelVolume.ModifiedChunksDispose();
        }

        private void Add()
        {
            for (float x = 0; x < transform.localScale.x; x += voxelVolume.VoxelSize/2f)
            {
                for (float z = 0; z < transform.localScale.z; z += voxelVolume.VoxelSize/2f)
                {
                    var localPos = new Vector3(x / transform.localScale.x, 0, z / transform.localScale.z);
                    var pos = transform.TransformPoint(localPos);
                    if (Physics.Raycast(pos, transform.up, out RaycastHit hit, length))
                    {
                        localPos = transform.InverseTransformPoint(hit.point);
                        var maxY = localPos.y;
                        for (float y = maxY; y >= 0; y -= voxelVolume.VoxelSize/2f)
                        {
                            localPos = new Vector3(x / transform.localScale.x, y, z / transform.localScale.z);
                            pos = transform.TransformPoint(localPos);
                             
                      
                        
                            if (hit.collider.GetComponent<ChunkVolume>())
                            {
                                voxelVolume.SetBlock(pos, (byte)blockType, (byte)materialId, out var chunk, false, null);
                            }
                            // Debug.DrawRay(pos, transform.up * length, Color.red, 5);
                        }
                    }
                }
            }

            voxelVolume.ModifiedChunksDispose();
        }
    }
}