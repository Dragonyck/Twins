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
using ExtraSkillSlots;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace Twins
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(ExtraSkillSlotsPlugin.GUID)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Dragonyck.Twins";
        public const string MODNAME = "Twins";
        public const string VERSION = "1.0.0";
        public const string SURVIVORNAME = "Twins";
        public const string SURVIVORNAMEKEY = "TWINS";
        public static GameObject characterPrefab;
        private static readonly Color characterColor = new Color(0.7f, 0.7f, 0.7f);

        private void Awake()
        {
            Prefabs.CreatePrefabs();
            CreatePrefab();
            RegisterStates();
            Hook.Hooks();
        }
        internal static void CreatePrefab()
        {
            var commandoBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            characterPrefab = PrefabAPI.InstantiateClone(commandoBody, SURVIVORNAME + "Body", true);
            characterPrefab.AddComponent<TwinBehaviour>();
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            characterPrefab.GetComponent<CharacterBody>().portraitIcon = Prefabs.Load<Texture2D>("RoR2/DLC1/Assassin2/Assassin2Body.png");
            characterPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Prefabs.crosshair;

            EntityStateMachine mainStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));

            Utils.NewStateMachine<Idle>(characterPrefab, "Jet");

            NetworkStateMachine networkStateMachine = characterPrefab.GetComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = characterPrefab.GetComponents<EntityStateMachine>();

            ContentAddition.AddBody(characterPrefab);

            string desc = "" +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > ";

            string outro = "..and so they left.";
            string fail = "..and so they vanished.";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", SURVIVORNAME);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_OUTRO", outro);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_FAIL", fail);

            var survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = SURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = characterPrefab;
                survivorDef.displayPrefab = Utils.NewDisplayModel(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, SURVIVORNAME + "Display");
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.desiredSortPosition = 0.2f;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };

            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<Primary1>(out hmm);
            ContentAddition.AddEntityState<Primary2>(out hmm);
            ContentAddition.AddEntityState<Primary3>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<Secondary1>(out hmm);
            ContentAddition.AddEntityState<Secondary2>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<Utility1>(out hmm);
            ContentAddition.AddEntityState<Utility2>(out hmm);
            ContentAddition.AddEntityState<Utility3>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<Special1>(out hmm);
            ContentAddition.AddEntityState<Special2>(out hmm);
            ContentAddition.AddEntityState<Special3>(out hmm);
            ContentAddition.AddEntityState<Special4>(out hmm);
            ContentAddition.AddEntityState<Extra>(out hmm);
            ContentAddition.AddEntityState<Extra1>(out hmm);
            ContentAddition.AddEntityState<CharacterMain>(out hmm);
            ContentAddition.AddEntityState<BaseTwinState>(out hmm);
        }
        static void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
            ExtraSkillSetup();
        }
        static void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            TwinBehaviour behaviour = characterPrefab.GetComponent<TwinBehaviour>();

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Primary";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 4;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            component.primary = Utils.NewGenericSkill(characterPrefab, SkillDef);
            var skillFamily = component.primary.skillFamily;

            behaviour.primary = Utils.NewGenericSkill(characterPrefab, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Primary 1";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary1));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 1f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.primary._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Primary 2";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary2));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.primary._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Primary 3";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary3));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.primary._skillFamily, SkillDef);
        }
        static void SecondarySetup()
        {
            TwinBehaviour behaviour = characterPrefab.GetComponent<TwinBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            SkillDef.skillName = "Secondary";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            component.secondary = Utils.NewGenericSkill(characterPrefab, SkillDef);
            var skillFamily = component.secondary.skillFamily;

            behaviour.secondary = Utils.NewGenericSkill(characterPrefab, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Secondary 1";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary1));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.secondary._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Secondary 2";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary2));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.secondary._skillFamily, SkillDef);
        }
        static void UtilitySetup()
        {
            TwinBehaviour behaviour = characterPrefab.GetComponent<TwinBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Utility";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            component.utility = Utils.NewGenericSkill(characterPrefab, SkillDef);
            var skillFamily = component.utility.skillFamily;

            behaviour.utility = Utils.NewGenericSkill(characterPrefab, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Utility 1";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility1));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.utility._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Utility 2";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility2));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.utility._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Utility 3";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility3));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.utility._skillFamily, SkillDef);
        }
        static void SpecialSetup()
        {
            TwinBehaviour behaviour = characterPrefab.GetComponent<TwinBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Special";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special));
            SkillDef.activationStateMachineName = "Body";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            component.special = Utils.NewGenericSkill(characterPrefab, SkillDef);
            var skillFamily = component.special.skillFamily;

            behaviour.special = Utils.NewGenericSkill(characterPrefab, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Special 1";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special1));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 12f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.special._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Special 2";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special2));
            SkillDef.activationStateMachineName = "Body";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 12f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.special._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<DeployableSkillDef>();
            SkillDef.skillName = "Special 3";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special3));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 60f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.special._skillFamily, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<DeployableSkillDef>();
            SkillDef.skillName = "Special 4";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special4));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 60f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(skillFamily, SkillDef);
            Utils.AddAlt(behaviour.special._skillFamily, SkillDef);
        }
        static void ExtraSkillSetup() 
        {
            ExtraSkillLocator component = characterPrefab.AddComponent<ExtraSkillLocator>();

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Extra Skill";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Extra));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);
            component.extraFirst = Utils.NewGenericSkill(characterPrefab, SkillDef);

            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.skillName = "Extra Skill 1";
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.activationState = new SerializableEntityStateType(typeof(Extra1));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            ContentAddition.AddSkillDef(SkillDef);

            Utils.AddAlt(component.extraFirst.skillFamily, SkillDef);
        }
    }
}
