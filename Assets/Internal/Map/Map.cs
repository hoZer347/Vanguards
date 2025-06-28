using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class Map : MonoBehaviour
	{
		static public Map main;

		[SerializeField]
		public Vector2Int dimensions;

		[SerializeField]
		int height = 1;
		public static int Height => main.height;

		[SerializeField]
		List<Cell> cellList;
		public List<Cell> CellList => cellList;

		[SerializeField]
		Dictionary<Vector2Int, Cell> cellDict;
		public Dictionary<Vector2Int, Cell> CellDict => cellDict;

		[SerializeField]
		Cell[,] cells = new Cell[0, 0];
		public Cell[,] Cells => cells;

		MeshFilter meshFilter = null;
		//MeshRenderer meshRenderer = null;
		
		public Cell this[Vector2Int position]
		{
			get
			{
				if (position.x < 0 ||
					position.y < 0 ||
					position.x > cells.GetLength(0) ||
					position.y > cells.GetLength(1))
					return null;

				return cells[
					Mathf.FloorToInt(position.x),
					Mathf.FloorToInt(position.y)];
			}
		}
		public Cell this[int i, int j]
		{
			get
			{
				if (i < 0 ||
					j < 0 ||
					i > cells.GetLength(0) ||
					j > cells.GetLength(1))
					return null;

				return cells[
					Mathf.FloorToInt(i),
					Mathf.FloorToInt(j)];
			}
		}

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

		#region Refresh

		private void Start()
		{
			Refresh();
			Build();

			//State.SetState(new St_Dialogue("", new St_Mp_InitialState()));
			State.SetState(new St_Mp_InitialState());
			//ScreenFader.FadeFromBlack(new St_Mp_InitialState());

			Obstacle[] obstacles = GetComponentsInChildren<Obstacle>();

			foreach (Obstacle obstacle in obstacles)
				obstacle.GetComponent<Collider>().enabled = false;
		}
		private void OnValidate() => Refresh();

		private void Refresh()
		{
			main = this;
		}

		#endregion

		public void Build()
		{
			Cell[,] oldCells = cells;
			cells = new Cell[dimensions.x, dimensions.y];

			cellList.Clear();

			// Copying Old Cells
			for (int x = 0; x < dimensions.x; x++)
				for (int y = 0; y < dimensions.y; y++)
				{
					Cell cell;

					if (x < oldCells.GetLength(0) &&
						y < oldCells.GetLength(1))
						cell = cells[x, y] = oldCells[x, y];

					else if (Physics.Raycast(
						new Ray(
							new Vector3(
								x + 0.5f,
								height,
								y + 0.5f),
							Vector3.down),
						out RaycastHit hit,
						height,
						~LayerMask.GetMask("Unit")) &&
						hit.collider.GetComponent<Surface>() != null)
						cell = cells[x, y] = new Cell(
							new Vector2Int(x, y),
							hit.point.y);

					else continue;

					cellList.Add(cell);
				};
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


			// Building Mesh
			List<Vector3> vertices = new();
			List<Color> colors = new();
			List<int> indices = new();
			List<Vector2> uv = new();
			HashSet<Cell> cellHash = cellList.ToHashSet();

			foreach (Cell cell in cellList)
				if (cell != null)
				{
					Vector3 origin = cell.Position;
					origin.y = height;

					Ray[] rays = new Ray[]
					{
						new(origin + new Vector3(-0.5f, 0, -0.5f), Vector3.down),
						new(origin + new Vector3(-0.5f, 0,  0.5f), Vector3.down),
						new(origin + new Vector3( 0.5f, 0,  0.5f), Vector3.down),
						new(origin + new Vector3( 0.5f, 0, -0.5f), Vector3.down),
					};

					List<RaycastHit> hits = new();

					foreach (Ray ray in rays)
						if (Physics.Raycast(
							ray,
							out RaycastHit hit,
							height * 2,
							LayerMask.GetMask("Surface")))

							hits.Add(hit);

					if (hits.Count == 4)
					{
						cell.MeshIndex = vertices.Count;

						foreach (RaycastHit hit in hits)
						{
							indices.Add(vertices.Count);
							vertices.Add(hit.point + Vector3.up * 0.01f);
							colors.Add(new Color(0, 1, 0, 0.3f));
							uv.Add(new Vector2(0, 0));
						};
					}
					else
					{
						cells[cell.Coords.x, cell.Coords.y] = null;
						cellHash.Remove(cell);
					};
					// TODO: Handle 3x hits
					// TODO: Check a slightly smaller area
				};

			cellList = cellHash.ToList();

			cellDict = new();
			foreach (Cell cell in cellList)
				if (cell != null)
					cellDict[cell.Coords] = cell;

			meshFilter = GetComponent<MeshFilter>();
			meshFilter.sharedMesh = new Mesh();
			meshFilter.sharedMesh.vertices = vertices.ToArray();
			meshFilter.sharedMesh.colors = colors.ToArray();
			meshFilter.sharedMesh.uv = uv.ToArray();
			meshFilter.sharedMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);

			meshFilter.sharedMesh.RecalculateBounds();
			//

			Refresh();

			Unit[] units = GetComponentsInChildren<Unit>();
			foreach (Unit unit in units)
				unit.LockToCell();
		}

		#region GUI

