using System;

namespace Content.Scripts.Game.Voxels
{
    [System.Serializable]
    public struct Block : IEquatable<Block> {
        [NonSerialized] public float health;       // Прочность блока
        public byte type;         // ID блока или тип (0 = пусто)
        public byte flags;        // Биты для настроек (например, разрушим/нет)
        public byte materialId;   // Для текстуры или материала

        public bool IsSolid => type != 0;
    
    
        public bool Equals(Block p)
        {
            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (type == p.type) && (flags == p.flags) && (materialId == p.materialId);
        }
    
        public static bool operator ==(Block lhs, Block rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Block lhs, Block rhs) => !(lhs == rhs);
    }
}