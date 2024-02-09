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
    class Primary1 : BaseTwinState
    {
        public override int meterGain => 0;
        private float stopwatch;
        // this min duration decides if a big voidball spawns after you charged for this amount of time, otherwise will spawn a small voidball
        private float ballMinChargeDuration = 1;
        private float ballMaxChargeDuration = 2;
        private float projectileFireFrequency = 0.15f;
        private float projectileDamage = 1.2f;
        private float ballMinDamage = 3;
        private float ballMaxDamage = 20;
        private float ballDamage;
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;
        private GameObject fullChargeEffectInstance;
        private string effectMuzzleString = "MuzzleCenter";

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode();
            ballMaxChargeDuration *= base.attackSpeedStat;
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"), muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.baseScale = Vector3.one * 2;
                    scale.timeMax = projectileFireFrequency;
                }
            }
        }
        void FireProjectiles()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * projectileDamage,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 120,
                    owner = base.gameObject,
                    position = aimRay.origin,//aimRay.origin + aimRay.direction * 2,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab"),
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        void FireBall()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * ballDamage,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = ballDamage * 100,
                    owner = base.gameObject,
                    position = aimRay.origin,//aimRay.origin + aimRay.direction * 2,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = base.fixedAge < ballMinChargeDuration ? Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallProjectile.prefab") : Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"),
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        public override void FixedUpdate() 
        {
            base.FixedUpdate();
            base.StartAimMode();
            ballDamage = Util.Remap(base.fixedAge, 0, ballMaxChargeDuration, ballMinDamage, ballMaxDamage);
            if (base.isAuthority && !base.inputBank.skill1.down)
            {
                base.outer.SetNextStateToMain();
                return;
            }
            if (base.fixedAge >= ballMaxChargeDuration)
            {
                if (!fullChargeEffectInstance && chargeEffectInstance)
                {
                    Destroy(chargeEffectInstance);
                    fullChargeEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorReadyMegaBlaster.prefab"), muzzleTransform.position, muzzleTransform.rotation);
                    fullChargeEffectInstance.transform.parent = muzzleTransform;
                    fullChargeEffectInstance.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one * 2;
                }
            }
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= projectileFireFrequency)
            {
                stopwatch = 0;
                FireProjectiles();
            }
        }
        public override void OnExit()
        {
            if (fullChargeEffectInstance)
            {
                Destroy(fullChargeEffectInstance);
            }
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            FireBall();
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
