using UnityEngine;

namespace Content.Scripts.Game
{
    public partial class PlayerController
    {
        [System.Serializable]
        public class WeaponMove
        {
            [SerializeField] private float speed;
            [SerializeField] private float moveModify;
            [SerializeField] private Transform holder;
    
            private Vector3 pos;
            private Vector3 oldPlayerPos;
            private Quaternion rotation;
            private Transform transform;


            public void Init(Transform transform)
            {
                this.transform = transform;
                pos = holder.localPosition;
                rotation = holder.rotation;
                oldPlayerPos = transform.position;
            }


            public void Update()
            {
                var deltaPos = oldPlayerPos - transform.position;

                oldPlayerPos = transform.position;

                holder.localPosition = Vector3.Lerp(holder.localPosition,
                    pos + holder.parent.InverseTransformDirection(deltaPos) * moveModify, Time.deltaTime * 5);
            
            
                holder.localEulerAngles = Vector3.zero;

                var targetPos = holder.rotation;

                rotation = Quaternion.Lerp(rotation, targetPos, Time.deltaTime * speed);
                holder.rotation = rotation;
            }

            public void SetDeltaPos()
            {
                oldPlayerPos = transform.position;
            }

        }
    }
}