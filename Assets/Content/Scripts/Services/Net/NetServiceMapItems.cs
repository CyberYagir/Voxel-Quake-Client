using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Content.Scripts.Game;
using LiteNetLib;
using ServerLibrary.Structs;

namespace Content.Scripts.Services.Net
{
    public class NetServiceMapItems : NetServiceModule
    {
        private Dictionary<int, WeaponSpot> weaponsSpots = new Dictionary<int, WeaponSpot>(5);
        
        public NetServiceMapItems(NetService netService) : base(netService)
        {
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
        }

        public override void Dispose()
        {
            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
        }

        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.PickupWeapon:
                    HandleCMDPickupWeapon(reader);
                    break;
            }
        }

        private void HandleCMDPickupWeapon(NetPacketReader reader)
        {
            var uid = reader.GetInt();
            var time = DateTime.Parse(reader.GetString(), CultureInfo.InvariantCulture);


            if (weaponsSpots.ContainsKey(uid))
            {
                weaponsSpots[uid].UpdateTime(time);
                return;
            }

            netService.StartCoroutine(WaitForSpot());
            
            IEnumerator WaitForSpot()
            {
                while (!weaponsSpots.ContainsKey(uid))
                {
                    yield return null;
                }
                
                weaponsSpots[uid].UpdateTime(time);
            }
        }

        public void AddWeaponSpot(WeaponSpot weaponSpot)
        {
            weaponsSpots.Add(weaponSpot.Uid, weaponSpot);
        }
        
        public void RPCPickupWeapon(int uid, string weaponName)
        {
            netService.Peer.RPCPickupWeapon(uid, weaponName);   
        }
    }
}