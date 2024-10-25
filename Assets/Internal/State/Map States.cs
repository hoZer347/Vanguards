using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	abstract public class St_MapState : State
	{
		static protected Unit selectedUnit = null;
		static protected Dictionary<Cell, float> range = new();
	};

	public class St_Mp_Initial_State : St_MapState
	{
		public override void OnEnter()
		{

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
					{
						selectedUnit = cell.Unit;
					};


					Unit unit = hit.collider.GetComponent<Unit>();
					if (unit != null)
					{
						selectedUnit = unit;

						SetState<St_Mp_ChooseAPosition>();
					};
				};
			};
		}

		public override void OnLeave()
		{

		}
	};

	public class St_Mp_ChooseAPosition : St_MapState
	{
		public override void OnEnter()
		{
			range.Clear();
			range.Add(Map.main[selectedUnit.transform.position], 5);

			Map.FloodFill(
				ref range,
				selectedUnit.MovementFFKernel);

			Map.cells1 = range;
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
					List<Vector3> l = new() { cell.Position };

					selectedUnit.SetPath(l);
				};
            };
		}

		public override void OnLeave()
		{

		}
	};
};
