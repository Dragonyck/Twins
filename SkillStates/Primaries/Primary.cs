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
    class Primary : BaseTwinState
    {
        public override int meterGain => 0;
        private float duration;
        private float baseDuration = 0.65f;
        private float minFireDuration;
        private float baseFireDuration = 0.25f;
        private bool hasFired;
        private float damageCoefficient = 2.5f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode();
            duration = baseDuration / base.attackSpeedStat;
            //if you wanna fire it after a delay
            minFireDuration = baseFireDuration / base.attackSpeedStat;
            Fire();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= minFireDuration && !hasFired)
            {
                hasFired = true;
                //Fire();
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        void Fire()
        {
            AkSoundEngine.PostEvent("Play_voidman_m2_shoot", base.gameObject);
            EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"), base.gameObject, muzzleString, false);
            Ray aimRay = base.GetAimRay();
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * damageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 500,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.voidProjectileSimple,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 180,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
