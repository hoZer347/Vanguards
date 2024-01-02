using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vanguards
{
	internal class CellHighlighter : MonoBehaviour
	{
		static internal CellHighlighter main;

		[SerializeField] internal MeshFilter meshFilter;
		[SerializeField] internal float edgeBuffer = 0.15f;

		static internal int width => Map.main.width;
		static internal int height => Map.main.height;

		private void Start()
		{
			main = GetComponent<CellHighlighter>();
			meshFilter = GetComponent<MeshFilter>();
		}

		static internal void Refresh()
		{
			List<Vector3> vertices = new();
			List<int> indices = new();
			List<Color> colors = new();

			foreach (Cell cell in Map.main.cells)
			{
				Color color = new();

				if (cell.Reachable)
					color = Map.main.PlayerMoveRangeColor;
				else if (cell.Attackable)
					color = Map.main.AttackRangeColor;
				else if (cell.Staffable)
					color = Map.main.StaffRangeColor;
				// TODO: Finish adding other highlight types here
				else continue;

				vertices.Add(new Vector3(cell.x + 0 + main.edgeBuffer, 0.0f, cell.y + 0 + main.edgeBuffer));
				vertices.Add(new Vector3(cell.x + 1 - main.edgeBuffer, 0.0f, cell.y + 0 + main.edgeBuffer));
				vertices.Add(new Vector3(cell.x + 1 - main.edgeBuffer, 0.0f, cell.y + 1 - main.edgeBuffer));
				vertices.Add(new Vector3(cell.x + 0 + main.edgeBuffer, 0.0f, cell.y + 1 - main.edgeBuffer));

				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);

				int index = indices.Count + 3;

				indices.Add(index - 0);
				indices.Add(index - 1);
				indices.Add(index - 2);
				indices.Add(index - 3);
			};

			main.meshFilter.sharedMesh = new Mesh();
			main.meshFilter.sharedMesh.vertices = vertices.ToArray();
			main.meshFilter.sharedMesh.colors = colors.ToArray();
			main.meshFilter.sharedMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
			main.meshFilter.sharedMesh.RecalculateNormals();
		}
	};
};
