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
using UnityEngine.Events;

namespace Twins
{
    class Special3 : BaseTwinState
    {
        private float duration = 0.35f;
        private float CooldownPerSummon;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active && behaviour.tracker)
            {
                for (int i = 0; i < behaviour.tracker.wispies.Length; i++)
                {
                    if (!behaviour.tracker.wispies[i])
                    {
                        MasterSummon summon = new MasterSummon
                        {
                            masterPrefab = Prefabs.wispMaster,
                            position = Utils.FindNearestNodePosition((base.transform.position + Vector3.up * 2) + UnityEngine.Random.rotation.normalized.eulerAngles * RoR2Application.rng.RangeFloat(1, 3), RoR2.Navigation.MapNodeGroup.GraphType.Air),
                            rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward),
                            summonerBodyObject = base.gameObject,
                            ignoreTeamMemberLimit = true
                        };
                        CharacterMaster characterMaster = summon.Perform();
                        Deployable deployable = characterMaster.gameObject.AddComponent<Deployable>();
                        deployable.onUndeploy = new UnityEvent();
                        deployable.onUndeploy.AddListener(new UnityAction(characterMaster.TrueKill));
                        base.characterBody.master.AddDeployable(deployable, Prefabs.wisp);

                        if (characterMaster.inventory)
                        {
                            switch (i)
                            {
                                case 0:
                                    characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteFireEquipment"));
                                    break;
                                case 1:
                                    characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteLightningEquipment"));
                                    break;
                                case 2:
                                    characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteEarthEquipment"));
                                    break;
                                case 3:
                                    characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteLunarEquipment"));
                                    break;
                            }
                        }
                    }
                }
            }
            /* if you wanna have a specific cd per summon
             * 
            if (behaviour.tracker.wispies.Length > 0)
            {
                var baseCD = base.skillLocator.special.skillDef.baseRechargeInterval;
                CooldownPerSummon = baseCD / Prefabs.WispCount(null, 0);
                base.skillLocator.special.RunRecharge(baseCD - CooldownPerSummon);
            }*/
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
