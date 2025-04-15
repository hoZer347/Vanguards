using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class Equippable : Item
	{
		[HideInInspector] public Attribute<int> RNG;
		[HideInInspector] public Attribute<int> MIN_RNG;

		virtual public void OnEquip()
		{ }

		virtual public void OnUnequip()
		{ }
	};
};
