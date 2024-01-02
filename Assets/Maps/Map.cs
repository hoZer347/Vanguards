using EasyButtons;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;


namespace Vanguards
{
	[Serializable]
	internal class Map : MonoBehaviour
	{
		static internal Map main = null;

		[SerializeField] internal Color UnhighlightedColor, PlayerMoveRangeColor, EnemyMoveRangeColor, StaffRangeColor, AttackRangeColor;

		[SerializeField] internal List<Cell> cellsToLoad;
		[SerializeField] int _width, _height;

		[SerializeField] internal Color GridColor;

#if (!UNITY_EDITOR)
		private void Update() => MapState.Update();
#else
		[SerializeField]
		string currentStateName;

		private void Update()
		{
			currentStateName = State<MapState>.Current.ToString().Split('.').Last();
			MapState.Update();
		}
#endif
		
		[Button]
		void Generate()
		{
			main = this;


			// Resetting everything
			cells = new Cell[width * height];


			// Loading Custom Cells
			foreach (Cell cell in cellsToLoad)
				this[cell.Coords] = cell;
			//


			// Filling in Empty Cells
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					this[i, j] = new Cell(new Vector2Int(i, j));
			//


			// Attaching Cells Together
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
				{
					this[i, j].U = this[i, j + 1];
					this[i, j].D = this[i, j - 1];
					this[i, j].L = this[i - 1, j];
					this[i, j].R = this[i + 1, j];
				};
			//

			if (GridManager.main == null)
				GridManager.main = GetComponentInChildren<GridManager>();

			if (CellHighlighter.main == null)
				CellHighlighter.main = GetComponentInChildren<CellHighlighter>();

			GridManager.Refresh();
			CellHighlighter.Refresh();
		}

		static void FloodFill(ref Dictionary<Cell, float> cells)
		{
			Dictionary<Cell, float> nextCells = new(cells);

			foreach (var originCell in nextCells)
			{
				Cell[] _cells = new Cell[4] { originCell.Key.U, originCell.Key.D, originCell.Key.L, originCell.Key.R };
				float range = originCell.Value;

				if (range <= 0) continue;

				foreach (var cellCandidate in _cells)
					if (cellCandidate != null)
					{
						if (!cells.ContainsKey(cellCandidate))
							cells.Add(cellCandidate, range - originCell.Key.difficulty);

						else if (cells[cellCandidate] >= range - originCell.Key.difficulty)
							cells[cellCandidate] = range - originCell.Key.difficulty;

						else continue;
					};
			};

			if (nextCells.Count == cells.Count) return;

			FloodFill(ref cells);
		}

		static internal void HighighUnitRange(Unit unit, bool suppressMovement = false)
		{
			// TODO: Account for minimum ranges

			UnHighlightAll();

			byte moveRange		= unit.GetValue("MOV");
			byte attackRange	= unit.GetAttackRange(); // TODO: Allow this to have minimum range
			byte staffRange		= unit.GetStaffRange();  // TODO: Allow this to have minimum range

			Cell uCell = unit.cell;
			if (uCell == null) return;

			if (!suppressMovement)
			{
				attackRange += moveRange;
				staffRange += moveRange;
			}
			else moveRange = 0;

			Dictionary<Cell, float> cells = new(){ { uCell, moveRange } };
			Task t0 = Task.Run(() => FloodFill(ref cells));

			Dictionary<Cell, float> attackCells = new() { { uCell, attackRange } };
			Task t1 = Task.Run(() => FloodFill(ref attackCells));

			Dictionary<Cell, float> staffCells = new() { { uCell, staffRange } };
			Task t2 = Task.Run(() => FloodFill(ref staffCells));

			t0.Wait();
			foreach (Cell cell in cells.Keys)
				cell.Reachable = true;

			t1.Wait();
			foreach (Cell cell in attackCells.Keys)
				cell.Attackable = true;

			t2.Wait();
			foreach (Cell cell in staffCells.Keys)
				cell.Staffable = true;

			CellHighlighter.Refresh();
		}

		static internal void UnHighlightAll() => Cell.UnHighlightAll();

		internal int width => _width;
		internal int height => _height;

		#region operator[] Access

		Cell nullCell = null;
		public ref Cell this[int x, int y]
		{
			get
			{
				if (x < 0 || y < 0 || x >= width || y >= height)
					return ref nullCell;

				return ref cells[x + y * width];
			}
		}
		public ref Cell this[Vector2Int coords]
		{
			get
			{
				if (coords.x < 0 || coords.y < 0 || coords.x >= width || coords.y >= height)
					return ref nullCell;

				return ref cells[coords.x + coords.y * width];
			}
		}

		#endregion

		internal Cell[] cells;

		private void Start()
		{
			main = this;
			MapState.Current = new SelectAUnit();
			Generate();
		}
		private void Awake() { main = this; }
		private void OnValidate()
		{
			main = this;

			foreach (Unit unit in UnitManager.units)
				if (unit != null)
					unit.Coords = unit.spawnCoords;
		}
	};
};
