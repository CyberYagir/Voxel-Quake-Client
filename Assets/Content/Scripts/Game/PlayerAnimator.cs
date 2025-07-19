using System;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using FIMSpace.FProceduralAnimation;
using LightServer.Base.PlayersModule;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;
using Zenject;

namespace Content.Scripts.Game
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private NetObject netObject;
        [SerializeField] private Animator animator;
        [SerializeField] private NetPlayerController netPlayerController;
        [SerializeField] private LegsAnimator legsAnimator;
        [SerializeField] private Transform rotationXIK;
        [SerializeField] private float maxSpeed;
        [SerializeField] private Transform weaponRotator;
        [SerializeField] private Transform weaponIKPoint;
        [SerializeField] private Transform weaponSpawnHolder;

        private GameObject spawnedWeapon;
        private float lastCameraX;
        private Vector2 lastVel;
        private WeaponsConfigObject weaponsConfigObject;

        [Inject]
        private void Construct(WeaponsConfigObject weaponsConfigObject)
        {
            this.weaponsConfigObject = weaponsConfigObject;
            animator.gameObject.SetActive(!netObject.isMine);
            enabled = !netObject.isMine;
        }

        private void Update()
        {
            var inverse = transform.InverseTransformDirection(netPlayerController.Velocity);

            var vel = new Vector2(inverse.x, inverse.z) / maxSpeed;

            var lerped = Vector2.Lerp(lastVel, vel, Time.deltaTime * 10);
            var lerpedX = Mathf.Lerp(lastCameraX, netPlayerController.NextCameraX, Time.deltaTime * 10);

            animator.SetFloat("X", lerped.x);
            animator.SetFloat("Y", lerped.y);

            var isHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2);

            animator.SetBool("IsJump", inverse.y > 0.1f && !isHit);


            rotationXIK.transform.SetXLocalEulerAngles(lerpedX);
            weaponRotator.SetXLocalEulerAngles(lerpedX);
            weaponIKPoint.position = weaponSpawnHolder.transform.position;
            weaponIKPoint.rotation = weaponSpawnHolder.transform.rotation;

            var velGlue = netPlayerController.Velocity;
            velGlue.y = 0;
            legsAnimator.GlueMode =
                velGlue.magnitude > 0.1f ? LegsAnimator.EGlueMode.Moving : LegsAnimator.EGlueMode.Idle;
            lastVel = lerped;
            lastCameraX = lerpedX;
        }


        public void ChangeWeapon(EWeaponType weapon)
        {
            if (!enabled) return;

            if (spawnedWeapon != null)
            {
                Destroy(spawnedWeapon.gameObject);
            }

            var so = weaponsConfigObject.WeaponsList.Find(x => x.Type == weapon);
            spawnedWeapon = Instantiate(so.Prefab, weaponSpawnHolder).gameObject;
            spawnedWeapon.ChangeLayerWithChilds(LayerMask.NameToLayer("Debris"));
        }
    }
}
