using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Vanguards
{
	static public class Algorithm
	{
		// Standard heuristic based Flood Fill
		public delegate float FloodFillHeuristic(Cell cell, float amount);

		public static void FloodFill(
			ref Dictionary<Cell, float> cells,
			FloodFillHeuristic kernel)
		{
			Dictionary<Cell, float> newCells = new(cells);

			foreach (var (cell, value) in newCells)
			{
				Cell[] _cells =
					new Cell[]
					{
						cell.U,
						cell.D,
						cell.L,
						cell.R,
					};

				foreach (Cell _cell in _cells)
					if (_cell != null)
					{
						float amount =
							kernel(
								_cell,
								value);

						if (amount <= 0)
							continue;

						if (cells.ContainsKey(_cell))
							cells[_cell] =
								Mathf.Max(
									cells[_cell],
									amount);

						else cells.Add(_cell, amount);
					};
			};

			if (newCells.Count == cells.Count)
				return;

			FloodFill(ref cells, kernel);
		}
		//


		// A* algorithm with impassable cell handling
		public delegate float AStarHeuristic(Cell from, Cell to);

		public static void AStar(
			Cell origin,
			Cell goal,
			ref List<Cell> path,
			AStarHeuristic heuristic)
		{
			path.Clear();

			// Custom comparer for SortedSet to avoid duplicate keys
			var openSet = new SortedSet<(float, Cell)>(Comparer<(float, Cell)>.Create((a, b) =>
				a.Item1 == b.Item1 ? a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()) : a.Item1.CompareTo(b.Item1)));

			var cameFrom = new Dictionary<Cell, Cell>();
			var gScore = new Dictionary<Cell, float> { [origin] = 0 };
			var fScore = new Dictionary<Cell, float> { [origin] = heuristic(origin, goal) };

			openSet.Add((fScore[origin], origin));

			while (openSet.Count > 0)
			{
				// Get the cell with the lowest F score
				var current = openSet.Min.Item2;
				openSet.Remove(openSet.Min);

				if (current == goal)
				{
					var totalPath = new List<Cell> { current };
					while (cameFrom.ContainsKey(current))
					{
						current = cameFrom[current];
						totalPath.Insert(0, current);
					};
					path.AddRange(totalPath);
					return;
				};

				foreach (var neighbor in new[] { current.U, current.D, current.L, current.R })
				{
					if (neighbor == null)
						continue;

					float tentativeGScore = gScore[current] + 1;

					if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
					{
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentativeGScore;
						float neighborFScore = tentativeGScore + heuristic(neighbor, goal);

						if (!openSet.Any(e => e.Item2 == neighbor))
							openSet.Add((neighborFScore, neighbor));
					};
				};
			};
		}
		//


		// Smooths Diagonal A* paths
		// TODO: Add a direct path check
		public static void SmoothAStar(
			ref List<Cell> path)
		{
			// TODO: Fix this so it doesn't cause a null position when
			//       pathing diagonally between spells

			//HashSet<Cell> hashSet = path.ToHashSet();

			//foreach (Cell cell in path)
			//{
			//	if (cell == path.Last()) break;

			//	int counter = 0;
			//	if (hashSet.Contains(cell.U)) counter++;
			//	if (hashSet.Contains(cell.D)) counter++;
			//	if (hashSet.Contains(cell.L)) counter++;
			//	if (hashSet.Contains(cell.R)) counter++;

			//	if (counter >= 2)
			//		hashSet.Remove(cell);
			//};

			//path = hashSet.ToList();
		}
		//


		// Pathing that assumes you've already floodfilled an area
		public static void PostFloodFillPath(
			Cell origin,
			Cell goal,
			ref List<Cell> path,
			ref Dictionary<Cell, float> floodfilled_cells)
		{
			if (!floodfilled_cells.ContainsKey(origin) ||
				!floodfilled_cells.ContainsKey(goal))
				return;

			path.Clear();
			path.Add(origin);

			Cell currentCell = origin;
			float remainingDistance = floodfilled_cells[currentCell];

			while (currentCell != goal && remainingDistance > 0)
			{
				var neighbors = new[]
				{
					currentCell.U,
					currentCell.D,
					currentCell.L,
					currentCell.R
				};

				Cell nextCell = null;
				float maxDistance = -1f;
				int minGoalDistance = int.MaxValue;

				foreach (var neighbor in neighbors)
				{
					if (neighbor != null &&
						floodfilled_cells.TryGetValue(neighbor, out float neighborDistance) &&
						neighborDistance < remainingDistance &&
						!path.Contains(neighbor))
					{
						int goalDistance =
							Mathf.Abs(neighbor.Coords.x - goal.Coords.x) +
							Mathf.Abs(neighbor.Coords.y - goal.Coords.y);

						// Select the neighbor with the highest remaining movement distance.
						// If tied, choose the one closer to the goal.
						if (neighborDistance > maxDistance ||
							(neighborDistance == maxDistance && goalDistance < minGoalDistance))
						{
							maxDistance = neighborDistance;
							minGoalDistance = goalDistance;
							nextCell = neighbor;
						};
					};
				};

				if (nextCell == null)
					break;

				path.Add(nextCell);
				currentCell = nextCell;
				remainingDistance = floodfilled_cells[currentCell];
			};
		}
		//


		// Getting Ranges Post-Floodfill
		public delegate bool PostFloodFillRangeHeuristic(Cell cell);

		static public void PostFloodFillRange(
			ref Dictionary<Cell, float> cells,
			ref Dictionary<Cell, float> output,
			PostFloodFillRangeHeuristic test,
			int min,
			int max)
		{
			var visited = new HashSet<Cell>();
			var queue = new Queue<(Cell cell, int depth)>();

			// Start from each input cell at depth 0
			foreach (var (cell, _) in cells)
			{
				queue.Enqueue((cell, 0));
				visited.Add(cell);
			};

			while (queue.Count > 0)
			{
				var (cell, depth) = queue.Dequeue();

				// Always traverse the cell, but only store it in output if within range
				if (depth >= min && depth <= max && test(cell))
					output[cell] = depth;

				// Continue traversing until max range is reached
				if (depth < max)
					foreach (var neighbor in new[] { cell.U, cell.D, cell.L, cell.R })
						if (neighbor != null && !visited.Contains(neighbor))
						{
							visited.Add(neighbor);
							queue.Enqueue((neighbor, depth + 1));
						};
			};
		}
		//


		// Copies class A int B given B inherits from A
		static public void CopyTo<A>(A source, A target)
		{
			foreach (FieldInfo field in typeof(A).GetFields())
			{
				if (field.IsStatic)
					continue;

				field.SetValue(target, field.GetValue(source));
			};
		}
		//
	};
};
