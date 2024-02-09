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
    class Secondary2 : Secondary1
    {
        public override string projectilePrefabPath => "RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab";
        public override GameObject blastEffectPrefab => Prefabs.lightningImpactEffect;
        public override DamageType GetDamageType()
        {
            return DamageType.Shock5s;
        }
    }
}
