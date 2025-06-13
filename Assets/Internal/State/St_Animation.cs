using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class St_Animation : State
	{ };

	public class An_Attack : St_Animation
	{
		Unit attacker = null;
		Unit receiver = null;
		HealthBar receiverHealthBar = null;

		string damageID = Guid.NewGuid().ToString("N");

		public An_Attack(
			Unit attacker,
			Unit receiver)
		{
			this.attacker = attacker;
			this.receiver = receiver;
			receiverHealthBar = this.receiver.GetComponentInChildren<HealthBar>();
		}

		override public void OnEnter()
		{ }

		int totalDamage;

		override public void OnUpdate()
		{
			totalDamage = (attacker.equipped as Weapon).CalculateDamage(attacker, receiver);

			receiver.HP.SetModifier($"{ totalDamage } Damage from { attacker.NAME.Value } ({ damageID })",
				(ref int hp) => { hp -= totalDamage; });
			attacker.ActionUsed = true;
			SetState(new St_Mp_InitialState());
			receiverHealthBar.Refresh();
		}

		public override void OnUndo()
		{
			receiver.HP.RmvModifier($"{ totalDamage } Damage from { attacker.NAME.Value } ({ damageID })");
			receiverHealthBar.Refresh();
		}
	};
};
