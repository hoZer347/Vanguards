using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanguards
{
	internal class PartyManager : MonoBehaviour
	{
		static internal PartyManager main;
		static internal List<Unit> units = new();

		private void Start()
		{
			main = GetComponent<PartyManager>();

			foreach (Unit unit in GetComponentsInChildren<Unit>())
				units.Add(unit);
		}
	};
};
