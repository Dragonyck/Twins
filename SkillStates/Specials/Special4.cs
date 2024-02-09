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
    class Special4 : BaseTwinState
    {
        private float duration = 0.35f;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active && behaviour.tracker)
            {
                MasterSummon summon = new MasterSummon
                {
                    masterPrefab = Prefabs.gupMaster,
                    position = Utils.FindNearestNodePosition(base.transform.position + UnityEngine.Random.rotation.normalized.eulerAngles * RoR2Application.rng.RangeFloat(1, 3), RoR2.Navigation.MapNodeGroup.GraphType.Ground),
                    rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward),
                    summonerBodyObject = base.gameObject,
                    ignoreTeamMemberLimit = true
                };
                CharacterMaster characterMaster = summon.Perform();
                Deployable deployable = characterMaster.gameObject.AddComponent<Deployable>();
                deployable.onUndeploy = new UnityEvent();
                deployable.onUndeploy.AddListener(new UnityAction(characterMaster.TrueKill));
                base.characterBody.master.AddDeployable(deployable, Prefabs.gup);
                characterMaster.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("ElitePoisonEquipment"));
            }
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
