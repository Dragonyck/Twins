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
	class CharacterMain : GenericCharacterMain
	{
		private EntityStateMachine jetpackStateMachine;
		public override void OnEnter()
		{
			base.OnEnter();
			this.jetpackStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jet");
		}
		public override void OnExit()
		{
			if (base.isAuthority && this.jetpackStateMachine)
			{
				this.jetpackStateMachine.SetNextState(new Idle());
			}
			base.OnExit();
		}
		public override void ProcessJump()
		{
			base.ProcessJump();
			if (this.hasCharacterMotor && this.hasInputBank && base.isAuthority)
			{
				bool canHover = base.inputBank.jump.down && base.characterMotor.velocity.y < 0f && !base.characterMotor.isGrounded;
				bool isHovering = this.jetpackStateMachine.state.GetType() == typeof(Hover);
				if (canHover && !isHovering)
				{
					this.jetpackStateMachine.SetNextState(new Hover());
				}
				else
				{
					this.jetpackStateMachine.SetNextState(new Idle());
				}
			}
		}
	}
	class Hover : BaseState
	{
		private float hoverVelocity = -0.3f;//below negative increases downard velocity, so increase towards positive numbers to hover longer
		private float hoverAcceleration = 60;
        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				float num = base.characterMotor.velocity.y;
				num = Mathf.MoveTowards(num, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
				base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
			}
		}
	}
}
