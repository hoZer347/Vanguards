using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;


namespace Vanguards
{
	public class Saveable : MonoBehaviour
	{ };

	[CustomEditor(typeof(Saveable), true)]
	public class SaveableEditor : Editor
	{
		static bool showFiles = true;

		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			string path = $"{Application.dataPath}/{target.GetType().Name.Split('.').Last()} Presets/{target.name}.json";
			string directory = Path.GetDirectoryName(path);

			GUILayout.BeginVertical();

			if (GUILayout.Button("Save"))
			{
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string data = JsonUtility.ToJson(target, true);

				File.WriteAllText(path, data);
			};

			if (showFiles = EditorGUILayout.Foldout(showFiles, "Saved Presets"))
				foreach (string file in Directory.GetFiles(directory, "*.json"))
				{
					GUILayout.BeginHorizontal();

					string fileName = Path.GetFileNameWithoutExtension(file);

					GUILayout.Label(Path.GetFileName(file));

					if (GUILayout.Button("Load", GUILayout.Width(100)))
					{
						string data = File.ReadAllText(file);

						JsonUtility.FromJsonOverwrite(data, target);

						((MonoBehaviour)target).gameObject.name = fileName;

						EditorUtility.SetDirty(target);
					};

					if (GUILayout.Button("X", GUILayout.Width(20)))
						File.Delete(file);

					GUILayout.EndHorizontal();
				};

			GUILayout.EndVertical();
		}
	};
};
