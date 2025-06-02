using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class Equippable : Item
	{
		[HideInInspector] public Attribute<int> RNG;		// Maximum Range of the equippable
		[HideInInspector] public Attribute<int> MIN_RNG;	// Minimum Range of the equippable
		[HideInInspector] public Attribute<string> TYPE;	// Dagger, Potion, Key Item, etc...

		virtual public bool DetermineEquippable(Unit unit) => true;

		virtual public void OnEquip()
		{ }

		virtual public void OnUnequip()
		{ }

		virtual public void OnAdd()
		{ }

		virtual public void OnRemove()
		{ }
	};
};
