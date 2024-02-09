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
    class Primary2 : Secondary1
    {
        public override int meterGain => 0;
        public override float fireballDamageCoefficient => 2.8f;
        public virtual GameObject projectilePrefab => null;
        public override string projectilePrefabPath => "RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab";
        public override DamageType GetDamageType()
        {
            return DamageType.Nullify;
        }
        public override bool ButtonDown()
        {
            return true;
        }
        public override void FireAttack()
        {
            if (position == Vector3.zero)
            {
                return;
            }
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = this.characterBody.damage * fireballDamageCoefficient,
                    damageTypeOverride = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 800,
                    owner = base.gameObject,
                    position = position,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = projectilePrefab ? projectilePrefab : Prefabs.Load<GameObject>(projectilePrefabPath),
                    rotation = Quaternion.identity,
                    useFuseOverride = false,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
