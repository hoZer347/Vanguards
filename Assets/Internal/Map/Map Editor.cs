#if UNITY_EDITOR

using System.IO;
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
		bool mapSelectionFoldOut = false;

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

			// Load New Map Dropdown
			if (mapSelectionFoldOut = EditorGUILayout.Foldout(mapSelectionFoldOut, "Load New Map: "))
			{
				string[] files = Directory.GetFiles(
					Application.dataPath + "/Map Prefabs/");

				foreach (string file in files)
				{
					if (file.EndsWith(".meta"))
						continue;

					string shortenedFile =
						file.Replace(
							Application.dataPath + "/Map Prefabs/",
							"").Replace(
							".prefab",
							"");

					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField(shortenedFile);

					if (GUILayout.Button("Load", GUILayout.Width(70)))
					{
						Load(shortenedFile);
						source_file = shortenedFile;
					};

					if (GUILayout.Button("X", GUILayout.Width(25)))
						File.Delete(file);

					//Texture2D display =
					//	AssetPreview.GetAssetPreview(
					//		AssetDatabase.LoadAssetAtPath<GameObject>(
					//			"/Assets/Map Prefabs/" + shortenedFile));

					//if (display != null)
					//{
					//	EditorGUI.DrawPreviewTexture(
					//		GUILayoutUtility.GetRect(64, 64),
					//		display);
					//};

					EditorGUILayout.EndHorizontal();
				};
			};
			//

			EditorGUILayout.EndVertical();
		}

		void Save(string file_name)
		{
			string fullPath = Application.dataPath + "/Map Prefabs/" + file_name + ".prefab";

			PrefabUtility.SaveAsPrefabAsset(
				GetComponentInChildren<Map>().gameObject,
				fullPath);

			Load(file_name);
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

		[CustomEditor(typeof(MapEditor))]
		public class MapEditorUI : Editor
		{
			override public void OnInspectorGUI()
				=> ((MapEditor)target).DoGUI();
		};
	};
};

#endif
