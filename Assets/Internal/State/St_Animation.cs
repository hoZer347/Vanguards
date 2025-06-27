using System;
using System.Collections;
using System.Collections.Generic;


namespace Vanguards
{
	public class St_Animation : State
	{
		protected bool shouldAnimate = true;
	};

	public class An_Attack : St_Animation
	{
		static Dictionary<(Unit, Unit), An_Attack> savedAttacks = new();

		Unit attacker = null;
		Unit receiver = null;
		HealthBar receiverHealthBar = null;

		string modifierID;
		int totalDamage = -1;
		
		System.Random random = new System.Random();
		int randomSeed;

		public An_Attack(
			Unit attacker,
			Unit receiver)
		{
			this.attacker = attacker;
			this.receiver = receiver;
			receiverHealthBar = this.receiver.GetComponentInChildren<HealthBar>();
			randomSeed = random.Next(0, 1000);
		}

		override public void OnUpdate()
		{
			if (totalDamage == -1)
			{
				totalDamage = (attacker.equipped as Weapon).CalculateDamage(attacker, receiver);
				modifierID = receiver.HP.SetModifier((ref int hp) => hp -= totalDamage);
			}
			else receiver.HP.SetModifier(modifierID, (ref int hp) => hp -= totalDamage);

			receiverHealthBar.Refresh();

			SetState(new St_Mp_InitialState());
		}

		public override void OnUndo()
		{
			receiver.HP.RmvModifier(modifierID);
			receiverHealthBar.Refresh();
		}
	};
};
