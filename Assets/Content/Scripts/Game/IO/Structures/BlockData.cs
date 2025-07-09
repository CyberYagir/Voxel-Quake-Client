using Content.Scripts.Game.Voxels;

namespace Content.Scripts.Game.IO.Structures
{
    [System.Serializable]
    public struct BlockData
    {
        public ushort id;
        public ushort lineLength;
        public Block block;

        public BlockData(int id, Block block, int lineLength)
        {
            this.id = (ushort)id;
            this.block = block;
            this.lineLength = (ushort)lineLength;
        }
    }
}