using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Vanguards
{
	public class St_Mp_Option : St_MapState
	{
		public override void OnUpdate()
		{
			SetState<Op_Cleanup>();
		}
	};

	public class Op_Wait : St_Mp_Option
	{ };

	public class Op_Attack : St_Mp_Option
	{
		static Dictionary<Cell, float> attackRange;
		static HashSet<Unit> attackableUnits = new();

		public override void OnEnter()
		{
			attackRange =
				new Dictionary<Cell, float>
				{
					{ Map.main[selectedUnit.transform.position], selectedUnit.AttackRange }
				};

			Algorithm.FloodFill(
				ref attackRange,
				(Cell cell, float amount, out bool shouldAdd) =>
				{
					shouldAdd = true;

					return amount -= 1;
				});

			Color[] colors = meshFilter.sharedMesh.colors;
			
			foreach (var element in attackRange)
			{
				Cell cell = element.Key;
				float amount = element.Value;

				if (cell.Unit != null)
					attackableUnits.Add(cell.Unit);

				colors[cell.MeshIndex + 0] = new Color(1, 0, 0, 0.3f);
				colors[cell.MeshIndex + 1] = new Color(1, 0, 0, 0.3f);
				colors[cell.MeshIndex + 2] = new Color(1, 0, 0, 0.3f);
				colors[cell.MeshIndex + 3] = new Color(1, 0, 0, 0.3f);
			};

			meshFilter.sharedMesh.colors = colors;
			meshFilter.sharedMesh.RecalculateBounds();
			meshFilter.sharedMesh.RecalculateNormals();
		}

		public override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
				SetState<Op_Cleanup>();
		}

		public override void OnLeave()
		{

		}
	};

	public class Op_Equip : St_Mp_Option
	{ };

	public class Op_Item : St_Mp_Option
	{ };

	public class Op_Cleanup : St_Mp_Option
	{
		public override void OnUpdate()
		{
			// TODO: Move to Op_Wait

			selectedUnit.ActionUsed = true;
			originalCell.Unit = null;
			Map.main[selectedUnit.transform.position].Unit = selectedUnit;
			originalCell = Map.main[selectedUnit.transform.position];

			SetState<St_Mp_InitialState>();
		}
	};
};
