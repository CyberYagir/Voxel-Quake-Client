using System;
using System.Collections.Generic;
using UnityEngine;

namespace Content.Scripts.Menu
{
    public class WeaponRotator : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private List<GameObject> weapons;

        private void Awake()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
            weapons.GetRandomItem().gameObject.SetActive(true);
        }

        void Update()
        {
            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.World);
        }
    }
}
