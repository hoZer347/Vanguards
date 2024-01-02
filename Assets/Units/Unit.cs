using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Vanguards
{
	enum Team
	{
		Null,
		Player,
		Enemy,
		Ally,
	};

	[Serializable]
	internal class Unit : Attributes
	{
		#region Highlighting Stuff

		Outline outline;

		Color originalColor;

		[SerializeField]
		float highlightWidth = 2.0f;
		internal bool Highlighted
		{
			get => outline.OutlineWidth > 0;
			set
			{
				if (value)
					outline.OutlineWidth = highlightWidth;
				else
					outline.OutlineWidth = 0;
			}
		}

		static internal void UnHighlightAll()
		{
			foreach (Unit unit in UnitManager.units)
				unit.Highlighted = false;
		}

		#endregion

		[SerializeField]
		internal Vector2Int spawnCoords;

		[SerializeField]
		bool done;
		internal bool Done
		{
			get => done;
			set
			{
				if (!done && value)
				{
					originalColor = GetComponent<MeshRenderer>().material.color;
					GetComponent<MeshRenderer>().material.color = new Color(.5f, .5f, .5f, originalColor.a);
				};

				if (done && !value)
				{
					GetComponent<MeshRenderer>().material.color = originalColor;
				};

				done = value;
			}
		}

		internal Vector2Int Coords
		{
			get => new Vector2Int((int)transform.position.x, (int)transform.position.z);
			set => transform.position = new Vector3(value.x + 0.5f, transform.position.y, value.y + 0.5f);
		}

		[SerializeField] internal Team team = Team.Null;

		internal List<Option> GetOptions()
		{
			List<Option> options = new();

			options.Add(new Wait());
			options.Add(new UseWeapon());

			return options;
		}

		internal Cell cell => Map.main[(int)transform.position.x, (int)transform.position.z];

		private void Start()
		{
			outline = GetComponent<Outline>();
		}

		internal byte GetAttackRange()
		{
			byte max_rng = 0;

			foreach (Weapon weapon in weapons)
				if (weapon != null)
					max_rng = Math.Max(max_rng, weapon.GetValue("RNG"));

			return max_rng;
		}

		internal byte GetStaffRange() // TODO: Allow 
		{
			byte max_rng = 0;

			foreach (Staff staff in staves)
				if (staff != null)
					max_rng = Math.Max(max_rng, staff.GetValue("RNG"));

			return max_rng;
		}

		internal byte GetMoveRange()
		{
			return GetValue("MOV");
		}

		[SerializeField] Attributes equipped;
		[SerializeField] List<Weapon> weapons;
		[SerializeField] List<Staff> staves;
	};
};
