using Content.Scripts.Game.Voxels;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class ChunkBuilderEditor : MonoBehaviour
    {
        [SerializeField] private VoxelVolume voxelVolume;
        [SerializeField] private Camera camera;
        [SerializeField] private DynamicChunkVolume dynamicChunkVolume;
        [SerializeField] private Transform gizmo;

        [SerializeField] private byte material;

        [SerializeField] private Mode mode;
        enum Mode
        {
            Remove,
            Add
        }

        private void Start()
        {
            gizmo.transform.parent = dynamicChunkVolume.transform;
        }

        private void Update()
        {

            if (Input.GetKeyDown("1"))
            {
                mode = Mode.Add;
            }

            if (Input.GetKeyDown("2"))
            {
                mode = Mode.Remove;
            }

            var isOk = Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit);

            if (isOk)
            {
                // Смещаемся на небольшое фиксированное расстояние внутрь поверхности
                var offset = hit.normal * (voxelVolume.VoxelSize * 0.1f);
                var worldPoint = hit.point +  (mode == Mode.Remove ? -offset : offset);

// Преобразуем в локальные координаты
                var localPoint = dynamicChunkVolume.transform.InverseTransformPoint(worldPoint);

// Находим индекс вокселя
                var voxelIndex = Vector3Int.FloorToInt(localPoint / voxelVolume.VoxelSize);

// Если нужны мировые координаты центра вокселя:
                var voxelCenter = (Vector3)voxelIndex * voxelVolume.VoxelSize + Vector3.one * (voxelVolume.VoxelSize * 0.5f);
                gizmo.transform.localPosition = voxelCenter;


                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (mode == Mode.Add)
                    {
                        dynamicChunkVolume.SetBlock(voxelIndex, 1, material);
                    }
                    else
                    {
                        dynamicChunkVolume.SetBlock(voxelIndex, 0, 0);
                    }
                    dynamicChunkVolume.ModifyChunk();
                }
            }
        }
    }
}
