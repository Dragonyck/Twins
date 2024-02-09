using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Twins
{
    class Utility : BaseTwinState
    {
        private float maxDuration = 2;
        private float maxDistance = 1000;
        private Vector3 position;
        private GameObject areaIndicator;
        private GameObject blinkVfxInstance;
        private CharacterModel model;

        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                areaIndicator = UnityEngine.Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab, base.transform.position, Quaternion.identity);
                areaIndicator.transform.localScale = new Vector3(2, 8, 2);
                areaIndicator.GetComponentInChildren<MeshRenderer>().material = Prefabs.Load<Material>("RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat");
            }
            blinkVfxInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkVfx.prefab"));
            blinkVfxInstance.transform.SetParent(base.transform, false);

            EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData()
            {
                origin = Util.GetCorePosition(base.gameObject),
                rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
            }, false);

            model = base.GetModelTransform().GetComponent<CharacterModel>();
            if (model)
            {
                model.invisibilityCount++;
            }

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
            }
        }
        public override void Update()
        {
            base.Update();
            if (areaIndicator)
            {
                float num = 0f;
                RaycastHit raycastHit;
                if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(base.GetAimRay(), base.gameObject, out num), out raycastHit, maxDistance + num, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                {
                    position = raycastHit.point;
                    areaIndicator.transform.position = position;
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= maxDuration || !base.inputBank.skill3.down)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
            }
            if (areaIndicator)
            {
                Destroy(areaIndicator);
            }
            if (blinkVfxInstance)
            {
                Destroy(blinkVfxInstance);
            }
            if (model)
            {
                model.invisibilityCount--;
            }
            if (position != Vector3.zero)
            {
                TeleportHelper.TeleportBody(base.characterBody, position);
                EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflash.prefab"), new EffectData()
                {
                    origin = position + Vector3.up,
                    rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward)
                }, false);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
