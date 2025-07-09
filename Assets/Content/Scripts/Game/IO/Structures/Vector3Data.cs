namespace Content.Scripts.Game.IO.Structures
{
    [System.Serializable]
    public struct Vector3Data
    {
        public float x, y, z;

        public Vector3Data(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}