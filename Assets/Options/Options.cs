using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	[Serializable]
	internal class Option : MapState
	{
		protected override void OnUpdate() => Current = new SelectAUnit();
	}

	[Serializable]
	internal class UseWeapon : Option
	{
		int targetUnitCycleIndex = 0;
		List<Unit> validTargets = new();
		Unit targetUnit = null;
		Unit TargetUnit
		{
			get => targetUnit;
			set
			{
				if (targetUnit != null)
					targetUnit.Highlighted = false;
				targetUnit = value;
				targetUnit.Highlighted = true;

				_ContextMenu.LoadAttackUI(selectedUnit, targetUnit);
			}
		}

		protected override void OnEnter()
		{
			_ContextMenu.Clear();
			Cell.UnHighlightAll();

			Map.HighighUnitRange(selectedUnit, true);

			foreach (Cell cell in Map.main.cells)
				if (cell.unit != null &&
					cell.unit != selectedUnit &&
					cell.Attackable &&
					cell.unit.team != selectedUnit.team)
					validTargets.Add(cell.unit);
		}

		protected override void OnUpdate()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			foreach (Unit unit in UnitManager.units)
				unit.Highlighted = false;

			if (TargetUnit != null)
				TargetUnit.Highlighted = true;

			if (Physics.Raycast(ray, out hit))
			{
				Unit unit = hit.transform.gameObject.GetComponent<Unit>();
				if (unit != null && validTargets.Contains(unit))
				{
					TargetUnit = unit;
					targetUnitCycleIndex = 0;
				};

				// LMB opens the UI
				if (Input.GetMouseButtonDown(0) && unit != null && validTargets.Contains(unit))
					TargetUnit = unit;
			};

			// Tab cycles through targets
			if (Input.GetKeyDown(KeyCode.Tab) && validTargets.Count > 0)
			{
				TargetUnit = validTargets[targetUnitCycleIndex];
				targetUnitCycleIndex++;
				if (targetUnitCycleIndex >= validTargets.Count)
					targetUnitCycleIndex = 0;
			};

			// RMB click goes back to option select
			if (Input.GetMouseButtonDown(1))
				Current = new SelectAnOption();
		}

		protected override void OnLeave()
		{
			_ContextMenu.Clear();
			Cell.UnHighlightAll();
			Unit.UnHighlightAll();
			validTargets.Clear();
		}
	};

	[Serializable]
	internal class AttackAnimation : Option
	{
		Unit targetUnit;

		internal AttackAnimation(Unit targetUnit) => this.targetUnit = targetUnit;

		protected override void OnEnter()
		{

		}

		protected override void OnUpdate()
		{
			Current = new UnitEndedAction();
		}

		protected override void OnLeave()
		{
			
		}
	};

	[Serializable]
	internal class UseStaff : Option
	{
		// TODO: Implement this
	};

	[Serializable]
	internal class UseSpell : Option
	{
		// TODO: Implement this
	};

	[Serializable]
	internal class Equip : Option
	{
		// TODO: Implement this
	};

	[Serializable]
	internal class Wait : Option
	{
		protected override void OnUpdate()
		{
			// TODO: Turn to grayscale
			Current = new UnitEndedAction();
		}
	};
};
