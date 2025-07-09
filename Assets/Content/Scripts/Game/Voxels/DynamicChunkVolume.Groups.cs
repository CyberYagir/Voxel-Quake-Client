using System.Collections.Generic;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public partial class DynamicChunkVolume
    {
         // Кэшированные массивы для переиспользования и избежания аллокаций
        private static readonly Queue<Vector3Int> _floodFillQueue = new Queue<Vector3Int>();
        private static readonly HashSet<int> _visitedIndices = new HashSet<int>();
        private static readonly List<int> _currentGroup = new List<int>();
        private static readonly Vector3Int[] _directions = {
            Vector3Int.right, Vector3Int.left,
            Vector3Int.up, Vector3Int.down,
            Vector3Int.forward, Vector3Int.back
        };

        /// <summary>
        /// Находит все связанные группы блоков в чанке и возвращает их как отдельные массивы индексов
        /// </summary>
        /// <returns>Список групп, где каждая группа содержит индексы связанных блоков</returns>
        public List<List<int>> GetConnectedGroups()
        {
            var groups = new List<List<int>>();
            
            // Очищаем статические коллекции
            _visitedIndices.Clear();
            _floodFillQueue.Clear();

            // Проходим по всем блокам
            for (int i = 0; i < blocks.Length; i++)
            {
                // Пропускаем пустые блоки и уже посещенные
                if (blocks[i].type == 0 || _visitedIndices.Contains(i))
                    continue;

                // Начинаем новую группу
                _currentGroup.Clear();
                FloodFillGroup(i, _currentGroup);

                // Если группа не пустая, добавляем её в результат
                if (_currentGroup.Count > 0)
                {
                    groups.Add(new List<int>(_currentGroup));
                }
            }

            return groups;
        }

        /// <summary>
        /// Flood-fill алгоритм для поиска всех связанных блоков
        /// </summary>
        private void FloodFillGroup(int startIndex, List<int> group)
        {
            _floodFillQueue.Clear();
            _floodFillQueue.Enqueue(IndexToCoord(startIndex));
            _visitedIndices.Add(startIndex);

            while (_floodFillQueue.Count > 0)
            {
                var currentCoord = _floodFillQueue.Dequeue();
                var currentIndex = CoordToIndex(currentCoord);
                
                group.Add(currentIndex);

                // Проверяем всех соседей
                for (int i = 0; i < _directions.Length; i++)
                {
                    var neighborCoord = currentCoord + _directions[i];
                    var neighborIndex = CoordToIndex(neighborCoord);

                    // Проверяем границы и что блок не пустой
                    if (IsValidCoord(neighborCoord) && 
                        neighborIndex != -1 && 
                        blocks[neighborIndex].type != 0 && 
                        !_visitedIndices.Contains(neighborIndex))
                    {
                        _visitedIndices.Add(neighborIndex);
                        _floodFillQueue.Enqueue(neighborCoord);
                    }
                }
            }
        }
        

        /// <summary>
        /// Преобразует индекс в координаты
        /// </summary>
        private Vector3Int IndexToCoord(int index)
        {
            int z = index / (ChunkSize.x * ChunkSize.y);
            int y = (index - z * ChunkSize.x * ChunkSize.y) / ChunkSize.x;
            int x = index - z * ChunkSize.x * ChunkSize.y - y * ChunkSize.x;
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Преобразует координаты в индекс
        /// </summary>
        private int CoordToIndex(Vector3Int coord)
        {
            if (!IsValidCoord(coord))
                return -1;
            return coord.x + ChunkSize.x * (coord.y + ChunkSize.y * coord.z);
        }

        /// <summary>
        /// Проверяет валидность координат
        /// </summary>
        private bool IsValidCoord(Vector3Int coord)
        {
            return coord.x >= 0 && coord.x < ChunkSize.x &&
                   coord.y >= 0 && coord.y < ChunkSize.y &&
                   coord.z >= 0 && coord.z < ChunkSize.z;
        }

        public void CalculateChunkParts()
        {
            var groups = GetConnectedGroups();
            voxelVolume.RemoveDynamicChunkVolume(this);

            var rigidbody = GetComponent<Rigidbody>();
            
            foreach (var group in groups)
            {
                var chunk = voxelVolume.CreateDynamicChunkVolume();

                chunk.transform.position = transform.position;
                chunk.transform.rotation = transform.rotation;
                chunk.gameObject.layer = LayerMask.NameToLayer("Debris");
                chunk.SetDynamicData(chunkSize, voxelSize);

                if (rigidbody != null)
                {
                    var chunkRigidbody = chunk.GetComponent<Rigidbody>();

                    if (chunkRigidbody != null)
                    {
                        chunkRigidbody.isKinematic = false;
                        chunkRigidbody.velocity = rigidbody.velocity;
                    }
                }
                
                foreach (var block in group)
                {
                    chunk.BlocksData[block] = BlocksData[block];
                }
                
                
                chunk.ModifyChunk();
            }
            gameObject.SetActive(false);
        }
    }
}