using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Vanguards
{
	internal class GridManager : MonoBehaviour
	{
		static internal GridManager main;

		[SerializeField] internal MeshFilter meshFilter;

		static internal int width => Map.main.width;
		static internal int height => Map.main.height;

		private void Start()
		{
			main = this;
			meshFilter = GetComponent<MeshFilter>();
		}

		static internal void Refresh()
		{
			// TODO: Align with Terrain

			List<Vector3> vertices = new();
			List<int> indices = new();
			List<Color> colors = new();

			Task t0 = Task.Run(() =>
			{
				for (int i = 0; i < width + 1; i++)
					for (int j = 0; j < height + 1; j++)
					{
						vertices.Add(new Vector3(i, 0.0f, j));
						colors.Add(Map.main.GridColor);
					};
			});

			Task t1 = Task.Run(() =>
			{
				for (int i = 0; i < width; i++)
					for (int j = 0; j < height; j++)
					{
						indices.Add((i + 0) + (j + 0) * (width + 1));
						indices.Add((i + 1) + (j + 0) * (width + 1));
						indices.Add((i + 1) + (j + 0) * (width + 1));
						indices.Add((i + 1) + (j + 1) * (width + 1));
						indices.Add((i + 1) + (j + 1) * (width + 1));
						indices.Add((i + 0) + (j + 1) * (width + 1));
						indices.Add((i + 0) + (j + 1) * (width + 1));
						indices.Add((i + 0) + (j + 0) * (width + 1));
					}
			});

			main.meshFilter.sharedMesh = new Mesh();

			t0.Wait();
			main.meshFilter.sharedMesh.vertices = vertices.ToArray();
			main.meshFilter.sharedMesh.colors = colors.ToArray();

			t1.Wait();
			main.meshFilter.sharedMesh.SetIndices(indices, MeshTopology.Lines, 0);
		}

		static internal void Clear()
		{
			
		}
	};
};
