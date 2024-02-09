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
    class DeployableBehaviour : MonoBehaviour
    {
        public TwinsDeployableTracker tracker;
        public CharacterMaster master;
        public DeployableType deployableType;
        public enum DeployableType
        {
            None,
            Wisp,
            Gup
        }
        private void Start()
        {
            master = base.GetComponent<CharacterMaster>();
            if (master && master.minionOwnership && master.minionOwnership.ownerMaster)
            {
                if (!tracker)
                {
                    tracker = master.minionOwnership.ownerMaster.GetComponent<TwinsDeployableTracker>();
                }
                if (deployableType == DeployableType.Wisp)
                {
                    switch (master.inventory.currentEquipmentIndex)
                    {
                        case var value when value == EquipmentCatalog.FindEquipmentIndex("EliteFireEquipment"):
                            tracker.wispies[0] = master;
                            break;
                        case var value when value == EquipmentCatalog.FindEquipmentIndex("EliteLightningEquipment"):
                            tracker.wispies[1] = master;
                            break;
                        case var value when value == EquipmentCatalog.FindEquipmentIndex("EliteEarthEquipment"):
                            tracker.wispies[2] = master;
                            break;
                        case var value when value == EquipmentCatalog.FindEquipmentIndex("EliteLunarEquipment"):
                            tracker.wispies[3] = master;
                            break;
                    }
                }
                else
                {
                    tracker.gupies.Add(master);
                    tracker.gupies.RemoveAll(x => x == null);
                }
            }
        }
    }
}
