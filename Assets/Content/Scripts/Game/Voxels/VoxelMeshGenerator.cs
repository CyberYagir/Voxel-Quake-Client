using System;
using System.Collections.Generic;
using Content.Scripts.Game.Scriptable;
using UnityEngine;
using UnityEngine.Rendering;

namespace Content.Scripts.Game.Voxels
{
    public static class VoxelMeshGenerator
    {
        // Структура для переиспользования данных оси без аллокаций
        private struct AxisData
        {
            public int dimension;
            public int u, v;
            public int width, height, length;
            public Vector3Int qVector;
        }
        
        private struct SubMeshData
        {
            public List<Vector3> vertices;
            public List<int> triangles;
            public List<Vector2> uvs;
            public List<Vector3> normals;
            public int materialId;
        }
    
        // Кэшированные массивы для переиспользования
        private static Block[] cachedMask;
        private static int cachedMaskSize = 0;
        private static HashSet<int> cachedMaterialsIds = new HashSet<int>();

        // НОВОЕ: Предварительно выделенные буферы для избежания аллокаций
        private static readonly List<Vector3> tempVertices = new List<Vector3>(4);
        private static readonly List<Vector3> tempNormals = new List<Vector3>(4);
        private static readonly List<Vector2> tempUVs = new List<Vector2>(4);
        private static readonly List<int> tempTriangles = new List<int>(6);
        
        // Пулы для SubMeshData - избегаем создания новых списков
        private static readonly Queue<SubMeshData> subMeshPool = new Queue<SubMeshData>();
        private static readonly int MaxPoolSize = 32;

        // Описание всех граней (6 сторон)
        private static readonly Vector3[] faceNormals =
        {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back, Vector3.forward
        };

