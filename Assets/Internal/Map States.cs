using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


namespace Vanguards
{
	abstract public class St_MapState : State
	{
		protected MeshFilter meshFilter = Map.main.GetComponent<MeshFilter>();
	};

	public class St_Mp_InitialState : St_MapState
	{
		Unit selectedUnit;
		Cell originalCell;

		public St_Mp_InitialState()
		{
			selectedUnit = null;
			originalCell = null;
		}

		public St_Mp_InitialState(Unit selectedUnit, Cell originalCell)
		{
			this.selectedUnit = selectedUnit;
			this.originalCell = originalCell;
		}

		public override void OnEnter()
		{
			if (selectedUnit != null)
			{
				selectedUnit.ActionUsed = false;

				selectedUnit.SetPath(new List<Vector3> { originalCell.Position });
				selectedUnit.transform.position = originalCell.Position;
			};

			selectedUnit = null;
			originalCell = null;

			OptionMenu.Clear();

			if (meshFilter.sharedMesh != null)
			{
				Color[] colors = meshFilter.sharedMesh.colors;

				for (int i = 0; i < colors.Length; i++)
					colors[i] = new();

				meshFilter.sharedMesh.colors = colors;
				meshFilter.sharedMesh.RecalculateBounds();
				meshFilter.sharedMesh.RecalculateNormals();
			};
		}

		public override void OnUpdate()
		{
			if (Physics.Raycast(
					Camera.main.ScreenPointToRay(Input.mousePosition),
					out RaycastHit hit))
			{
				if (Input.GetMouseButtonDown(0))
				{
					Cell cell = Map.main[hit.point];
					if (cell != null)
						selectedUnit = cell.Unit;

					Unit unit = hit.collider.GetComponent<Unit>();
					if (unit != null &&
						!unit.ActionUsed &&
						unit.Team == Unit.eTeam.Player)
					{
						selectedUnit = unit;
						originalCell = Map.main[unit.transform.position];

						SetState(new St_Mp_ChooseAPosition(selectedUnit, originalCell));
					};
				};
			};
		}
	};

	public class St_Mp_ChooseAPosition : St_MapState
	{
		protected Dictionary<Cell, float> moveRange = new();
		protected Dictionary<Cell, float> attackRange = new();
		protected Dictionary<Cell, float> staffRange = new();

		protected Cell goal;

		Unit selectedUnit;
		Cell originalCell;

		public St_Mp_ChooseAPosition(Unit selectedUnit, Cell originalCell)
		{
			this.selectedUnit = selectedUnit;
			this.originalCell = originalCell;
		}

