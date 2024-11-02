using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Vanguards
{
	abstract public class St_MapState : State
	{
		static protected Unit selectedUnit = null;
		static protected Cell originalCell = null;
		static protected Dictionary<Cell, float> range = new();
	};

	public class St_Mp_InitialState : St_MapState
	{
		public override void OnEnter()
		{
			if (selectedUnit != null)
			{
				selectedUnit.SetPath(new List<Cell>());
				selectedUnit.transform.position = originalCell.Position;

				selectedUnit = null;
			};
			
			originalCell = null;

			OptionMenu.Clear();
			range.Clear();
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
		Cell goal;
		Dictionary<Cell, float> attackRange = new();

		public override void OnEnter()
		{
			goal = null;

			range.Clear();
			range.Add(
				Map.main[selectedUnit.transform.position],
				selectedUnit.TotalRange);

			Algorithm.FloodFill(
				ref range,
				selectedUnit.FF_Kernel);

		}

		public override void OnUpdate()
		{
			if (Physics.Raycast(
				Camera.main.ScreenPointToRay(Input.mousePosition),
				out RaycastHit hit))
			{
				Cell cell = Map.main[hit.point];

				if (cell != null &&
					range.ContainsKey(cell))
				{
					if (Input.GetMouseButtonDown(0))
					{
						selectedUnit.transform.position = cell.Position;
						selectedUnit.SetPath(new List<Cell>());

						SetState<St_Mp_ChooseAnOption>();
					}


					else if (cell != goal)
					{
						goal = cell;

						// Add snapping here

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
								if (!range.ContainsKey(f))
									return float.PositiveInfinity;

								return Vector3.Distance(f.Position, t.Position);
							});

						Algorithm.SmoothAStar(ref path);

						path.RemoveAt(0);
						selectedUnit.SetPath(path);
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
