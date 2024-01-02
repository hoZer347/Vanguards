using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanguards
{
	internal class EnemyManager : MonoBehaviour
	{
		static internal EnemyManager main;
		static internal List<Unit> units = new();

		private void Start()
		{
			main = GetComponent<EnemyManager>();

			foreach (Unit unit in GetComponentsInChildren<Unit>())
				units.Add(unit);
		}
	};
};
