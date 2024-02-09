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
using RoR2.UI.MainMenu;
using RoR2.CharacterAI;

namespace Twins
{
    class Hook
    {
        internal static void Hooks()
        {
            On.RoR2.UI.LoadoutPanelController.Row.FromSkin += Row_FromSkin;
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
            On.EntityStates.Mage.FlyUpState.CreateBlinkEffect += FlyUpState_CreateBlinkEffect; ;
        }

        private static void FlyUpState_CreateBlinkEffect(On.EntityStates.Mage.FlyUpState.orig_CreateBlinkEffect orig, EntityStates.Mage.FlyUpState self, Vector3 origin)
        {
            if (self.GetComponent<TwinBehaviour>())
            {
                return;
            }
            orig(self, origin);
        }

        private static void CharacterMaster_onStartGlobal(CharacterMaster master)
        {
            var aiOwnership = master.GetComponent<AIOwnership>();
            if (aiOwnership && aiOwnership.ownerMaster)
            {
                var behaviour = aiOwnership.ownerMaster.GetComponent<DeployableBehaviour>();
                if (behaviour && behaviour.deployableType == DeployableBehaviour.DeployableType.Gup)
                {
                    if (!master.GetComponent<DeployableBehaviour>())
                    {
                        var deployable = master.gameObject.AddComponent<DeployableBehaviour>();
                        deployable.tracker = behaviour.tracker;
                        deployable.deployableType = DeployableBehaviour.DeployableType.Gup;
                        UnityEngine.Object.Destroy(aiOwnership.ownerMaster.gameObject);
                    }
                }
            }
        }
        private static object Row_FromSkin(On.RoR2.UI.LoadoutPanelController.Row.orig_FromSkin orig, LoadoutPanelController owner, BodyIndex bodyIndex)
        {
            var body = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
            if (body && body.GetComponent<TwinBehaviour>())
            {
                var texts = owner.GetComponentsInChildren<LanguageTextMeshController>();
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].token == "LOADOUT_SKILL_MISC")
                    {
                        switch (i)
                        {
                            case 1:
                                texts[i].token = "<color=#D0A1F8>Moonlight Primary</color>";
                                break;
                            case 3:
                                texts[i].token = "<color=#D0A1F8>Moonlight Secondary</color>";
                                break;
                            case 5:
                                texts[i].token = "<color=#D0A1F8>Moonlight Utility</color>";
                                break;
                            case 7:
                                texts[i].token = "<color=#D0A1F8>Moonlight Special</color>";
                                break;
                        }
                    }
                }
            }
            return orig(owner, bodyIndex);
        }
    }
}
