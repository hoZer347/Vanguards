using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Vanguards
{
	public class St_Option : St_MapState
	{
		public St_Option(Unit unit)
			=> this.selectedUnit = unit;

		protected string displayName;
		protected Unit selectedUnit;

		public override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(1))
				FallBack<St_Mp_ChooseAnOption>();

			if (Input.GetKeyDown(KeyCode.Escape))
				FallBack<St_Mp_InitialState>();
		}

		override public void OnLeave()
		{

		}
	};

	public class Op_Wait : St_Option
	{
		public Op_Wait(Unit unit) : base(unit)
		{ }

		public override void OnEnter()
		{
			Color[] colors = meshFilter.sharedMesh.colors;

			foreach (Cell cell in Map.main.CellList)
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
			=> SetState(new St_Mp_End(selectedUnit));
	};

	public class Op_Attack : St_Option
	{
		static Dictionary<Cell, float> attackRange;
		Weapon weapon;

		public Op_Attack(Unit unit, Weapon weapon) : base(unit)
			=> this.weapon = weapon;

		public override void OnEnter()
		{
			var (minAttackRange, maxAttackRange) = selectedUnit.GetAttackRange();

			attackRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], weapon.RNG.Value + 1 }
				};

			Algorithm.PostFloodFillRange(
				ref attackRange,
				ref attackRange,
				(Cell cell) => true,
				minAttackRange,
				maxAttackRange);

			attackRange.Remove(Map.main[selectedUnit.transform.position]);

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
	};

	public class Op_Staff : St_Option
	{
		static Dictionary<Cell, float> staffRange;

		Staff staff;

		public Op_Staff(Unit unit, Staff staff) : base(unit)
			=> this.staff = staff;
		
		public override void OnEnter()
		{
			var (minStaffRange, maxStaffRange) = selectedUnit.GetStaffRange();

			staffRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], maxStaffRange }
				};

			Algorithm.PostFloodFillRange(
				ref staffRange,
				ref staffRange,
				(Cell cell) => true,
				minStaffRange,
				maxStaffRange);

			staffRange.Remove(Map.main[selectedUnit.transform.position]);

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
	};

	public class Op_Equip : St_Option
	{
		Equippable equippable;

		public Op_Equip(Unit unit, Equippable equippable) : base(unit)
			=> this.equippable = equippable;
		
		public override void OnUpdate()
		{
			selectedUnit.equipped = equippable;
			equippable.transform.SetAsLastSibling();
			FallBack<St_Mp_ChooseAnOption>();
		}
	};

	public class Op_Consumable : St_Option
	{
		public Op_Consumable(Unit unit) : base(unit)
		{ }
	};
};
