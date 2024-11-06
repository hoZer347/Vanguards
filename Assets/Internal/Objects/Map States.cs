using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Vanguards.Unit;


namespace Vanguards
{
	abstract public class St_MapState : State
	{
		static protected MeshFilter meshFilter = Map.main.GetComponent<MeshFilter>();
		static protected Unit selectedUnit = null;
		static protected Cell originalCell = null;
	};

	public class St_Mp_InitialState : St_MapState
	{
		public override void OnEnter()
		{
			if (selectedUnit != null)
			{
				selectedUnit.SetPath(new List<Vector3>());
				selectedUnit.transform.position = originalCell.Position;

				selectedUnit = null;
			};
			
			originalCell = null;

			OptionMenu.Clear();

			MeshFilter meshFilter = Map.main.GetComponent<MeshFilter>();

			if (meshFilter.sharedMesh != null)
			{
				Color[] colors = meshFilter.sharedMesh.colors;

				for (int i = 0; i < colors.Length; i++)
					colors[i] = new Color(0, 0, 0, 0);

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
						!unit.ActionUsed)
					{
						selectedUnit = unit;
						originalCell = Map.main[unit.transform.position];

						SetState<St_Mp_ChooseAPosition>();
					};
				};
			};

			if (Input.GetKeyDown(KeyCode.Escape))
				SetState<St_Mp_InitialState>();
		}

		public override void OnLeave()
		{

		}
	};

	public class St_Mp_ChooseAPosition : St_MapState
	{
		static protected Dictionary<Cell, float> range = new();
		static protected Dictionary<Cell, float> traverseable;    // Cells that can be pathed through
		static protected Dictionary<Cell, float> moveRange;      // Cells the unit can land on

		static protected Cell goal;

		public override void OnEnter()
		{
			// Calculating Move Range
			range.Clear();
			range.Add(
				Map.main[selectedUnit.transform.position],
				selectedUnit.TotalRange);

			Algorithm.FloodFill(
				ref range,
				(Cell cell, float amount, out bool shouldAdd) =>
				{
					if (cell.Unit != null)
						if (cell.Unit.Team == eTeam.Enemy)
						{
							shouldAdd = false;
							return 0.0f;
						};

					shouldAdd = true;
					return amount - cell.Difficulty;
				});

			Color[] colors = meshFilter.sharedMesh.colors;

			foreach (var element in range)
			{
				Cell cell = element.Key;
				float amount = element.Value;

				if (cell != null)
				{
					if (amount > selectedUnit.MoveRange)
						cell.Color = new Color(0, 0, 1, 0.3f);
					else if (amount > selectedUnit.AttackRange)
						cell.Color = new Color(1, 0, 0, 0.3f);
					else if (amount > selectedUnit.StaffRange)
						cell.Color = new Color(0, 1, 0, 0.3f);
					else cell.Color = new Color(0, 0, 0, 0.5f);

					colors[cell.MeshIndex + 0] = cell.Color;
					colors[cell.MeshIndex + 1] = cell.Color;
					colors[cell.MeshIndex + 2] = cell.Color;
					colors[cell.MeshIndex + 3] = cell.Color;
				};
			};

			meshFilter.sharedMesh.colors = colors;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();

			moveRange = range
				.Where(pair => pair.Value > selectedUnit.TotalRange - selectedUnit.MoveRange)
				.ToDictionary(pair => pair.Key, pair => pair.Value);

			traverseable = new(moveRange);

			foreach (Unit unit in Component.FindObjectsOfType<Unit>())
			{
				if (unit == selectedUnit)
					continue;

				moveRange.Remove(Map.main[unit.transform.position]);
			};
			//
		}

		public override void OnUpdate()
		{
			RaycastHit[] hits =
				Physics.RaycastAll(
					Camera.main.ScreenPointToRay(Input.mousePosition));

			if (hits.Length > 0)
			{
				RaycastHit hit = hits[0];

				Cell cell = Map.main[hit.point];

				// Handling the case where it skips right to attacking stage
				// when clicking on an enemy unit while holding a weapon
				// clicking on a friendly unit while holding a staff
				Unit hoveredUnit;
				int i = 0;
				do
				{
					hoveredUnit = hit.collider.GetComponent<Unit>();
					
					if (hoveredUnit != null)
					{
						if (Input.GetMouseButtonDown(0))
						{
							if (hoveredUnit.Team == Unit.eTeam.Enemy &&
								selectedUnit.hasAttack)
							{
								// TODO: Implement this

								SetState<Op_Attack>();

								return;
							};

							if (hoveredUnit.Team == selectedUnit.Team &&
								selectedUnit.hasStaff)
							{
								// TODO: Implement staff usage

								return;
							};
						};

						hit = hits[++i];
					};
				}
				while (hoveredUnit != null);

				// Handling moving to a cell
				if (cell != null &&
					traverseable.ContainsKey(cell))
				{
					// Snap to a position if left clicked
					if (Input.GetMouseButtonDown(0))
					{
						if (selectedUnit.Path.Count > 0)
						{
							selectedUnit.transform.position = selectedUnit.Path.Last();
							selectedUnit.SetPath(new List<Vector3> { selectedUnit.Path.Last() });
						};

						SetState<St_Mp_ChooseAnOption>();

						return;
					};


					// Generating path to cursor RaycastHit
					if (
						(hoveredUnit == null || hoveredUnit == selectedUnit) &&
						cell != null &&
						cell != goal &&
						moveRange.ContainsKey(cell))
					{
						goal = cell;

						List<Cell> path = new List<Cell>
						{
							Map.main[selectedUnit.transform.position]
						};

						Algorithm.AStar(
							Map.main[selectedUnit.transform.position],
							cell,
							ref path,
							(Cell f, Cell t) =>
							{
								if (!traverseable.ContainsKey(f) ||
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

			if (Input.GetKeyDown(KeyCode.Escape))
				SetState<St_Mp_InitialState>();
		}

		public override void OnLeave()
		{
			Color[] colors = new Color[meshFilter.sharedMesh.colors.Length];
			meshFilter.sharedMesh.colors = colors;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();
		}
	};

	public class St_Mp_ChooseAnOption : St_MapState
	{
		public override void OnEnter()
		{
			OptionMenu.GenerateOptions(selectedUnit);
		}

		public override void OnUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				SetState<St_Mp_InitialState>();
		}

		public override void OnLeave()
		{ }
	};
};
