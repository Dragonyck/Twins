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
    class Secondary : BaseTwinState
    {
        private float minDamage = 3;
        private float maxDamage = 20;
        private float ballMaxChargeDuration = 2;
        private float damageCoefficient;
        private Transform muzzleTransform;
        private GameObject chargeEffectInstance;
        private string effectMuzzleString = "MuzzleCenter";

        public override void OnEnter()
        {
            base.OnEnter();
            ballMaxChargeDuration *= base.attackSpeedStat;
            muzzleTransform = base.FindModelChild(effectMuzzleString);
            if (muzzleTransform)
            {
                chargeEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"), muzzleTransform.position, muzzleTransform.rotation);
                chargeEffectInstance.transform.parent = muzzleTransform;
                ObjectScaleCurve scale = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if (scale)
                {
                    scale.timeMax = ballMaxChargeDuration;
                }
            }
        }
        void Fire()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * damageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = damageCoefficient * 100,
                    owner = base.gameObject,
                    position = aimRay.origin,//aimRay.origin + aimRay.direction * 2,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.gravityVoidballProjectile,
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
            damageCoefficient = Util.Remap(base.fixedAge, 0, ballMaxChargeDuration, minDamage, maxDamage);
            if (base.isAuthority && base.fixedAge >= ballMaxChargeDuration || !base.inputBank.skill2.down)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (chargeEffectInstance)
            {
                Destroy(chargeEffectInstance);
            }
            Fire();
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
