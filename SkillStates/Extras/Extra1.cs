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
    class Extra1 : BaseTwinState
    {
        private float duration = 0.55f;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                if (behaviour.activeBuffWard)
                {
                    NetworkServer.Destroy(behaviour.activeBuffWard);
                }
                var ward = UnityEngine.Object.Instantiate(Prefabs.hauntedWard, base.characterBody.corePosition, Quaternion.identity);
                UnityEngine.Object.Destroy(ward.GetComponent<NetworkedBodyAttachment>());
                ward.GetComponent<BuffWard>().radius = 15;
                ward.AddComponent<DestroyOnTimer>().duration = 10;
                ward.GetComponent<TeamFilter>().teamIndex = base.teamComponent.teamIndex;
                behaviour.activeBuffWard = ward.gameObject;
                NetworkServer.Spawn(ward);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