#if UNITY_EDITOR
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

			Gizmos.color = Color.green;

			foreach (Cell cell in cells)
				if (cell != null)
				{
					Gizmos.DrawSphere(
						Vector3Int.FloorToInt(transform.position) +
						cell.Position,

						0.1f);

					if (cell.U != null)
						Gizmos.DrawLine(
							Vector3Int.FloorToInt(transform.position) +
							cell.Position,
							Vector3Int.FloorToInt(transform.position) +
							cell.U.Position);

					if (cell.D != null)
						Gizmos.DrawLine(
							Vector3Int.FloorToInt(transform.position) +
							cell.Position,
							Vector3Int.FloorToInt(transform.position) +
							cell.D.Position);

					if (cell.L != null)
						Gizmos.DrawLine(
							Vector3Int.FloorToInt(transform.position) +
							cell.Position,
							Vector3Int.FloorToInt(transform.position) +
							cell.L.Position);

					if (cell.R != null)
						Gizmos.DrawLine(
							Vector3Int.FloorToInt(transform.position) +
							cell.Position,
							Vector3Int.FloorToInt(transform.position) +
							cell.R.Position);
				};
		}

		public void DoGUI()
		{
			EditorGUILayout.BeginVertical();


			// Dimensions
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Dimensions: ");
			var oldDimensions = dimensions;
			dimensions.x = EditorGUILayout.IntField(dimensions.x);
			dimensions.y = EditorGUILayout.IntField(dimensions.y);
			dimensions.x = Mathf.Max(1, dimensions.x);
			dimensions.y = Mathf.Max(1, dimensions.y);
			if (oldDimensions != dimensions)
				EditorUtility.SetDirty(this);
			EditorGUILayout.EndHorizontal();
			//


			// Height
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Height: ");
			var oldHeight = height;
			height = EditorGUILayout.IntField(height);
			height = Mathf.Max(1, height);
			if (oldHeight != height)
				EditorUtility.SetDirty(this);
			EditorGUILayout.EndHorizontal();
			//


			// Show Gizmos
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Show Gizmos: ");
			showGizmos = EditorGUILayout.Toggle(showGizmos);
			EditorGUILayout.EndHorizontal();
			//


			// Rebuild Button
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Build"))
				Build();
			EditorGUILayout.EndHorizontal();
			//


			EditorGUILayout.EndVertical();
		}

		[CustomEditor(typeof(Map))]
		public class MapUI : Editor
		{
			override public void OnInspectorGUI()
				=>((Map)target).DoGUI();
		};
#endif

		#endregion
	};
};
