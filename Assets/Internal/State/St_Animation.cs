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
		int totalDamage = 0;
		
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

		override public void OnEnter()
			=> attacker.HP.RmvModifier("Damage Preview");

		override public void OnUpdate()
		{
			if (!shouldAnimate)
			{
				shouldAnimate = !shouldAnimate;
				Undo();
				return;
			};

			if (savedAttacks.TryGetValue(
				(attacker, receiver),
				out An_Attack attack) &&
				this != attack)
			{
				attack.OnUpdate();
				return;
			};

			shouldAnimate = !shouldAnimate;

			totalDamage = (attacker.equipped as Weapon).CalculateDamage(attacker, receiver);

			modifierID = receiver.HP.SetModifier((ref int hp) => { hp -= totalDamage; });
			attacker.ActionUsed = true;
			SetState(new St_Mp_InitialState());
			receiverHealthBar.Refresh();

			savedAttacks.Add((attacker, receiver), this);

			if (receiver.HP.Value <= 0)
				receiver.transform.gameObject.SetActive(false); // TODO: Handle death animation
		}

		override public void OnLeave()
		{
			var key = (attacker, receiver);

			if (savedAttacks.ContainsKey(key))
				savedAttacks.Remove(key);
		}

		override public void OnUndo()
		{
			if (savedAttacks.TryGetValue(
				(attacker, receiver),
				out An_Attack attack) &&
				this != attack)
			{
				attack.OnUndo();
				return;
			};

			receiver.HP.RmvModifier(modifierID);
			receiverHealthBar.Refresh();
			receiver.transform.gameObject.SetActive(true);
			attacker.ActionUsed = false;
		}
	};
};
