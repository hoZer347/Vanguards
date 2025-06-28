using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class Saveable : MonoBehaviour
	{
		public void Load(string file)
		{
			string fileName = Path.GetFileNameWithoutExtension(file);

			string data = File.ReadAllText(file);

			JsonUtility.FromJsonOverwrite(data, this);

			gameObject.name = fileName;

			EditorUtility.SetDirty(this);
		}

		public void Save(string file)
		{
			string path = $"{Application.dataPath}/{ GetType().Name.Split('.').Last()} Presets/{ name }.json";
			string directory = Path.GetDirectoryName(path);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			string data = JsonUtility.ToJson(this, true);

			File.WriteAllText(path, data);

			Saveable[] saveables = FindObjectsByType<Saveable>(FindObjectsSortMode.None);

			foreach (Saveable saveable in saveables)
				if (saveable.gameObject.name == name &&
					saveable != this)
					saveable.Load(file);
		}
	};

	[CustomEditor(typeof(Saveable), true)]
	public class SaveableEditor : Editor
	{
		static bool showFiles = true;

		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			string path = $"{ Application.dataPath}/{target.GetType().Name.Split('.').Last() } Presets/{ target.name }.json";
			string directory = Path.GetDirectoryName(path);

			GUILayout.BeginVertical();

			if (GUILayout.Button("Save"))
				((Saveable)target).Save(path);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			if (showFiles = EditorGUILayout.Foldout(showFiles, "Saved Presets"))
				foreach (string file in Directory.GetFiles(directory, "*.json"))
				{
					GUILayout.BeginHorizontal();

					GUILayout.Label(Path.GetFileName(file));

					if (GUILayout.Button("Load", GUILayout.Width(100)))
						((Saveable)target).Load(file);

					if (GUILayout.Button("X", GUILayout.Width(20)))
						File.Delete(file);

					GUILayout.EndHorizontal();
				};

			GUILayout.EndVertical();
		}
	};
};
