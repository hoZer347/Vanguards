using System;
using UnityEngine;
using Unity;
using System.Collections.Generic;


namespace Vanguards
{
	enum TType
	{
		Null,
		Grass,
		Chasm,
		Trees,
	};

	// TODO: Change to a single Mesh Renderer

	[Serializable]
	internal class Cell
	{
		internal Cell(Vector2Int coords) => Coords = coords;

		[SerializeField] internal float difficulty = 1.0f;
		[SerializeField] internal TType terrainType;
		[SerializeField] internal Vector2Int Coords;
		internal int x => Coords.x;
		internal int y => Coords.y;

		// TODO: Improve this
		internal Unit unit
		{
			get
			{
				foreach (Unit unit in UnitManager.units)
					if (unit != null && unit.Coords == Coords)
						return unit;

				return null;
			}
		}

		internal Cell U = null, D = null, L = null, R = null;

		#region Highlighting

		static internal void UnHighlightAll()
		{
			if (Map.main.cells == null) return;

			foreach (Cell cell in Map.main.cells)
			{
				cell.reachable = false;
				cell.attackable = false;
				cell.staffable = false;
			};

			CellHighlighter.Refresh();
		}

		bool reachable = false;
		internal bool Reachable
		{
			get => reachable;
			set
			{
				reachable = value;
			}
		}

		bool attackable = false;
		internal bool Attackable
		{
			get => attackable;
			set
			{
				attackable = value;
			}
		}

		bool staffable = false;
		internal bool Staffable
		{
			get => staffable;
			set
			{
				staffable = value;
			}
		}

		#endregion
	};
};
