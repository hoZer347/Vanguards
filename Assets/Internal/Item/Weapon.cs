using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using System;
using static Vanguards.Weapon;



#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class Weapon : Equippable
	{
		public enum WeaponDMGType
		{
			PHYSICAL = 0,
			MAGICAL = 1,

		};

		[HideInInspector] public Attribute<int> PWR;
		[HideInInspector] public Attribute<WeaponDMGType> DMG_TYPE;

		Func<Weapon, Unit, Unit, int> damageCalculation =
			(Weapon weapon, Unit attacker, Unit receiver) =>
			{
				if (weapon.DMG_TYPE.Value == WeaponDMGType.PHYSICAL)
					return weapon.PWR.Value + attacker.STR.Value - receiver.DEF.Value;

				if (weapon.DMG_TYPE.Value == WeaponDMGType.MAGICAL)
					return weapon.PWR.Value + attacker.MAG.Value - receiver.RES.Value;

				return 0;
			};

		public int CalculateDamage(Unit attacker, Unit receiver)
			=> damageCalculation(this, attacker, receiver);
	};
};
