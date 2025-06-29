using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	[Serializable]
	public class Map : MonoBehaviour
	{
		static public Map main;

		[SerializeField]
		public Vector2Int dimensions;

		[SerializeField]
		int height = 1;
		public static int Height => main.height;

		List<Cell> cellList;
		public List<Cell> CellList => cellList;

		Dictionary<Vector2Int, Cell> cellDict;
		public Dictionary<Vector2Int, Cell> CellDict => cellDict;

		Cell[,] cells = new Cell[0, 0];
		public Cell[,] Cells => cells;

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

			if (StateMachine.Current == null)
			//State.SetState(new St_Dialogue("", new St_Mp_InitialState()));
			//State.SetState(new St_Mp_InitialState());
				ScreenFader.FadeFromBlack(new St_Mp_InitialState());

			Obstacle[] obstacles = GetComponentsInChildren<Obstacle>();

			foreach (Obstacle obstacle in obstacles)
				obstacle.GetComponent<Collider>().enabled = false;
		}
		private void OnValidate() => Refresh();

		private void Refresh()
			=> main = this;

		#endregion

#if UNITY_EDITOR

		public void Build()
		{
			// Generating Rays
			Vector3 offset = new(0.5f, 0, 0.5f);

			int w = dimensions.x;
			int h = dimensions.y;

			Ray[] rays = new Ray[w * h];
			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
					rays[i * h + j] =
						new Ray(
							new Vector3(j, Map.Height, i) + offset,
							Vector3.down);
			//


			// Generating Cells
			int layerSurface = LayerMask.NameToLayer("Surface");
			int layerObstacle = LayerMask.NameToLayer("Obstacle");
			int combinedMask = (1 << layerSurface) | (1 << layerObstacle);

			cellList = new();
			int vIndex = 0;
			Vector2[] uvs = new Vector2[w * h * 4];
			int[] indices = new int[w * h * 4];
			Vector3[] positions = new Vector3[w * h * 4];
			Color[] colors = new Color[w * h * 4];
			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
					if (Physics.Raycast(
						rays[i + j * w],
						out RaycastHit hit,
						Height,
						combinedMask) &&
						hit.collider.gameObject.layer != layerObstacle)
					{
						cellList.Add(new Cell(new Vector2Int(i, j), hit.point.y));
						cellList.Last().MeshIndex = vIndex;

						// Don't Rebuild the mesh on Start
						if (!Application.isPlaying)
						{
							positions[vIndex + 3] = new Vector3(i, hit.point.y + 0.001f, j);
							positions[vIndex + 2] = new Vector3(i + 1, hit.point.y + 0.001f, j);
							positions[vIndex + 1] = new Vector3(i + 1, hit.point.y + 0.001f, j + 1);
							positions[vIndex + 0] = new Vector3(i, hit.point.y + 0.001f, j + 1);

							colors[vIndex + 0] = new Color(0, 0, 0, 0.5f);
							colors[vIndex + 1] = new Color(0, 0, 0, 0.5f);
							colors[vIndex + 2] = new Color(0, 0, 0, 0.5f);
							colors[vIndex + 3] = new Color(0, 0, 0, 0.5f);

							uvs[vIndex + 0] = new Vector2(0, 0);
							uvs[vIndex + 2] = new Vector2(1, 0);
							uvs[vIndex + 1] = new Vector2(1, 1);
							uvs[vIndex + 3] = new Vector2(0, 1);

							indices[vIndex + 0] = vIndex + 0;
							indices[vIndex + 1] = vIndex + 1;
							indices[vIndex + 2] = vIndex + 2;
							indices[vIndex + 3] = vIndex + 3;
						};

						vIndex += 4;
					};
			//


			// Linking Cells
			cells = new Cell[w, h];
			cellDict = new();
			foreach (Cell cell in cellList)
			{
				cells[cell.Coords.x, cell.Coords.y] = cell;
				cellDict[cell.Coords] = cell;
			};

			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
				{
					Cell cell = cells[i, j];
					if (cell == null) continue;
					if (i > 0)
						cell.L = cells[i - 1, j];
					if (i < w - 1)
						cell.R = cells[i + 1, j];
					if (j > 0)
						cell.U = cells[i, j - 1];
					if (j < h - 1)
						cell.D = cells[i, j + 1];
				};
			//


			// Generating Mesh
			if (!Application.isPlaying)
			{
				MeshFilter meshFilter = GetComponent<MeshFilter>();
				if (meshFilter.sharedMesh != null)
					meshFilter.sharedMesh.Clear();
				meshFilter.sharedMesh = new Mesh
				{
					name = $"{gameObject.name}",
					vertices = positions,
					uv = uvs,
					colors = colors,
				};

#if UNITY_EDITOR
				EditorApplication.delayCall +=
				() =>
				{
#endif
					meshFilter.sharedMesh.SetIndices(indices, MeshTopology.Quads, 0);
					meshFilter.sharedMesh.RecalculateNormals();
					meshFilter.sharedMesh.RecalculateBounds();
#if UNITY_EDITOR
				};
#endif
			};
			//


			// Saving Mesh
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				string directory = "Assets/Map Meshes/";

				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				if (File.Exists($"{directory}{gameObject.name}.asset"))
					AssetDatabase.DeleteAsset($"{directory}{gameObject.name}.asset");

				AssetDatabase.CreateAsset(
					GetComponent<MeshFilter>().sharedMesh,
					$"{directory}{gameObject.name}.asset");
			};
#endif
			//
		}

		#region GUI

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

			foreach (Cell cell in cellList)
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

		[CustomEditor(typeof(Map))]
		public class MapEditor : Editor
		{
			override public void OnInspectorGUI()
			{
				Map map = (Map)target;
				EditorGUI.BeginDisabledGroup(Application.isPlaying);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Build"))
					map.Build();
				EditorGUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup();
				base.OnInspectorGUI();
			}
		};

		#endregion

#endif
	};
};
