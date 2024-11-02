#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Vanguards
{
	public class MapEditor : MonoBehaviour
	{
		Map map;
		FogOfWar fogOfWar;

		[SerializeField]
		string source_file = "Unititled";

		bool mapFoldOut = false;
		bool fogOfWarFoldOut = false;
		//bool mapSelectionFoldOut = false;

		public void DoGUI()
		{
			EditorGUILayout.BeginVertical();

			// File Management
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Source File: ");
			source_file = EditorGUILayout.TextField(source_file);
			EditorGUILayout.Space();
			EditorGUI.indentLevel++;
			if (GUILayout.Button("Save"))
				Save(source_file);
			if (GUILayout.Button("Load"))
				Load(source_file);
			EditorGUI.indentLevel--;
			EditorGUILayout.EndHorizontal();
			//

			// Map GUI
			map = GetComponentInChildren<Map>();
			if (map != null &&
				(mapFoldOut = EditorGUILayout.Foldout(mapFoldOut, "Map: ")))
			{
				EditorGUI.indentLevel++;
				map.DoGUI();
				EditorGUI.indentLevel--;
			};
			//

			// Fog Of War
			fogOfWar = GetComponentInChildren<FogOfWar>();
			if (fogOfWar != null &&
				(fogOfWarFoldOut = EditorGUILayout.Foldout(mapFoldOut, "Fog Of War: ")))
			{
				EditorGUI.indentLevel++;
				fogOfWar.DoGUI();
				EditorGUI.indentLevel--;
			};
			//

			// Load New Map
			// TODO: Load all prefabs from Assets/Map Prefabs
			//

			EditorGUILayout.EndVertical();
		}

		void Save(string file_name)
		{
			string fullPath = Application.dataPath + "/Map Prefabs/" + file_name + ".prefab";

			PrefabUtility.SaveAsPrefabAsset(
				GetComponentInChildren<Map>().gameObject,
				fullPath);
		}

		void Load(string file_name)
		{
			string fullPath = Application.dataPath + "/Map Prefabs/" + file_name + ".prefab";
			string relPath = "Assets/Map Prefabs/" + file_name + ".prefab";

			int childCount = transform.childCount;
			for (int i = childCount - 1; i >= 0; i--)
				DestroyImmediate(transform.GetChild(i).gameObject);

			PrefabUtility.InstantiatePrefab(
				AssetDatabase.LoadAssetAtPath<GameObject>(relPath),
				transform);
		}
	};

	[CustomEditor(typeof(MapEditor))]
	public class MapEditorUI : Editor
	{
		public override void OnInspectorGUI()
		{
			((MapEditor)target).DoGUI();
		}
	};
};

#endif
