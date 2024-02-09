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
    class Utility2 : Secondary2
    {
        public override int meterGain => 10;
        public override float fireballDamageCoefficient => 5;
        public override string projectilePrefabPath => "";
        public override GameObject blastEffectPrefab => Prefabs.Load<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab");
        public override bool ButtonDown()
        {
            return true;
        }
        public override DamageType GetDamageType()
        {
            return DamageType.SlowOnHit | DamageType.Stun1s | DamageType.Freeze2s;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
