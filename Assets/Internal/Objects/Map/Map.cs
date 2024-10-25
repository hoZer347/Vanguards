using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;


namespace Vanguards
{
	public class Map : MonoBehaviour
	{
		static public Map main;

		[SerializeField]
		public Vector2Int dimensions;

		public ref Cell this[Vector2Int position] => ref cells[position.x, position.y];
		public ref Cell this[int i, int j] => ref cells[i, j];
		public Cell this[Vector3 position]
		{
			get
			{
				if (position.x < 0 ||
					position.z < 0 ||
					position.x > cells.GetLength(0) ||
					position.z > cells.GetLength(1))
					return null;

				return cells[
					Mathf.FloorToInt(position.x),
					Mathf.FloorToInt(position.z)];
			}
		}

		[SerializeField]
		int height = 1;
		public static int Height => main.height;

		[SerializeField]
		public List<Cell> cellList = new();

		static Cell[,] cells = new Cell[0, 0];

		public delegate float FloodFillKernel(Cell cell, float amount, out bool shouldAdd);

		public static void FloodFill(
			ref Dictionary<Cell, float> cells,
			FloodFillKernel kernel)
		{
			Dictionary<Cell, float> newCells = new(cells);

			foreach (var element in newCells)
			{
				if (element.Value <= 0) continue;

				var _cells =
					new Cell[]
					{
						element.Key.U,
						element.Key.D,
						element.Key.L,
						element.Key.R,
					};

				var amount = element.Value;

				foreach (Cell _cell in _cells)
					if (_cell != null)
					{
						var _amount = kernel(_cell, amount, out bool shouldAdd);

						if (shouldAdd)
							if (cells.ContainsKey(_cell))
							{
								cells[_cell] = Mathf.Max(
									cells[_cell],
									_amount);
							}
							else cells.Add(_cell, _amount);
					};
			};

			if (newCells.Count == cells.Count) return;

			FloodFill(ref cells, kernel);
		}
		
		static public Dictionary<Cell, float> cells1 = new();

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		[EasyButtons.Button]
		private void Refresh()
		{
			main = this;

			State.SetState<St_Mp_Initial_State>();

			dimensions.x = Mathf.Max(1, dimensions.x);
			dimensions.y = Mathf.Max(1, dimensions.y);

			Cell[,] oldCells = cells;
			cells = new Cell[dimensions.x, dimensions.y];

			cells1.Clear();

			// Copying Old Cells
			for (int x = 0; x < dimensions.x; x++)
				for (int y = 0; y < dimensions.y; y++)
					if (x < oldCells.GetLength(0) &&
						y < oldCells.GetLength(1))
						cells[x, y] = oldCells[x, y];

					else if (Physics.Raycast(
						new Ray(
							new Vector3(
								x + 0.5f,
								height,
								y + 0.5f),
							Vector3.down),
						out RaycastHit hit,
						float.PositiveInfinity,
						surfaceLayer))

						cells[x, y] = new Cell(
							new Vector2Int(x, y),
							hit.point.y);
			//


			// Adding Cells to Gizmos
#if UNITY_EDITOR
			cellList.Clear();
			for (int x = 0; x < dimensions.x; x++)
				for (int y = 0; y < dimensions.y; y++)
					if (Physics.Raycast(
						new Ray(
							new Vector3(
								x + 0.5f,
								height,
								y + 0.5f),
							Vector3.down),
						out RaycastHit hit))

						cellList.Add(cells[x, y]);
#endif
			//


			// Connecting Adjacent Cells
			for (int i = 0; i < dimensions.x; i++)
				for (int j = 0; j < dimensions.y; j++)
					if (cells[i, j] != null)
					{
						if (j < dimensions.y - 1) cells[i, j].U = cells[i, j + 1];
						if (j > 0) cells[i, j].D = cells[i, j - 1];
						if (i > 0) cells[i, j].L = cells[i - 1, j];
						if (i < dimensions.x - 1) cells[i, j].R = cells[i + 1, j];
					};
			//

			Unit[] units = FindObjectsOfType<Unit>();
			foreach (Unit unit in units)
				unit.LockToCell();
		}

		[SerializeField]
		bool showGizmos = true;

		private void OnDrawGizmos()
		{
			if (!showGizmos) return;

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(
				Vector3Int.FloorToInt(transform.position) +
				new Vector3(
					dimensions.x,
					height,
					dimensions.y) / 2,

				new Vector3(
					dimensions.x,
					height,
					dimensions.y));

			foreach (Cell cell in cells)
				if (cell != null)
				{
					Gizmos.color = Color.green;

					Gizmos.DrawSphere(
						Vector3Int.FloorToInt(transform.position) +
						cell.Position,

						0.1f);
				};

			foreach (Cell cell in cells1.Keys)
				if (cell != null)
				{
					Gizmos.color = Color.blue;

					Gizmos.DrawSphere(
						Vector3Int.FloorToInt(transform.position) +
						cell.Position,

						0.10001f);
				};
		}

		[SerializeField]
		LayerMask surfaceLayer;
		public LayerMask SurfaceLayer => surfaceLayer;
	};
};
