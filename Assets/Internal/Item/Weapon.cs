using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


#endif


namespace Vanguards
{
	public class Weapon : Equippable
	{
		[HideInInspector] public Attribute<int> PWR;
		[HideInInspector] public Attribute<int> RNG;
		[HideInInspector] public Attribute<int> MIN_RNG;
	};
};
