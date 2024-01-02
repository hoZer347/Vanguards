using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanguards
{
	internal class UnitManager : MonoBehaviour
	{
		static internal UnitManager main;
		static internal List<Unit> units = new();

		private void Start()
		{
			main = GetComponent<UnitManager>();

			foreach (Unit unit in GetComponentsInChildren<Unit>())
			{
				unit.Coords = unit.spawnCoords;
				units.Add(unit);
			};
		}

		private void OnValidate()
		{
			foreach (Unit unit in GetComponentsInChildren<Unit>())
			{
				unit.Coords = unit.spawnCoords;
			};
		}
	};
};
