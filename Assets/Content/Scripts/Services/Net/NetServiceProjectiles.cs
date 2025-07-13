using System;
using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Weapons;
using Content.Scripts.Scriptable;
using DG.Tweening;
using LiteNetLib;
using ServerLibrary.Structs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public class NetServiceProjectiles : NetServiceModule
    {
        private Dictionary<string, ProjectileBase> projectiles = new Dictionary<string, ProjectileBase>();
        private ProjectilesConfigObject projectilesConfig;

        public NetServiceProjectiles(NetService netService, ProjectilesConfigObject projectilesConfig, PrefabSpawnerFabric spawnerFabric) : base(netService)
        {
            this.projectilesConfig = projectilesConfig;
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
        }
        
        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.SpawnProjectile:
                    HandleCMDSpawnProjectile(reader);
                    break;
                case ECMDName.DestroyProjectile:
                    HandleCMDDestroyProjectile(reader);
                    break;
            }
        }

        private void HandleCMDDestroyProjectile(NetPacketReader reader)
        {
            var projectileUid = reader.GetString();
            var pos = PacketsManager.ReadVector(reader).Convert();
            DestroyProjectile(projectileUid, pos);
        }

        private void HandleCMDSpawnProjectile(NetPacketReader reader)
        {
            var ownerID = reader.GetInt();
            var projectileType = (EProjectileType)reader.GetByte();
            var projectileUID = reader.GetString();
            
            var pos = PacketsManager.ReadVector(reader).Convert();
            var forward = PacketsManager.ReadVector(reader).Convert();
            var spawnPoint = PacketsManager.ReadVector(reader).Convert();


            var prefab = projectilesConfig.GetProjectilePrefab(projectileType);
            
            switch (projectileType)
            {
                case EProjectileType.Rocket:
                    SpawnRocket(pos, forward, prefab, spawnPoint, ownerID, projectileUID);
                    break;
                case EProjectileType.Rail:
                    SpawnRail(pos, forward, prefab, spawnPoint, ownerID, projectileUID);
                    break;
                case EProjectileType.Gauntlet:
                    SpawnGauntlet(pos, forward, prefab, spawnPoint, ownerID, projectileUID);
                    break;
                case EProjectileType.Machinegun:
                    SpawnBullet(pos, forward, prefab, spawnPoint, ownerID, projectileUID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SpawnBullet(Vector3 pos, Vector3 forward, ProjectileBase prefab, Vector3 spawnPoint, int ownerID, string uid)
        {
            var dir = (pos + forward * 500) - spawnPoint;
            var item = netService.SpawnedFabric.SpawnItem(prefab, spawnPoint)
                .With(x => x.Init(dir, ownerID, uid))
                .With(x=>x.SetHitScanPoint(pos, forward))
                .With(x => AddProjectile(uid, x));
            item.OnProjectileEnd += delegate
            {
                RPCDestroyProjectile(uid, item.transform.position);
            };
        }

        private void SpawnGauntlet(Vector3 pos, Vector3 forward, ProjectileBase prefab, Vector3 spawnPoint, int ownerID, string uid)
        {
            var item = netService.SpawnedFabric.SpawnItem(prefab, spawnPoint)
                .With(x => x.Init(forward, ownerID, uid))
                .With(x=>AddProjectile(uid, x));
        }

        private void SpawnRocket(Vector3 pos, Vector3 forward, ProjectileBase prefab, Vector3 spawnPoint, int ownerID, string uid)
        {
            var raycast = Physics.Raycast(pos, forward, out RaycastHit raycastHit, 500, LayerMask.GetMask("Default"));
            var dir = (pos + forward * 500) - spawnPoint;
            if (raycast)
            {
                dir = raycastHit.point - spawnPoint;
            }
            
            var item = netService.SpawnedFabric.SpawnItem(prefab, spawnPoint)
                .With(x => x.Init(dir, ownerID, uid))
                .With(x=>AddProjectile(uid, x));
            
            if (!raycast)
            {
     
                DOVirtual.DelayedCall(30, delegate
                {
                    DestroyProjectile(uid, item.transform.position);
                }).SetUpdate(UpdateType.Fixed).SetLink(item.gameObject);
            }
        }

        private void SpawnRail(Vector3 pos, Vector3 forward, ProjectileBase prefab, Vector3 spawnPoint, int ownerID,
            string uid)
        {
            var raycast = Physics.Raycast(pos, forward, out RaycastHit raycastHit, 500, LayerMask.GetMask("Default"));
            
            var dir = (pos + forward * 500) - spawnPoint;
            if (raycast)
            {
                dir = raycastHit.point - spawnPoint;
            }
            
            var item = netService.SpawnedFabric.SpawnItem(prefab, spawnPoint)
                .With(x => x.Init(dir, ownerID, uid))
                .With(x=>x.SetHitScanPoint(pos, forward))
                .With(x => AddProjectile(uid, x));
            item.OnProjectileEnd += delegate
            {
                RPCDestroyProjectile(uid, item.transform.position);
            };
        }

        private void AddProjectile(string uid, ProjectileBase projectile)
        {
            projectiles.Add(uid, projectile);
        }

        private void DestroyProjectile(string uid, Vector3 pos)
        {
            if (projectiles.ContainsKey(uid))
            {
                if (projectiles[uid] != null)
                {
                    projectiles[uid].transform.position = pos;
                    projectiles[uid].DestroyProjectile();
                }

                projectiles.Remove(uid);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
        }

        public void RPCSpawnProjectile(EProjectileType id, Vector3 camPos, Vector3 camForward, Vector3 spawnPoint)
        {
            netService.Peer.RPCSpawnProjectile(id, camPos.Convert(), camForward.Convert(), spawnPoint.Convert());
        }

        public void RPCDestroyProjectile(string uid, Vector3 pos)
        {
            netService.Peer.RPCDestroyProjectile(uid, pos.Convert());
        }
    }
}