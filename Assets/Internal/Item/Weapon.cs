using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class Weapon : Equippable
	{
		[HideInInspector] public Attribute<int> PWR;
	};
};
