using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	internal class MapState : State<MapState>
	{
		static protected Unit selectedUnit = null;
		static protected Vector3 originalPosition = new();
	};

	internal class SelectAUnit : MapState
	{
		protected override void OnEnter()
		{
			// Resetting Everything
			if (selectedUnit != null)
				selectedUnit.transform.position = originalPosition;
			originalPosition = new();
			//

			Cell.UnHighlightAll();
		}

		protected override void OnUpdate()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			foreach (Unit unit in UnitManager.units)
				unit.Highlighted = false;

			if (Physics.Raycast(ray, out hit))
			{
				Unit unit = hit.transform.GetComponent<Unit>();
				if (unit != null)
				{
					unit.Highlighted = true;
					_ContextMenu.LoadAttributes(unit);

					if (Input.GetMouseButtonDown(0) &&
						unit.team == Team.Player &&
						!unit.Done)
					{
						originalPosition = unit.transform.position;
						selectedUnit = unit;
						Current = new SelectADestination();
						return;
					};
				};
			};
		}
	};

	internal class SelectADestination : MapState
	{
		int originalLayer;
		static Dictionary<Cell, float> highlightedCells = new();

		protected override void OnEnter()
		{
			selectedUnit.Highlighted = true;
			selectedUnit.transform.position = originalPosition;
			originalLayer = selectedUnit.transform.gameObject.layer;
			selectedUnit.transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

			Map.HighighUnitRange(selectedUnit);
		}

		protected override void OnUpdate()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				Cell cell = Map.main[(int)hit.point.x, (int)hit.point.z];
				if (cell != null)
				{
					selectedUnit.transform.position = hit.point;

					if (Input.GetMouseButtonDown(0) &&
						selectedUnit.cell.Reachable)
					{
						selectedUnit.Coords = new Vector2Int((int)hit.point.x, (int) hit.point.z);
						Current = new SelectAnOption();
						return;
					};
				};
			};

			if (Input.GetMouseButtonDown(1))
			{
				Current = new SelectAUnit();
				return;
			};
		}

		protected override void OnLeave()
		{
			selectedUnit.transform.gameObject.layer = originalLayer;
			highlightedCells.Clear();
			Cell.UnHighlightAll();
		}
	};

	internal class SelectAnOption : MapState
	{
		Unit hoveredUnit = null;

		protected override void OnEnter()
		{
			_ContextMenu.LoadUnitOptions(selectedUnit);
			Map.HighighUnitRange(selectedUnit, true);
		}

		protected override void OnUpdate()
		{
			Unit.UnHighlightAll();

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				hoveredUnit = hit.transform.gameObject.GetComponent<Unit>();
				if (hoveredUnit != null)
				{
					hoveredUnit.Highlighted = true;

					if (Input.GetMouseButtonDown(0))
						switch (hoveredUnit.team)
						{
							case Team.Player:
								Current = new UseStaff();
								break;

							case Team.Enemy:
								Current = new UseWeapon();
								break;

							case Team.Ally:
								Current = new UseStaff();
								break;

							case Team.Null:
								break;
						};
				};
			};

			if (Input.GetMouseButtonDown(1))
			{
				Current = new SelectADestination();
				return;
			};
		}

		protected override void OnLeave()
		{
			_ContextMenu.Clear();
			Unit.UnHighlightAll();
			Cell.UnHighlightAll();
		}
	};

	internal class UnitEndedAction : MapState
	{
		protected override void OnUpdate()
		{
			originalPosition = selectedUnit.transform.position;
			selectedUnit.Done = true;

			foreach (Unit unit in PartyManager.units)
				if (!unit.Done)
				{
					Current = new SelectAUnit();
					return;
				};

			foreach (Unit unit in PartyManager.units)
				unit.Done = false;

			Current = new EndTurn();
		}
	}

	internal class EndTurn : MapState
	{
		protected override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
				Current = new SelectAUnit();
		}

		protected override void OnLeave()
		{
			foreach (Unit unit in PartyManager.units)
				unit.Done = false;
		}
	}
};
