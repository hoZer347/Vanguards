using System;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	[Serializable]
	public sealed class Cell
	{
		[SerializeField]
		string name;

		[SerializeField]
		Color color;
		public Color Color
		{
			get => color;
			set => color = value;
		}

		#region Terrain Type

		public enum eTerrainType
		{
			Grass,
			Forest,
			Mountain,
			Water,
		};

		[SerializeField]
		eTerrainType terrainType;
		public eTerrainType TerrainType
		{
			get => terrainType;
			set => terrainType = value;
		}

        #endregion

        #region Highlighting

  //      public enum eHighlight
  //      {
  //          None,			// Unhighlighted
		//	Traverseable,   // Passable, but you can't stop on it
		//	MoveRange,      // You can move to and over this cell
		//	AttackRange,    // You can attack, but not move to this cell
		//	StaffRange,     // You can use a staff, but not move to this cell
		//};

		//[SerializeField]
		//public eHighlight highlight;
		//public eHighlight Highlight
		//{
		//	get => highlight;
		//	set => highlight = value;
		//}

		#endregion

		public Cell(Vector2Int coords, float height)
		{
			this.coords = coords;
			position =
				new Vector3(
					coords.x + 0.5f,
					height,
					coords.y + 0.5f);

			name = $"Cell at: {coords.x}, {coords.y}";
		}

		public Cell(int x, int y, float height)
		{
			coords =
				new Vector2Int(
					x,
					y);

			position =
				new Vector3(
					x + 0.5f,
					height,
					y + 0.5f);

			name = $"Cell at: {coords.x}, {coords.y}";
		}

		int meshIndex = 0;
		public int MeshIndex
		{
			get => meshIndex;
			set => meshIndex = value;
		}
		
		[SerializeField]
		float difficulty = 1;
		public float Difficulty
		{
			get => difficulty;
			set => difficulty = value;
		}

		Unit unit = null;
		public Unit @Unit
		{
			get => unit;
			set => unit = value;
		}

		static public int GridDistance(Cell c0, Cell c1)
		{
			Vector2Int d = c0.coords - c1.coords;

			return Mathf.Abs(d.x) + Mathf.Abs(d.y);
		}

		private Vector2Int coords;
		private Vector3 position;

		public Vector2Int Coords => coords;
		public Vector3 Position => position;

		public Cell U = null, D = null, L = null, R = null;
	};
};
