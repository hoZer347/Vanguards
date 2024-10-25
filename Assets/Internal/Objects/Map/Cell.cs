using System;
using UnityEngine;


namespace Vanguards
{
	[Serializable]
	public class Cell
	{
		[SerializeField]
		string name;

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
		public eTerrainType TerrainType => terrainType;

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

		[SerializeField]
		float difficulty = 1;
		public float Difficulty => difficulty;

		virtual public void SetDifficulty(float difficulty)
		{
			this.difficulty = difficulty;
		}

		Unit unit = null;
		public Unit @Unit => unit;

		private Vector2Int coords;
		private Vector3 position;

		public Vector2Int Coords => coords;
		public Vector3 Position => position;


		public Cell U = null, D = null, L = null, R = null;
	};
};
