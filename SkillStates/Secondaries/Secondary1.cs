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
    class Secondary1 : BaseTwinState
    {
        public virtual float duration
        {
            get
            {
                return 0.45f;
            }
        }
        public GameObject areaIndicator;
        public virtual float blastRadius
        {
            get
            {
                return 18;
            }
        }
        public virtual float maxDistance
        {
            get
            {
                return 1000;
            }
        }
        public virtual float blastamageCoefficient
        {
            get
            {
                return 4;
            }
        }
        public virtual int fireballCount
        {
            get
            {
                return 4;
            }
        }
        public virtual float fireballDamageCoefficient
        {
            get
            {
                return 4;
            }
        }
        public virtual string projectilePrefabPath
        {
            get
            {
                return "RoR2/Base/FireballsOnHit/FireMeatBall.prefab";
            }
        }
        public virtual string blastEffectPrefabPath
        {
            get
            {
                return "RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab";
            }
        }
        public virtual GameObject blastEffectPrefab
        {
            get
            {
                return null;
            }
        }
        public virtual bool useIndicator
        {
            get
            {
                return false;
            }
        }
        public Vector3 position;

        public override void OnEnter()
        {
            base.OnEnter();
            //remove authority check if you want other players to see it
            if (base.isAuthority)
            {
                if (useIndicator)
                {
                    areaIndicator = UnityEngine.Object.Instantiate(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                    areaIndicator.transform.localScale = Vector3.one * blastRadius;
                }
                else
                {
                    float num = 0f;
                    RaycastHit raycastHit;
                    if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(base.GetAimRay(), base.gameObject, out num), out raycastHit, maxDistance + num, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                    {
                        position = raycastHit.point;
                    }
                    FireAttack();
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (areaIndicator)
            {
                float num = 0f;
                RaycastHit raycastHit;
                if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(base.GetAimRay(), base.gameObject, out num), out raycastHit, maxDistance + num, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                {
                    position = raycastHit.point;
                    areaIndicator.transform.position = position;
                }
            }
        }
        public virtual bool ButtonDown()
        {
            return base.inputBank.skill2.down;
        }
        public virtual DamageType GetDamageType()
        {
            return DamageType.PercentIgniteOnHit;
        }
        public virtual void FireAttack()
        {
            if (position == Vector3.zero)
            {
                return;
            }
            new BlastAttack
            {
                attacker = base.gameObject,
                baseDamage = damageStat * blastamageCoefficient,
                baseForce = 1800,
                crit = base.RollCrit(),
                damageType = GetDamageType(),
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = 1,
                radius = blastRadius,
                position = position,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = base.teamComponent.teamIndex
            }.Fire();

            if (!projectilePrefabPath.IsNullOrWhiteSpace())
            {
                Vector3 direction = base.GetAimRay().direction;
                float angles = (float)(360 / fireballCount);
                for (int i = 0; i < fireballCount; i++)
                {
                    float pi = (float)i * 3.1415927f * 2f / (float)fireballCount;
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = Prefabs.Load<GameObject>(projectilePrefabPath),
                        position = (position + Vector3.up * 4.5f) + new Vector3(Mathf.Sin(pi), 0f, Mathf.Cos(pi)),
                        rotation = Util.QuaternionSafeLookRotation(direction),
                        procChainMask = default(ProcChainMask),
                        target = null,
                        owner = base.gameObject,
                        damage = fireballDamageCoefficient,
                        crit = base.RollCrit(),
                        force = 200f,
                        damageColorIndex = DamageColorIndex.Default,
                        speedOverride = RoR2Application.rng.RangeFloat(15, 30),
                        useSpeedOverride = true
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    direction.x += Mathf.Sin(pi + UnityEngine.Random.Range(-20, 20));
                    direction.z += Mathf.Cos(pi + UnityEngine.Random.Range(-20, 20));
                }
            }
            EffectManager.SpawnEffect(blastEffectPrefab ? blastEffectPrefab : Prefabs.Load<GameObject>(blastEffectPrefabPath), new EffectData()
            {
                origin = position,
                scale = blastRadius
            }, true);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && !ButtonDown())
            {
                this.outer.SetNextStateToMain();
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (position == Vector3.zero)
            {
                return;
            }
            if (useIndicator && base.isAuthority)
            {
                FireAttack();
            }
            if (areaIndicator)
            {
                Destroy(areaIndicator);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
