using System;
using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class WeaponSpot : MapAdditionalItem
    {
        [SerializeField] private WeaponDataObject weaponDataObject;
        [SerializeField] private Transform rotator;
        [SerializeField] private float rotateSpeed = 1f;
        
        [SerializeField] private bool isHasWeapon;

        private float timer = 0f;
        private float reloadTime;

        public override void Init(MapObjectsService mapObjectsService, int uid, NetService netService)
        {
            base.Init(mapObjectsService, uid, netService);
            
            reloadTime = (float)(double)GetKey("reload_time");

            netService.GetModule<NetServiceMapItems>().AddWeaponSpot(this);
        }

        private void Update()
        {
            rotator.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);

            if (isHasWeapon)
            {
                rotator.localScale = Vector3.Lerp(rotator.localScale, Vector3.one, Time.deltaTime * 5f);
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= reloadTime)
                {
                    timer = 0;
                    isHasWeapon = true;
                }
                rotator.localScale = Vector3.Lerp(rotator.localScale, Vector3.zero, Time.deltaTime * 20f);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player && isHasWeapon)
            {
                if (player.IsMine)
                {
                    player.GetComponent<PlayerInventory>().AddWeapon(weaponDataObject);
                    var mapItemsModule = netService.GetModule<NetServiceMapItems>();
                    mapItemsModule.RPCPickupWeapon(Uid, ID);
                    isHasWeapon = false;
                }
            }
        }

        public void UpdateTime(DateTime time)
        {
            var delta = DateTime.UtcNow - time;
            timer = (float)delta.TotalSeconds;
            print(delta.TotalSeconds);
            isHasWeapon = false;
        }
    }
}
