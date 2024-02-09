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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace Twins
{
    class Special1 : Secondary1
    {
        public override string projectilePrefabPath => "RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab";
        public override float blastamageCoefficient => 25f;
        public override bool useIndicator => true;
        public override bool ButtonDown()
        {
            return base.inputBank.skill4.down;
        }
        public override void FireAttack()
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = Prefabs.Load<GameObject>(projectilePrefabPath),
                position = position,
                rotation = Quaternion.identity,
                procChainMask = default(ProcChainMask),
                target = null,
                owner = base.gameObject,
                damage = blastamageCoefficient,
                crit = base.RollCrit(),
                force = 2000f,
                damageTypeOverride = DamageType.Generic,
                damageColorIndex = DamageColorIndex.Default,
                speedOverride = 0,
                useSpeedOverride = false
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
    }
}
