using UnityEngine;

namespace Content.Scripts.Game.IO.Structures
{
    [System.Serializable]
    public struct SavedVolume
    {
        public Vector3IntData boundsSize;
        public Vector3IntData chunkSize;
        public float voxelSize;
        public ChunkData[] chunks;
        public Vector3Data rotation;
        public Vector3Data position;

        public SavedVolume(Vector3Int boundsSize, Vector3Int chunkSize, float voxelSize)
        {
            this.boundsSize = boundsSize.Convert();
            this.chunkSize = chunkSize.Convert();
            this.voxelSize = voxelSize;
            this.chunks = null;
            rotation = new Vector3Data();
            position = new Vector3Data();
        }

        public void SetPos(Vector3Data pos, Vector3Data rot)
        {
            rotation = rot;
            position = pos;
        }
    }
}