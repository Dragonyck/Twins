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

namespace Twins
{
    class Utils
    {
        public static Vector3 FindNearestNodePosition(Vector3 targetPosition, MapNodeGroup.GraphType nodeGraphType)
        {
            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = HullClassification.Golem;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.prefab = Prefabs.Load<GameObject>("RoR2/Base/Common/DirectorSpawnProbeHelperPrefab.prefab");
            Vector3 result = targetPosition;
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                position = targetPosition
            }, RoR2Application.rng));
            if (gameObject)
            {
                result = gameObject.transform.position;
            }
            if (gameObject)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            UnityEngine.Object.Destroy(spawnCard);
            return result;
        }
        public static EntityStateMachine NewStateMachine<T>(GameObject obj, string customName) where T : EntityState
        {
            SerializableEntityStateType s = new SerializableEntityStateType(typeof(T));
            var newStateMachine = obj.AddComponent<EntityStateMachine>();
            newStateMachine.customName = customName;
            newStateMachine.initialStateType = s;
            newStateMachine.mainStateType = s;
            return newStateMachine;
        }
        public static GenericSkill NewGenericSkill(GameObject obj, SkillDef skill)
        {
            GenericSkill generic = obj.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            generic._skillFamily = newFamily;
            SkillFamily skillFamily = generic.skillFamily;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skill,
                viewableNode = new ViewablesCatalog.Node(skill.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
            return generic;
        }
        public static void AddAlt(SkillFamily skillFamily, SkillDef SkillDef)
        {
            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
        }
        public static ObjectScaleCurve AddScaleComponent(GameObject target, float timeMax)
        {
            ObjectScaleCurve scale = target.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = timeMax;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            return scale;
        }
        public static GameObject NewDisplayModel(GameObject model, string name)
        {
            GameObject characterDisplay = PrefabAPI.InstantiateClone(model, name, false);
            characterDisplay.GetComponentInChildren<CharacterModel>().enabled = false;
            var renderers = characterDisplay.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.shader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
                renderers[i].material.shaderKeywords = null;
            }
            return characterDisplay;
        }
    }
}
