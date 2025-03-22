using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


namespace Vanguards
{
	public class St_Mp_Option : St_MapState
	{
		virtual protected bool OptionIsAvailable(Unit unit)
		{
			return false;
		}

		static public bool OptionIsAvailable<_Option>(Unit unit)
			where _Option : St_Mp_Option, new()
		{
			_Option option = new(); // DON'T DO THIS IN ANY OTHER SITUATION, USE S
			return option.OptionIsAvailable(unit);
		}

		public override void OnUpdate()
		{
			SetState<St_End>();
		}
	};

	public class Op_Wait : St_Mp_Option
	{ };

	public class Op_Attack : St_Mp_Option
	{
		static Dictionary<Cell, float> attackRange;

		protected override bool OptionIsAvailable(Unit unit)
		{
			if (unit.AttackRange == 0)
				return false;



			return false;
		}

		public override void OnEnter()
		{
			OptionMenu.Clear();

			float maxAttackRange = selectedUnit.AttackRange;
			float minAttackRange = selectedUnit.MinAttackRange;

			attackRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], selectedUnit.AttackRange }
				};

			Algorithm.FloodFill(
				ref attackRange,
				(Cell cell, float amount) =>
				{
					return amount - 1;
				});

			attackRange.Remove(Map.main[selectedUnit.transform.position]);

			attackRange = attackRange
				.Where(attackRange => attackRange.Value <= maxAttackRange - minAttackRange)
				.ToDictionary(
					attackRange => attackRange.Key,
					attackRange => attackRange.Value);

			Color[] colors = meshFilter.sharedMesh.colors;
			
			foreach (Cell cell in Map.main.CellList)
				if (attackRange.ContainsKey(cell))
				{
					colors[cell.MeshIndex + 0] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 1] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 2] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 3] = new Color(1, 0, 0, 0.5f);
				}
				else
				{
					colors[cell.MeshIndex + 0] = new();
					colors[cell.MeshIndex + 1] = new();
					colors[cell.MeshIndex + 2] = new();
					colors[cell.MeshIndex + 3] = new();
				};

			meshFilter.sharedMesh.colors = colors;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();
		}

		public override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
				SetState<St_End>();

			if (Input.GetKeyDown(KeyCode.Escape))
				SetState<St_Mp_InitialState>();
		}
	};

	public class Op_Staff : St_Mp_Option
	{
		static Dictionary<Cell, float> staffRange;

		public override void OnEnter()
		{
			OptionMenu.Clear();

			float maxStaffRange = selectedUnit.StaffRange;
			float minStaffRange = selectedUnit.MinStaffRange;

			staffRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], selectedUnit.StaffRange }
				};

			Algorithm.FloodFill(
				ref staffRange,
				(Cell cell, float amount) =>
				{
					return amount - 1;
				});

			staffRange.Remove(Map.main[selectedUnit.transform.position]);

			staffRange = staffRange
				.Where(staffRange => staffRange.Value <= maxStaffRange - minStaffRange)
				.ToDictionary(
					staffRange => staffRange.Key,
					staffRange => staffRange.Value);

			Color[] colors = meshFilter.sharedMesh.colors;

			foreach (Cell cell in Map.main.CellList)
				if (staffRange.ContainsKey(cell))
				{
					colors[cell.MeshIndex + 0] = new Color(0, 1, 0, 0.5f);
					colors[cell.MeshIndex + 1] = new Color(0, 1, 0, 0.5f);
					colors[cell.MeshIndex + 2] = new Color(0, 1, 0, 0.5f);
					colors[cell.MeshIndex + 3] = new Color(0, 1, 0, 0.5f);
				}
				else
				{
					colors[cell.MeshIndex + 0] = new();
					colors[cell.MeshIndex + 1] = new();
					colors[cell.MeshIndex + 2] = new();
					colors[cell.MeshIndex + 3] = new();
				};

			meshFilter.sharedMesh.colors = colors;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();
		}

		public override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
				SetState<St_End>();

			if (Input.GetKeyDown(KeyCode.Escape))
				SetState<St_Mp_InitialState>();
		}
	};

	public class Op_Equip : St_Mp_Option
	{ };

	public class Op_Item : St_Mp_Option
	{ };
};
