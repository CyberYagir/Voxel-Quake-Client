using System;
using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using DG.Tweening;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class WeaponSpot : MapAdditionalItem
    {
        private static Collider[] results = new Collider[10];
        [SerializeField] private WeaponDataObject weaponDataObject;
        [SerializeField] private Transform rotator;
        [SerializeField] private float rotateSpeed = 1f;
        
        [SerializeField] private bool isHasWeapon;

        [SerializeField] private float gravityRadius;


        private bool falling;
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

            if (!falling)
            {
                var overlaps = Physics.OverlapSphereNonAlloc(transform.position, gravityRadius, results, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
                if (overlaps <= 0)
                {
                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
                    {
                        transform.DOMoveY(hit.point.y + 0.1f, 0.5f).SetLink(gameObject).OnComplete(delegate
                        {
                            falling = false;
                        });
                        falling = true;
                    }
                }
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
            isHasWeapon = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, gravityRadius);
        }
    }
}