		public override void OnEnter()
		{
			OptionMenu.Clear();

			// Assigning all units to their respective cells
			foreach (Cell cell in Map.main.Cells)
				if (cell != null)
					cell.Unit = null;

			foreach (Unit unit in Component.FindObjectsOfType<Unit>())
			{
				Cell cell = Map.main[unit.transform.position];
				if (cell != null)
					cell.Unit = unit;
			};
			//

			// Getting Move Range
			moveRange =
				new Dictionary<Cell, float>
				{
					{ originalCell, selectedUnit.MoveRange + 1 }
				};

			Algorithm.FloodFill(
				ref moveRange,
				(Cell cell, float amount) =>
				{
					if (cell.Unit != null &&
						cell.Unit.Team == Unit.eTeam.Enemy)
						return 0;

					return amount - cell.Difficulty;
				});
			//


			// Getting Attack Range
			int maxAttackRange = selectedUnit.GetAttackRange();

			attackRange =
				moveRange.ToDictionary(
					moveRange => moveRange.Key,
					moveRange => moveRange.Value * 0 + maxAttackRange + 1);

			Algorithm.FloodFill(ref attackRange, (Cell cell, float amount) => amount - 1);
			//


			// Getting Staff Range
			int maxStaffRange = selectedUnit.GetStaffRange();

			staffRange =
				moveRange.ToDictionary(
					moveRange => moveRange.Key,
					moveRange => moveRange.Value * 0 + maxStaffRange + 1);

			Algorithm.FloodFill(ref staffRange, (Cell cell, float amount) => amount - 1);
			//


			// Painting Cells
			// TODO: Add a cell coloring function to Cell
			Color[] colors = meshFilter.sharedMesh.colors;

			foreach (Cell cell in Map.main.CellList)
				if (moveRange.ContainsKey(cell))
				{
					colors[cell.MeshIndex + 0] = new Color(0, 0, 1, 0.5f);
					colors[cell.MeshIndex + 1] = new Color(0, 0, 1, 0.5f);
					colors[cell.MeshIndex + 2] = new Color(0, 0, 1, 0.5f);
					colors[cell.MeshIndex + 3] = new Color(0, 0, 1, 0.5f);
				}
				else if (attackRange.ContainsKey(cell))
				{
					colors[cell.MeshIndex + 0] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 1] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 2] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 3] = new Color(1, 0, 0, 0.5f);
				}
				else if (staffRange.ContainsKey(cell))
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
			//

			Unit[] units = GameObject.FindObjectsOfType<Unit>();
			foreach (Unit unit in units)
			{
				if (unit == selectedUnit)
					continue;

				Cell cell = Map.main[unit.transform.position];
				if (cell != null &&
					moveRange.ContainsKey(cell))
					moveRange.Remove(cell);
			};
		}

		public override void OnUpdate()
		{
			RaycastHit[] hits =
				Physics.RaycastAll(
					Camera.main.ScreenPointToRay(Input.mousePosition));

			if (hits.Length > 0)
			{
				Cell hoveredCell = null;

				// Handling the case where it skips right to attacking stage
				// when clicking on an enemy unit while holding a weapon
				// clicking on a friendly unit while holding a staff
				HashSet<Unit> hoveredUnits = new();
				foreach (RaycastHit hit in hits)
				{
					Unit unit = hit.collider.GetComponent<Unit>();
					if (unit != null)
						hoveredUnits.Add(unit);
					else hoveredCell = Map.main[hit.point];
				};

				// Cutting to Attack / Staff Option
				foreach (Unit hoveredUnit in hoveredUnits)
					if (hoveredUnit != null &&
						hoveredUnit != selectedUnit)
					{
						if (hoveredUnit.Team == Unit.eTeam.Enemy)
						{
							// TODO: Add Attack Tooltip

							// TODO: Expand this

							if (Input.GetMouseButtonDown(0))
							{
								//targets.Clear();
								//targets.Add(hoveredUnit);
								//SetState<St_Mp_JumpToAttack>();

								//return;
							};
						};
					};

				// Handling moving to a cell
				if (hoveredCell != null &&
					moveRange.ContainsKey(hoveredCell))
				{
					// Snap to a position if left clicked
					if (Input.GetMouseButtonDown(0))
					{
						if (selectedUnit.Path.Count > 0)
						{
							selectedUnit.transform.position = selectedUnit.Path.Last();
							selectedUnit.SetPath(
								new List<Vector3>
								{
									selectedUnit.Path.Last()
								});
						};

						SetState(new St_Mp_ChooseAnOption(selectedUnit));

						return;
					};


					// Generating path to cursor RaycastHit
					if (hoveredCell != null &&
						hoveredCell != goal &&
						moveRange.ContainsKey(hoveredCell))
					{
						goal = hoveredCell;

						List<Cell> path = new List<Cell> { Map.main[selectedUnit.transform.position] };

						Algorithm.AStar(
							Map.main[selectedUnit.transform.position],
							hoveredCell,
							ref path,
							(Cell f, Cell t) =>
							{
								if (!moveRange.ContainsKey(f) ||
									(f.Unit != null && f.Unit.Team == Unit.eTeam.Enemy))
									return float.PositiveInfinity;

								return Vector3.Distance(f.Position, t.Position);
							});

						Algorithm.SmoothAStar(ref path);

						if (path.Count > 1)
							path.RemoveAt(0);

						List<Vector3> vPath = new();
						vPath.Add(selectedUnit.transform.position);
						foreach (Cell _cell in path)
							vPath.Add(_cell.Position);

						selectedUnit.SetPath(vPath);
					};
				};
			};

			// TODO: Change this to a generic "back" option
			if (Input.GetKeyDown(KeyCode.Escape) ||
				Input.GetMouseButtonDown((int)MouseButton.Right))
				FallBack<St_Mp_InitialState>();
		}
	};

	public class St_Mp_ChooseAnOption : St_MapState
	{
		Dictionary<Cell, float> attackRange;
		Dictionary<Cell, float> staffRange;

		Unit selectedUnit;

		public St_Mp_ChooseAnOption(Unit selectedUnit)
			=> this.selectedUnit = selectedUnit;

		public override void OnEnter()
		{
			OptionMenu.Clear();
			OptionMenu.EnableOptions(selectedUnit);

			float maxAttackRange = selectedUnit.GetAttackRange();

			attackRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], maxAttackRange }
				};

			Algorithm.FloodFill(ref attackRange, (Cell cell, float amount) => amount - 1);

			attackRange.Remove(Map.main[selectedUnit.transform.position]);

			float maxStaffRange = selectedUnit.GetStaffRange();

			staffRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], maxStaffRange }
				};

			Algorithm.FloodFill( ref staffRange, (Cell cell, float amount) => amount - 1);

			staffRange.Remove(Map.main[selectedUnit.transform.position]);

			Color[] colors = meshFilter.sharedMesh.colors;

			foreach (Cell cell in Map.main.CellList)
				if (attackRange.ContainsKey(cell))
				{
					colors[cell.MeshIndex + 0] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 1] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 2] = new Color(1, 0, 0, 0.5f);
					colors[cell.MeshIndex + 3] = new Color(1, 0, 0, 0.5f);
				}
				else if (staffRange.ContainsKey(cell))
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
			if (Input.GetMouseButtonDown(0) &&
				Physics.Raycast(
					Camera.main.ScreenPointToRay(Input.mousePosition),
					out RaycastHit hit))
			{
				Cell cell = Map.main[hit.point];
				Unit unit = hit.collider.GetComponent<Unit>();
				if (unit != null &&
					cell != null)
				{
					if (unit.Team == Unit.eTeam.Enemy &&
						attackRange.ContainsKey(cell))
					{
						if (selectedUnit.equipped is Weapon weapon)
							SetState(new Op_Attack(
								selectedUnit,
								weapon));
					}

					else
					if ((unit.Team == Unit.eTeam.Player || unit.Team == Unit.eTeam.Ally) &&
						staffRange.ContainsKey(cell))
						if (selectedUnit.equipped is Staff staff)
							SetState(new Op_Staff(selectedUnit, staff));
				};
			};

			// TODO: Change this to a generic "back" option
			if (Input.GetKeyDown(KeyCode.Escape))
				FallBack<St_Mp_InitialState>();

			if (Input.GetMouseButtonDown((int)MouseButton.Right))
				FallBack<St_Mp_ChooseAPosition>();
		}

		public override void OnLeave()
		{
			OptionMenu.Clear();
		}
	};

	public class St_End : St_MapState
	{
		Unit selectedUnit;
		public St_End(Unit selectedUnit)
		 => this.selectedUnit = selectedUnit;
		

		public override void OnUpdate()
		{
			selectedUnit.ActionUsed = true;
			Map.main[selectedUnit.transform.position].Unit = selectedUnit;

			SetState(new St_Mp_InitialState());
		}
	};
};
