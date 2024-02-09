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
using TMPro;

namespace Twins
{
    class TwinBehaviour : MonoBehaviour
    {
        public GenericSkill primary;
        public GenericSkill secondary;
        public GenericSkill utility;
        public GenericSkill special;
        private CharacterBody body;
        private SkillLocator skillLocator;
        private GameObject meterBar;
        private TextMeshProUGUI currentMeter;
        private TextMeshProUGUI fullMeter;
        private Image barImage;
        [SerializeField]
        private int meterValue = 0;
        [SerializeField]
        private int maxMeterValue = 40;
        private bool barSetupDone;
        public bool canExecute = true;
        private Color barColor = new Color(0.8156863f, 0.6313726f, 0.972549f);
        private bool swapped;
        public GameObject activeBuffWard;
        public TwinsDeployableTracker tracker;
        internal bool maxMeter
        {
            get
            {
                return meterValue >= maxMeterValue;
            }
        }
        public float currentMeterValue
        {
            get
            {
                return meterValue;
            }
        }
        private bool muzzleToggle = true;
        public string muzzleString
        {
            get
            {
                string s = muzzleToggle ? "MuzzleRight" : "MuzzleLeft";
                muzzleToggle ^= true;
                return s;
            }
        }
        public void AddMeter(int value)
        {
            if (value + meterValue >= maxMeterValue && skillLocator)
            {
                if (swapped)
                {
                    swapped = false;

                    skillLocator.primary.UnsetSkillOverride(this, primary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.UnsetSkillOverride(this, secondary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.UnsetSkillOverride(this, utility.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.special.UnsetSkillOverride(this, special.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                else
                {
                    swapped = true;

                    skillLocator.primary.SetSkillOverride(this, primary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.secondary.SetSkillOverride(this, secondary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.utility.SetSkillOverride(this, utility.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                    skillLocator.special.SetSkillOverride(this, special.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                meterValue = 0;
                return;
            }
            meterValue += value;
        }
        private void OnEnable()
        {
            body = base.GetComponent<CharacterBody>();
            skillLocator = body.skillLocator;
            On.RoR2.UI.HUD.Update += HUD_Update;
        }
        private void Start()
        {
            if (body.masterObject)
            {
                tracker = body.masterObject.GetComponent<TwinsDeployableTracker>();
                if (!tracker)
                {
                    tracker = body.masterObject.AddComponent<TwinsDeployableTracker>();
                }
            }
        }
        private void HUD_Update(On.RoR2.UI.HUD.orig_Update orig, HUD self)
        {
            orig(self);
            if (body && !skillLocator)
            {
                skillLocator = body.skillLocator;
            }
            meterValue = Mathf.Clamp(meterValue, 0, maxMeterValue);
            if (barImage)
            {
                barImage.fillAmount = (float)meterValue / (float)maxMeterValue;
            }
            string value = meterValue.ToString();
            if (currentMeter && currentMeter.text != value)
            {
                currentMeter.text = value;
            }
            #region barSetup
            if (self.targetBodyObject && self.targetBodyObject == base.gameObject && self.mainUIPanel && Util.HasEffectiveAuthority(self.targetBodyObject.GetComponent<NetworkIdentity>()))
            {
                if (!meterBar)
                {
                    var healthBar = self.mainUIPanel.GetComponentInChildren<HealthBar>();
                    if (healthBar && healthBar.gameObject)
                    {
                        var images = healthBar.gameObject.GetComponentsInChildren<Image>();
                        if (!barSetupDone)
                        {
                            for (int i = 0; i < images.Length; i++)
                            {
                                if (images.Length == 5) barSetupDone = true;
                            }
                        }
                        if (barSetupDone)
                        {
                            meterBar = Instantiate(healthBar.gameObject, healthBar.gameObject.transform.parent);
                            meterBar.name = "MeterBar";
                            Destroy(meterBar.GetComponent<HealthBar>());
                            var texts = meterBar.GetComponentsInChildren<TextMeshProUGUI>();
                            for (int l = 0; l < texts.Length; l++)
                            {
                                if (texts[l] && texts[l].gameObject)
                                {
                                    if (texts[l].gameObject.name == "CurrentHealthText")
                                    {
                                        currentMeter = texts[l];
                                        currentMeter.text = "0";
                                    }
                                    if (texts[l].gameObject.name == "FullHealthText")
                                    {
                                        fullMeter = texts[l];
                                        fullMeter.text = maxMeterValue.ToString();
                                    }
                                }
                            }
                            var meterImages = meterBar.GetComponentsInChildren<Image>();
                            for (int t = 0; t < meterImages.Length; t++)
                            {
                                if (meterImages[t] && meterImages[t].gameObject)
                                {
                                    if (meterImages[t] != meterImages[3] && meterImages[t] != meterImages[0])
                                    {
                                        Destroy(meterImages[t].gameObject);
                                    }
                                    if (meterImages[t] == meterImages[3])
                                    {
                                        barImage = meterImages[t];
                                        barImage.color = barColor;
                                        barImage.type = Image.Type.Filled;
                                        barImage.fillMethod = Image.FillMethod.Horizontal;
                                        barImage.fillCenter = false;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        private void OnDisable()
        {
            On.RoR2.UI.HUD.Update -= HUD_Update;
            if (meterBar)
            {
                Destroy(meterBar);
            }
        }
    }
}
