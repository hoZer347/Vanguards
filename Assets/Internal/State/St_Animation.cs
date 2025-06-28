using System;
using System.Collections;
using System.Collections.Generic;


namespace Vanguards
{
	public class St_Animation : WeakState
	{ };

	public class An_Attack : St_Animation
	{
		static Dictionary<(Unit, Unit), An_Attack> savedAttacks = new();

		Unit attacker = null;
		Unit receiver = null;
		HealthBar receiverHealthBar = null;

		string modifierID;
		int totalDamage = -1;
		
		public An_Attack(
			Unit attacker,
			Unit receiver)
		{
			this.attacker = attacker;
			this.receiver = receiver;
			receiverHealthBar = this.receiver.GetComponentInChildren<HealthBar>();
			//randomSeed = random.Next(0, 1000);
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

			if (receiver.HP.Value <= 0)
				receiver.transform.gameObject.SetActive(false);

			SetState(new St_Mp_InitialState());

			attacker.ActionUsed = true;
		}

		override public void OnUndo()
		{
			receiver.HP.RmvModifier(modifierID);
			receiverHealthBar.Refresh();

			if (receiver.HP.Value > 0)
				receiver.transform.gameObject.SetActive(true);

			attacker.ActionUsed = false;
		}
	};

	public class An_Spell : St_Animation
	{

	}
};
