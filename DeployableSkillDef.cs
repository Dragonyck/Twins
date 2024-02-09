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
using RoR2.Navigation;
using JetBrains.Annotations;
using System.Linq;

namespace Twins
{
    class DeployableSkillDef : SkillDef
    {
        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new DeployableSkillDef.InstanceData
            {
                behaviour = skillSlot.GetComponent<TwinBehaviour>()
            };
        }
        internal static bool IsExecutable([NotNull] GenericSkill skillSlot)
        {
            DeployableSkillDef.InstanceData data = ((DeployableSkillDef.InstanceData)skillSlot.skillInstanceData);
            TwinBehaviour behaviour = data.behaviour;
            if (behaviour && behaviour.tracker)
            {
                bool executable = true;
                if (Array.IndexOf(skillSlot.skillFamily.variants, Array.Find<SkillFamily.Variant>(skillSlot.skillFamily.variants, (x) => x.skillDef == skillSlot.skillDef)) == 3)
                {
                    executable = behaviour.tracker.wispies.Count(x => x != null) < 4;
                }
                else
                {
                    executable = behaviour.tracker.gupies.Count(x => x != null) == 0;
                }
                return executable;
            }
            return true;
        }
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return DeployableSkillDef.IsExecutable(skillSlot) && base.CanExecute(skillSlot);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && DeployableSkillDef.IsExecutable(skillSlot);
        }
        class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public TwinBehaviour behaviour;
        }
    }
}
