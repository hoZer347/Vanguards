using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class St_Option : St_MapState
	{
		public St_Option(Unit unit)
			=> this.selectedUnit = unit;

		protected string displayName;
		protected Unit selectedUnit;
		
		override public void OnUpdate()
		{
			if (Input.GetMouseButtonDown(1))
			{
				Menu[] menus = GameObject.FindObjectsByType<Menu>(FindObjectsSortMode.None);
				foreach (Menu optionMenu in menus)
					optionMenu.ClearOptions();
				FallBack<St_Mp_ChooseAnOption>();	
			};

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Menu[] menus = GameObject.FindObjectsByType<Menu>(FindObjectsSortMode.None);
				foreach (Menu optionMenu in menus)
					optionMenu.ClearOptions();
				FallBack<St_Mp_InitialState>();
			};
		}

		override public void OnLeave()
		{ }
	};

	public class Op_Wait : St_Option
	{
		public Op_Wait(Unit unit) : base(unit)
		{ }

		override public void OnEnter()
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

		override public void OnUpdate()
			=> SetState(new St_Mp_End(selectedUnit));
	};

	public class Op_Attack : St_Option
	{
		static Dictionary<Cell, float> attackRange;

		Unit receiver;

		public Op_Attack(Unit attacker, Unit receiver) : base(attacker)
		{
			selectedUnit = attacker;
			this.receiver = receiver;
		}

		override public void OnEnter()
		{
			var (minAttackRange, maxAttackRange) = selectedUnit.GetAttackRange();

			(int, int) range = selectedUnit.GetAttackRange();

			attackRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], range.Item2 + 1 }
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

			WeaponSelectionMenu.instance.DisplayWeaponOptions(selectedUnit, receiver);
		}

		override public void OnLeave()
			=> WeaponSelectionMenu.instance.ClearOptions();

		override public void OnUndo()
			=> WeaponSelectionMenu.instance.ClearOptions();
	};

	public class Op_Staff : St_Option
	{
		static Dictionary<Cell, float> staffRange;

		Unit receiver;

		public Op_Staff(Unit unit, Unit receiver) : base(unit)
			=> this.receiver = receiver;

		override public void OnEnter()
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

		override public void OnUpdate()
		{
			selectedUnit.equipped = equippable;

			if (equippable != null)
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
