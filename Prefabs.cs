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
using ThreeEyedGames;
using RoR2.CharacterAI;
using UnityEngine.Rendering.PostProcessing;

namespace Twins
{
    class Prefabs
    {
        internal static GameObject voidProjectileSimpleGhost;
        internal static GameObject voidProjectileSimple;
        internal static GameObject voidTracer;
        internal static GameObject beetleWard;
        internal static GameObject boulderProjectileGhost;
        internal static GameObject boulderProjectile;
        internal static GameObject boulderChargeEffect;
        internal static GameObject lightningImpactEffect;
        internal static GameObject gravityVoidballProjectile;
        internal static GameObject iceDotZone;
        internal static GameObject forceField;
        internal static GameObject wispMaster;
        internal static GameObject gupMaster;
        internal static GameObject chargeSunEffect;
        internal static GameObject sunStreamEffect;
        internal static GameObject sun;
        internal static GameObject sunExplosion;
        internal static GameObject crosshair;
        internal static GameObject hauntedWard;
        internal static DeployableSlot wisp;
        internal static DeployableSlot gup;
        internal static int WispCount(CharacterMaster master, int count)
        {
            return 4;
        }
        internal static int GupCount(CharacterMaster master, int count)
        {
            return 1;
        }
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        internal static void CreatePrefabs()
        {
            wisp = DeployableAPI.RegisterDeployableSlot(new DeployableAPI.GetDeployableSameSlotLimit(WispCount));
            gup = DeployableAPI.RegisterDeployableSlot(new DeployableAPI.GetDeployableSameSlotLimit(GupCount));

            hauntedWard = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab"), "TwinsHauntedWard", true);
            hauntedWard.GetComponentInChildren<ParticleSystemRenderer>().enabled = false;

            crosshair = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/UI/SprintingCrosshair.prefab"), "TwinsCrosshair", false);
            crosshair.GetComponent<RawImage>().color = Color.white;

            wispMaster = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Wisp/WispMaster.prefab"), "TwinsWispMaster", true);
            wispMaster.AddComponent<DeployableBehaviour>().deployableType = DeployableBehaviour.DeployableType.Wisp;
            ContentAddition.AddMaster(wispMaster);

            Load<GameObject>("RoR2/DLC1/Gup/GeepMaster.prefab").AddComponent<AIOwnership>();
            Load<GameObject>("RoR2/DLC1/Gup/GipMaster.prefab").AddComponent<AIOwnership>();
            gupMaster = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab"), "TwinsGupMaster", true);
            gupMaster.GetComponent<CharacterMaster>().destroyOnBodyDeath = false;
            gupMaster.AddComponent<DeployableBehaviour>().deployableType = DeployableBehaviour.DeployableType.Gup;
            ContentAddition.AddMaster(gupMaster);

            chargeSunEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/ChargeGrandParentSunHands.prefab"), "SunCharge", false);
            chargeSunEffect.transform.localScale = Vector3.one * 0.5f;
            chargeSunEffect.GetComponentInChildren<ObjectScaleCurve>().transform.localScale = Vector3.one * 1.5f;
            var sunCL = chargeSunEffect.GetComponentInChildren<Light>();
            sunCL.color = new Color(0.45f, 0, 1);
            sunCL.useColorTemperature = false;
            var Sunmesh = chargeSunEffect.GetComponentInChildren<MeshRenderer>(true);
            Sunmesh.gameObject.SetActive(true);
            Sunmesh.material = new Material(Sunmesh.material);
            Sunmesh.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampBottledChaos.png"));
            var sunP = chargeSunEffect.GetComponentInChildren<ParticleSystemRenderer>(true);
            sunP.material = new Material(sunP.material);
            sunP.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
            sunP.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));

            sunStreamEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSunChannelStartStream.prefab"), "SunStream", false);
            sunStreamEffect.GetComponent<ChildLocator>().FindChild("EndPoint").gameObject.AddComponent<DestroyOnDestroy>().target = sunStreamEffect;
            UnityEngine.Object.Destroy(sunStreamEffect.GetComponentInChildren<MeshRenderer>(true).gameObject);
            var sunSP = sunStreamEffect.GetComponentsInChildren<ParticleSystemRenderer>(true)[1];
            sunSP.transform.localScale = Vector3.one * 0.5f;
            var m = sunSP.trailMaterial;
            sunSP.trailMaterial = new Material(m);
            sunSP.trailMaterial.SetColor("_TintColor", new Color(0.45f, 0, 1));
            sunSP.trailMaterial.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));

            sun = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSun.prefab"), "Sun", true);
            UnityEngine.Object.Destroy(sun.GetComponent<EntityStateMachine>());
            UnityEngine.Object.Destroy(sun.GetComponent<NetworkStateMachine>());
            var sunL = sun.GetComponentInChildren<Light>();
            sunL.intensity = 100;
            sunL.range = 40;
            sunL.color = new Color(0.45f, 0, 1);
            var sunMeshes = sun.GetComponentsInChildren<MeshRenderer>(true);
            var sunIndicator = sunMeshes[0];
            sunIndicator.material = new Material(sunIndicator.material);
            sunIndicator.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampPortalVoid.png"));
            sunIndicator.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
            var Sunmesh2 = sunMeshes[1];
            Sunmesh2.material = Sunmesh.material;
            sunMeshes[2].enabled = false;
            var sunPP = sun.GetComponentInChildren<PostProcessVolume>();
            sunPP.profile = Load<PostProcessProfile>("RoR2/Base/title/ppLocalNullifier.asset");
            sunPP.sharedProfile = sunPP.profile;
            sunPP.gameObject.AddComponent<SphereCollider>().radius = 40;
            foreach (ParticleSystemRenderer r in sun.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var name = r.name;
                if (name == "GlowParticles, Fast" || name == "GlowParticles")
                {
                    r.material = sunP.material;
                }
                if (name == "SoftGlow, Backdrop")
                {
                    r.material = new Material(Load<Material>("RoR2/Junk/Common/VFX/matTeleportOutBodyGlow.mat"));
                    r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                }
                if (name == "Donut" || name == "Trails")
                {
                    r.material = new Material(r.material);
                    r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                    r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
                    r.trailMaterial = r.material;
                }
                if (name == "Goo, Drip")
                {
                    r.enabled = false;
                }
            }

            sunExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSunSpawn.prefab"), "SunExplosion", false);
            var sunePP = sunExplosion.GetComponentInChildren<PostProcessVolume>();
            sunePP.profile = sunPP.profile;
            sunePP.sharedProfile = sunPP.profile;
            var suneL = sunExplosion.GetComponentInChildren<Light>();
            suneL.intensity = 100;
            suneL.range = 40;
            suneL.color = new Color(0.45f, 0, 1);
            foreach (ParticleSystemRenderer r in sunExplosion.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                var name = r.name;
                if (r.material)
                {
                    r.material = new Material(r.material);
                    r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                    r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
                    r.trailMaterial = r.material;
                }
            }
            ContentAddition.AddEffect(sunExplosion);

            lightningImpactEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Lightning/LightningStrikeImpact.prefab"), "LightningImpactEffect", false);
            // this effect doesn't scale properly with the correct radius, so at 4 it matches 18 radius
            lightningImpactEffect.transform.localScale = Vector3.one * 4;
            ContentAddition.AddEffect(lightningImpactEffect);

            forceField = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MajorConstructBubbleShield.prefab"), "ForceField", true);
            forceField.GetComponentInChildren<MeshCollider>().gameObject.layer = 0;
            UnityEngine.Object.Destroy(forceField.GetComponent<NetworkedBodyAttachment>());
            UnityEngine.Object.Destroy(forceField.GetComponent<VFXAttributes>());
            var ffScale = forceField.GetComponentInChildren<ObjectScaleCurve>();
            ffScale.useOverallCurveOnly = true;
            ffScale.overallCurve = AnimationCurve.Linear(0, 0.35f, 1, 1);
            forceField.AddComponent<DestroyOnTimer>().duration = Utility3.forcefieldDuration;
            var forceMesh = forceField.GetComponentInChildren<MeshRenderer>();
            var forceMaterials = forceMesh.sharedMaterials;
            forceMesh.sharedMaterials[0] = new Material(forceMaterials[0]);
            forceMesh.sharedMaterials[0].SetColor("_TintColor", new Color(0.07843f, 0, 1));
            forceMesh.sharedMaterials[0].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonLighting.png"));
            forceMesh.sharedMaterials[1] = new Material(forceMaterials[1]);
            forceMesh.sharedMaterials[1].SetColor("_TintColor", new Color(0.39215f, 0, 1));
            forceMesh.sharedMaterials[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampHippoVoidEye.png"));
            var forceP = forceField.GetComponentInChildren<ParticleSystemRenderer>();
            forceP.material = new Material(forceP.material);
            forceP.material.SetColor("_TintColor", new Color(0.07843f, 0.02745f, 1));
            forceP.material.DisableKeyword("VERTEXCOLOR");

            voidProjectileSimpleGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigGhost.prefab"), "VoidProjectileSimpleGhost", false);
            Utils.AddScaleComponent(voidProjectileSimpleGhost, 0.12f);

            iceDotZone = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarExploder/LunarExploderProjectileDotZone.prefab"), "IceDotZone", true);
            iceDotZone.GetComponent<ProjectileDotZone>().onEnd.m_PersistentCalls = new UnityEngine.Events.PersistentCallGroup();
            foreach (ParticleSystemRenderer r in iceDotZone.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                r.enabled = false;
            }
            iceDotZone.GetComponentInChildren<Decal>().enabled = false;
            var effect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab").transform.GetChild(0).gameObject, "IceDotEffect", false);
            effect.transform.parent = iceDotZone.transform;
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localScale = Vector3.one;
            foreach (ParticleSystem p in effect.GetComponentsInChildren<ParticleSystem>())
            {
                p.transform.localScale = Vector3.one * 22;
                var main = p.main;
                main.loop = true;
                main.playOnAwake = true;
                var r = p.GetComponent<ParticleSystemRenderer>();
                r.materials = new Material[] { r.material, r.material, r.material, r.material, r.material, r.material, r.material, r.material };
            }
            ContentAddition.AddProjectile(iceDotZone);

            gravityVoidballProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"), "VoidProjectileSimple", true);
            var GvbiMPACT = gravityVoidballProjectile.GetComponent<ProjectileImpactExplosion>();
            GvbiMPACT.fireChildren = true;
            GvbiMPACT.childrenCount = 1;
            GvbiMPACT.childrenDamageCoefficient = 1;
            GvbiMPACT.childrenProjectilePrefab = Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab");
            var gVBSimple = gravityVoidballProjectile.GetComponent<ProjectileSimple>();
            gVBSimple.desiredForwardSpeed = 40;
            gVBSimple.lifetime = 10;
            gVBSimple.updateAfterFiring = false;
            gravityVoidballProjectile.GetComponent<Rigidbody>().useGravity = true;
            var antiGravity = gravityVoidballProjectile.AddComponent<AntiGravityForce>();
            antiGravity.rb = gravityVoidballProjectile.GetComponent<Rigidbody>();
            antiGravity.antiGravityCoefficient = 0.7f;
            ContentAddition.AddProjectile(gravityVoidballProjectile);

            voidProjectileSimple = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"), "VoidProjectileSimple", true);
            voidProjectileSimple.GetComponent<ProjectileController>().ghostPrefab = voidProjectileSimpleGhost;
            var rb = voidProjectileSimple.GetComponent<Rigidbody>();
            rb.useGravity = true;
            var antiGrav = voidProjectileSimple.AddComponent<AntiGravityForce>();
            antiGrav.rb = rb;
            antiGrav.antiGravityCoefficient = 0.7f;
            ContentAddition.AddProjectile(voidProjectileSimple);

            voidTracer = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab"), "VoidTracer", false);
            var particles = voidTracer.GetComponentsInChildren<ParticleSystemRenderer>();
            particles[particles.Length - 1].transform.localScale = new Vector3(0, 0, 0.25f);
            UnityEngine.Object.Destroy(voidTracer.GetComponentInChildren<ShakeEmitter>());

            beetleWard = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Beetle/BeetleWard.prefab"), "VoidWard", true);
            Material impMat = new Material(Load<Material>("RoR2/Base/Imp/matImpBoss.mat"));
            impMat.SetFloat("_Cull", 0);
            beetleWard.GetComponentInChildren<SkinnedMeshRenderer>().material = impMat;
            beetleWard.AddComponent<DestroyOnTimer>().duration = 10;
            var beetleWParticles = beetleWard.GetComponentsInChildren<ParticleSystemRenderer>();
            beetleWParticles[0].material = Load<Material>("RoR2/DLC1/PortalVoid/matPortalVoid.mat");
            beetleWParticles[1].enabled = false;
            beetleWard.GetComponentInChildren<MeshRenderer>().material = Load<Material>("RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat");

            boulderProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab"), "BoulderProjectileGhost", false);
            boulderProjectileGhost.transform.localScale = Vector3.one * 0.3f; 

            boulderChargeEffect = PrefabAPI.InstantiateClone(boulderProjectileGhost, "BoulderChargeEffect", false);
            UnityEngine.Object.Destroy(boulderChargeEffect.GetComponent<ProjectileGhostController>());
            Utils.AddScaleComponent(boulderChargeEffect, 0.2f);

            boulderProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab"), "BoulderProjectile", true);
            boulderProjectile.transform.localScale = Vector3.one * 0.3f;
            boulderProjectile.GetComponent<ProjectileController>().ghostPrefab = boulderProjectileGhost;
            boulderProjectile.GetComponent<ProjectileImpactExplosion>().bonusBlastForce = new Vector3(0, 900, 0);
            ContentAddition.AddProjectile(boulderProjectile);
        }
        [SystemInitializer(typeof(BuffCatalog))]
        private static void BuffCatalogInit()
        {
            beetleWard.GetComponent<BuffWard>().buffDef = RoR2Content.Buffs.LifeSteal;
        }
    }
}