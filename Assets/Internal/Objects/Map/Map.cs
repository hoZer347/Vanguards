using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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

		static public Dictionary<Cell, float> cells1 = new();
		static public List<Cell> cells2 = new();
		static public List<Cell> cells3 = new();

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		[EasyButtons.Button]
		private void Refresh()
		{
			main = this;

			State.SetState<St_Mp_InitialState>();

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

			foreach (Cell cell in cells2)
				if (cell != null)
				{
					Gizmos.color = Color.cyan;

					Gizmos.DrawSphere(
						Vector3Int.FloorToInt(transform.position) +
						cell.Position,

						0.10002f);
				};

			foreach (Cell cell in cells3)
				if (cell != null)
				{
					Gizmos.color = Color.red;

					Gizmos.DrawSphere(
						Vector3Int.FloorToInt(transform.position) +
						cell.Position,

						0.10002f);
				};
		}

		[SerializeField]
		LayerMask surfaceLayer;
		public LayerMask SurfaceLayer => surfaceLayer;
	};
};