        private static readonly Vector3[,] faceVertices = new Vector3[6, 4]
        {
            // LEFT
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(0, 0, 1)
            },
            // RIGHT
            {
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0)
            },
            // BOTTOM
            {
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 0)
            },
            // TOP
            {
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 1, 1),
                new Vector3(0, 1, 1)
            },
            // BACK
            {
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0)
            },
            // FRONT
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, 0, 1)
            }
        };

        private static Dictionary<string, Mesh> meshInstanced = new Dictionary<string, Mesh>(2000);
        private static Dictionary<string, Material[]> meshInstancedMaterials = new Dictionary<string, Material[]>(2000);
        private static List<IMeshDrawable> modifiedNeighboursChunks = new List<IMeshDrawable>(6);
        private static Dictionary<int, SubMeshData> subMeshDict = new Dictionary<int, SubMeshData>(6);
        private static List<Vector3> allVertices = new List<Vector3>();
        private static List<Vector3> allNormals = new List<Vector3>();
        private static List<Vector2> allUVs = new List<Vector2>();
        private static List<SubMeshDescriptor> subMeshes = new List<SubMeshDescriptor>();
        private static List<int> allTriangles = new List<int>(256);
        private static List<int> materialIds = new List<int>(256);

        public static List<IMeshDrawable> ModifiedNeighboursChunks => modifiedNeighboursChunks;

        /// <summary>
        /// Создаёт или получает SubMeshData из пула
        /// </summary>
        private static SubMeshData GetPooledSubMeshData(int materialId)
        {
            SubMeshData subMesh;
            
            if (subMeshPool.Count > 0)
            {
                subMesh = subMeshPool.Dequeue();
                // Очищаем списки для повторного использования
                subMesh.vertices.Clear();
                subMesh.triangles.Clear();
                subMesh.uvs.Clear();
                subMesh.normals.Clear();
                subMesh.materialId = materialId;
            }
            else
            {
                subMesh = new SubMeshData
                {
                    vertices = new List<Vector3>(256),
                    triangles = new List<int>(256),
                    uvs = new List<Vector2>(256),
                    normals = new List<Vector3>(256),
                    materialId = materialId
                };
            }
            
            return subMesh;
        }

        /// <summary>
        /// Возвращает SubMeshData в пул
        /// </summary>
        private static void ReturnToPool(SubMeshData subMesh)
        {
            if (subMeshPool.Count < MaxPoolSize)
            {
                subMeshPool.Enqueue(subMesh);
            }
        }

        /// <summary>
        /// Основной метод генерации меша с использованием Greedy Meshing алгоритма
        /// Оптимизирован для минимальных аллокаций
        /// </summary>
        public static Mesh GenerateMesh(IMeshDrawable chunk, MaterialListObject materials)
        {
            var hash = GetChunkHashOptimized(chunk);
            if (meshInstanced.ContainsKey(hash))
            {
                chunk.SetMaterials(meshInstancedMaterials[hash]);
                return meshInstanced[hash];
            }
            
            if (meshInstanced.Count >= 2000)
            {
                meshInstanced.Clear();
                meshInstancedMaterials.Clear();
                Debug.LogError("Clear Meshes");
            }

            Vector3Int size = chunk.ChunkSize;
            float voxelSize = chunk.VoxelSize;

            cachedMaterialsIds.Clear();
            
            // Возвращаем использованные SubMeshData в пул
            foreach (var kvp in subMeshDict)
            {
                ReturnToPool(kvp.Value);
            }
            subMeshDict.Clear();
            
            modifiedNeighboursChunks.Clear();
            
            // Проходим по всем трём измерениям
            for (int dimension = 0; dimension < 3; dimension++)
            {
                ProcessDimension(chunk, dimension, size, voxelSize);
            }

            var mesh = CreateMultiMaterialMesh(materials, chunk, hash);
            
            mesh.name = hash;
            meshInstanced.Add(hash, mesh);
            
            return mesh;
        }
        
        public static string GetChunkHashOptimized(IMeshDrawable chunk)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                const ulong fnvPrime = 1099511628211UL;
            
                var blocks = chunk.BlocksData;
                var chunkSize = chunk.ChunkSize;
            
                // Хешируем размер чанка
                hash ^= (ulong)chunkSize.x;
                hash *= fnvPrime;
                hash ^= (ulong)chunkSize.y;
                hash *= fnvPrime;
                hash ^= (ulong)chunkSize.z;
                hash *= fnvPrime;
            
                // Хешируем только непустые блоки с их позициями
                for (int x = 0; x < chunkSize.x; x++)
                {
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        for (int z = 0; z < chunkSize.z; z++)
                        {
                            int index = x + y * chunkSize.x + z * chunkSize.x * chunkSize.y;
                            var block = blocks[index];
                        
                            if (block.IsSolid)
                            {
                                // Хешируем позицию блока
                                hash ^= (ulong)x;
                                hash *= fnvPrime;
                                hash ^= (ulong)y;
                                hash *= fnvPrime;
                                hash ^= (ulong)z;
                                hash *= fnvPrime;
                            
                                // Хешируем данные блока
                                hash ^= block.type;
                                hash *= fnvPrime;
                                hash ^= block.flags;
                                hash *= fnvPrime;
                                hash ^= block.materialId;
                                hash *= fnvPrime;
                            }
                        }
                    }
                }
            
                return hash.ToString("X");
            }
        }

        /// <summary>
        /// Обрабатывает одно измерение (ось) для генерации граней
        /// </summary>
        private static void ProcessDimension(
            IMeshDrawable chunk, 
            int dimension, 
            Vector3Int size, 
            float voxelSize
        )
        {
            // Инициализируем данные оси без аллокаций
            AxisData axisData = InitializeAxisData(dimension, size);

            // Обрабатываем обе стороны (переднюю и заднюю)
            ProcessAxisDirection(chunk, ref axisData, false, voxelSize);
            ProcessAxisDirection(chunk, ref axisData, true, voxelSize);
        }

        /// <summary>
        /// Инициализирует данные оси без создания новых объектов
        /// </summary>
        private static AxisData InitializeAxisData(int dimension, Vector3Int size)
        {
            AxisData data;
            data.dimension = dimension;
            data.u = (dimension + 1) % 3;
            data.v = (dimension + 2) % 3;
        
            int[] dims = { size.x, size.y, size.z };
            data.width = dims[dimension];
            data.height = dims[data.u];
            data.length = dims[data.v];
        
            data.qVector = Vector3Int.zero;
            data.qVector[dimension] = 1;
        
            return data;
        }

        /// <summary>
        /// Обрабатывает одно направление оси (переднюю или заднюю грань)
        /// </summary>
        private static void ProcessAxisDirection(
            IMeshDrawable chunk, 
            ref AxisData axisData,
            bool isBackFace, 
            float voxelSize)
        {
            int maskSize = axisData.height * axisData.length;
            EnsureMaskCapacity(maskSize);

            // Обрабатываем все слои
            for (int slice = 0; slice <= axisData.width; slice++)
            {
                BuildSliceMask(chunk, ref axisData, slice, isBackFace);
                ProcessSliceMask(ref axisData, slice, isBackFace, voxelSize);
            }
        }

        /// <summary>
        /// Обеспечивает достаточную вместимость кэшированной маски
        /// </summary>
        private static void EnsureMaskCapacity(int requiredSize)
        {
            if (cachedMask == null || cachedMaskSize < requiredSize)
            {
                cachedMask = new Block[requiredSize];
                cachedMaskSize = requiredSize;
            }
        }

        /// <summary>
        /// Строит маску для одного среза, определяя видимые грани
        /// </summary>
        private static void BuildSliceMask(IMeshDrawable chunk, ref AxisData axisData, int slice, bool isBackFace)
        {
            Vector3Int pos = Vector3Int.zero;
            Vector3Int npos = Vector3Int.zero;

            for (int j = 0; j < axisData.length; j++)
            {
                for (int i = 0; i < axisData.height; i++)
                {
                    Block current = new Block { type = 0 };
                    Block neighbor = new Block { type = 0 };

                    // Получаем текущий блок
                    if (slice < axisData.width)
                    {
                        pos[axisData.dimension] = slice;
                        pos[axisData.u] = i;
                        pos[axisData.v] = j;
                        current = GetVoxel(chunk, pos);
                    }

                    // Получаем соседний блок
                    if (slice > 0)
                    {
                        npos[axisData.dimension] = slice - 1;
                        npos[axisData.u] = i;
                        npos[axisData.v] = j;
                        neighbor = GetVoxel(chunk, npos);
                    }

                    // ИСПРАВЛЕНИЕ: Правильная логика определения видимой грани
                    Block visibleBlock = new Block { type = 0 };
            
                    if (isBackFace)
                    {
                        // Задняя грань: показываем, если текущий блок есть, а соседний - нет
                        if (current.type != 0 && neighbor.type == 0)
                        {
                            visibleBlock = current;
                        }
                    }
                    else
                    {
                        // Передняя грань: показываем, если соседний блок есть, а текущий - нет
                        if (neighbor.type != 0 && current.type == 0)
                        {
                            visibleBlock = neighbor;
                        }
                    }

                    cachedMask[i + j * axisData.height] = visibleBlock;
                }
            }
        }

        /// <summary>
        /// Определяет видимый блок на основе текущего и соседнего блоков
        /// </summary>
        private static Block DetermineVisibleBlock(ref Block current, ref Block neighbor, bool isBackFace)
        {
            if (isBackFace)
            {
                return (current.type != 0 && neighbor.type == 0) ? current : new Block { type = 0 };
            }
            else
            {
                return (neighbor.type != 0 && current.type == 0) ? neighbor : new Block { type = 0 };
            }
        }

        /// <summary>
        /// Обрабатывает маску среза, создавая оптимизированные квады
        /// </summary>
        private static void ProcessSliceMask(
            ref AxisData axisData,
            int slice,
            bool isBackFace,
            float voxelSize)
        {
            for (int j = 0; j < axisData.length;)
            {
                for (int i = 0; i < axisData.height;)
                {
                    Block block = cachedMask[i + j * axisData.height];
                
                    if (block.type == 0)
                    {
                        i++;
                        continue;
                    }

                    // Вычисляем размер квада для greedy meshing
                    int width = CalculateQuadWidth(ref axisData, i, j, ref block);
                    int height = CalculateQuadHeight(ref axisData, i, j, width, ref block);

                    // Очищаем обработанную область
                    ClearMaskArea(ref axisData, i, j, width, height);

                    // Создаём квад
                    CreateQuadForMaterial(ref axisData, i, j, width, height, slice, isBackFace, voxelSize, block.materialId);
                
                    i += width;
                }
                j++;
            }
        }

        private static void CreateQuadForMaterial(
            ref AxisData axisData,
            int i, int j, int width, int height,
            int slice, bool isBackFace, float voxelSize, int materialId)
        {
            // Получаем или создаём подмеш для данного материала
            if (!subMeshDict.TryGetValue(materialId, out SubMeshData subMesh))
            {
                subMesh = GetPooledSubMeshData(materialId);
                subMeshDict[materialId] = subMesh;
            }

            // Создаём квад БЕЗ АЛЛОКАЦИЙ
            CreateQuadOptimized(ref subMesh, ref axisData, i, j, width, height, slice, isBackFace, voxelSize);

            // Обновляем данные в словаре
            subMeshDict[materialId] = subMesh;
        }

        /// <summary>
        /// Вычисляет ширину квада для greedy meshing
        /// </summary>
        private static int CalculateQuadWidth(ref AxisData axisData, int i, int j, ref Block block)
        {
            int width = 1;
            while (i + width < axisData.height && cachedMask[i + width + j * axisData.height].type == block.type && cachedMask[i + width + j * axisData.height].materialId == block.materialId)
                width++;
            return width;
        }

        /// <summary>
        /// Вычисляет высоту квада для greedy meshing
        /// </summary>
        private static int CalculateQuadHeight(ref AxisData axisData, int i, int j, int width, ref Block block)
        {
            int height = 1;
            bool canExpand = true;
        
            while (j + height < axisData.length && canExpand)
            {
                for (int k = 0; k < width; k++)
                {
                    if (cachedMask[i + k + (j + height) * axisData.height].type != block.type || cachedMask[i + k + (j + height) * axisData.height].materialId != block.materialId)
                    {
                        canExpand = false;
                        break;
                    }
                }
                if (canExpand) height++;
            }
        
            return height;
        }

        /// <summary>
        /// Очищает обработанную область в маске
        /// </summary>
        private static void ClearMaskArea(ref AxisData axisData, int i, int j, int width, int height)
        {
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    cachedMask[i + dx + (j + dy) * axisData.height].type = 0;
                    cachedMask[i + dx + (j + dy) * axisData.height].materialId = 0;
                }
            }
        }

        /// <summary>
        /// ОПТИМИЗИРОВАННАЯ ВЕРСИЯ: Создаёт квад БЕЗ временных аллокаций
        /// Напрямую добавляет данные в списки SubMeshData
        /// </summary>
        private static void CreateQuadOptimized(
            ref SubMeshData subMesh,
            ref AxisData axisData, 
            int i, int j, int width, int height, 
            int slice, bool isBackFace, float voxelSize)
        {
            // Вычисляем позицию квада
            Vector3Int basePos = Vector3Int.zero;
            basePos[axisData.dimension] = slice;
            basePos[axisData.u] = i;
            basePos[axisData.v] = j;

            Vector3Int du = Vector3Int.zero;
            du[axisData.u] = width;

            Vector3Int dv = Vector3Int.zero;
            dv[axisData.v] = height;

            // Вычисляем вертексы напрямую
            Vector3 v0 = (Vector3)basePos * voxelSize;
            Vector3 v1 = (Vector3)(basePos + dv) * voxelSize;
            Vector3 v2 = (Vector3)(basePos + du + dv) * voxelSize;
            Vector3 v3 = (Vector3)(basePos + du) * voxelSize;

            // Корректируем порядок вершин для задней грани
            if (isBackFace)
            {
                Vector3 temp = v1;
                v1 = v3;
                v3 = temp;
            }

            // Получаем текущий базовый индекс
            int vertStart = subMesh.vertices.Count;
            
            // Напрямую добавляем вертексы в список (без промежуточных аллокаций)
            subMesh.vertices.Add(v0);
            subMesh.vertices.Add(v1);
            subMesh.vertices.Add(v2);
            subMesh.vertices.Add(v3);

            // Добавляем треугольники напрямую
            subMesh.triangles.Add(vertStart);
            subMesh.triangles.Add(vertStart + 2);
            subMesh.triangles.Add(vertStart + 1);
            subMesh.triangles.Add(vertStart);
            subMesh.triangles.Add(vertStart + 3);
            subMesh.triangles.Add(vertStart + 2);

            // Вычисляем и добавляем нормали
            Vector3 normal = Vector3.zero;
            normal[axisData.dimension] = isBackFace ? -1 : 1;

            subMesh.normals.Add(normal);
            subMesh.normals.Add(normal);
            subMesh.normals.Add(normal);
            subMesh.normals.Add(normal);

            // Добавляем UV координаты напрямую
            subMesh.uvs.Add(Vector2.zero);
            subMesh.uvs.Add(new Vector2(width, 0));
            subMesh.uvs.Add(new Vector2(width, height));
            subMesh.uvs.Add(new Vector2(0, height));
        }

        /// <summary>
        /// УСТАРЕВШИЙ МЕТОД: Создаёт квад (4 вертекса и 2 треугольника)
        /// Заменён на CreateQuadOptimized для избежания аллокаций
        /// </summary>
        private static void CreateQuad(ref List<Vector3> verts, ref List<int> tris, ref List<Vector2> uvs,
            ref List<Vector3> normals,
            ref AxisData axisData, int i, int j, int width, int height, int slice, bool isBackFace, float voxelSize)
        {
            // Вычисляем позицию квада
            Vector3Int basePos = Vector3Int.zero;
            basePos[axisData.dimension] = slice;
            basePos[axisData.u] = i;
            basePos[axisData.v] = j;

            Vector3Int du = Vector3Int.zero;
            du[axisData.u] = width;

            Vector3Int dv = Vector3Int.zero;
            dv[axisData.v] = height;

            // Вычисляем вертексы
            Vector3 v0 = (Vector3)basePos * voxelSize;
            Vector3 v1 = (Vector3)(basePos + dv) * voxelSize;
            Vector3 v2 = (Vector3)(basePos + du + dv) * voxelSize;
            Vector3 v3 = (Vector3)(basePos + du) * voxelSize;

            // Корректируем порядок вершин для задней грани
            if (isBackFace)
            {
                Vector3 temp = v1;
                v1 = v3;
                v3 = temp;
            }

            // ИСПРАВЛЕНИЕ: Используем текущее количество вертексов в списке как базовый индекс
            int vertStart = verts.Count;
            verts.Add(v0);
            verts.Add(v1);
            verts.Add(v2);
            verts.Add(v3);

            // Добавляем треугольники с правильными индексами
            AddQuadTriangles(ref tris, vertStart);

            // Добавляем нормали
            Vector3 normal = Vector3.zero;
            normal[axisData.dimension] = isBackFace ? -1 : 1;

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            // Добавляем UV координаты
            uvs.Add(Vector2.zero);
            uvs.Add(new Vector2(width, 0));
            uvs.Add(new Vector2(width, height));
            uvs.Add(new Vector2(0, height));
        }

        /// <summary>
        /// Добавляет треугольники для квада
        /// </summary>
        private static void AddQuadTriangles(ref List<int> tris, int vertStart)
        {
            tris.Add(vertStart);
            tris.Add(vertStart + 2);
            tris.Add(vertStart + 1);
            tris.Add(vertStart);
            tris.Add(vertStart + 3);
            tris.Add(vertStart + 2);
        }

        private static Mesh CreateMultiMaterialMesh(MaterialListObject materialList, IMeshDrawable chunkVolume,
            string hash)
        {
            var mesh = new Mesh();

            if (subMeshDict.Count == 0)
            {
                return mesh; // Пустой меш
            }

            // Объединяем все вертексы и создаём подмеши
            allVertices.Clear();
            allNormals.Clear();
            allUVs.Clear();
            subMeshes.Clear();

            // Собираем все треугольники в один массив
            allTriangles.Clear();
            materialIds.Clear();

            foreach (var kvp in subMeshDict)
            {
                var subMesh = kvp.Value;

                if (subMesh.vertices.Count == 0)
                    continue;

                // ИСПРАВЛЕНИЕ: Корректно смещаем индексы треугольников
                int baseVertexIndex = allVertices.Count;

                // Добавляем вертексы, нормали и UV
                allVertices.AddRange(subMesh.vertices);
                allNormals.AddRange(subMesh.normals);
                allUVs.AddRange(subMesh.uvs);

                // Добавляем треугольники со смещением индексов
                int triangleStart = allTriangles.Count;
                foreach (int triangle in subMesh.triangles)
                {
                    allTriangles.Add(triangle + baseVertexIndex);
                }

                // Создаём описание подмеша
                var subMeshDescriptor = new SubMeshDescriptor
                {
                    topology = MeshTopology.Triangles,
                    indexStart = triangleStart,
                    indexCount = subMesh.triangles.Count,
                    baseVertex = 0
                };

                subMeshes.Add(subMeshDescriptor);
                materialIds.Add(kvp.Key);
            }

            // Устанавливаем данные меша
            mesh.SetVertices(allVertices);
            mesh.SetNormals(allNormals);
            mesh.SetUVs(0, allUVs);

            // Устанавливаем индексы (все треугольники)
            mesh.SetIndices(allTriangles, MeshTopology.Triangles, 0);

            // Устанавливаем количество подмешей
            mesh.subMeshCount = subMeshes.Count;

            // Устанавливаем подмеши
            for (int i = 0; i < subMeshes.Count; i++)
            {
                mesh.SetSubMesh(i, subMeshes[i]);
            }

            // Создаём массив материалов
            Material[] materials = new Material[materialIds.Count];
            for (int i = 0; i < materialIds.Count; i++)
            {
                materials[i] = materialList.GetMaterial(materialIds[i]);
            }

            meshInstancedMaterials.Add(hash, materials);
            chunkVolume.SetMaterials(materials);

            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Создаёт финальный Unity меш
        /// </summary>
        private static Mesh CreateMesh(List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Vector3> normals)
        {
            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Получает вокселъ с учётом соседних чанков
        /// </summary>
        private static Block GetVoxel(IMeshDrawable chunk, Vector3Int pos)
        {
            return chunk.GetBlockWithNeighbors(pos.x, pos.y, pos.z, ref modifiedNeighboursChunks);
        }
    }
}