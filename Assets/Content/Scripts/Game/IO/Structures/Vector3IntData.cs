namespace Content.Scripts.Game.IO.Structures
{
    [System.Serializable]
    public struct Vector3IntData
    {
        public ushort x, y, z;

        public Vector3IntData(int x, int y, int z)
        {
            this.x = (ushort)x;
            this.y = (ushort)y;
            this.z = (ushort)z;
        }
    }
}