using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vanguards
{
	public delegate void Modifier<ValueType>(ref ValueType value);

	[Serializable]
	public class Attribute<ValueType>
#if UNITY_EDITOR
		: AttributeGUI
#endif
	{
		[SerializeField]
		private string name;

		[SerializeField]
		private ValueType @base;

		private Dictionary<string, Modifier<ValueType>> modifiers = new();

#if UNITY_EDITOR
		protected bool showModifiers = false;
#endif

		public string Name => name;
		public ValueType Base => @base;

		public ValueType Value
		{
			get
			{
				ValueType value = @base;
				foreach (var modifier in modifiers.Values)
					modifier(ref value);
				return value;
			}
		}

		public void SetBase(ValueType newBase) => @base = newBase;

		public void SetModifier(string name, Modifier<ValueType> modifier)
			=> modifiers[name] = modifier;

		public void RmvModifier(string name) => modifiers.Remove(name);

#if UNITY_EDITOR
		public override void DoGUI(string label)
		{
			EditorGUIUtility.labelWidth = 50;

			EditorGUILayout.BeginHorizontal();

			GUILayout.Label(label, GUILayout.Width(120));

			// Change check
			EditorGUI.BeginChangeCheck();

			ValueType newBase = @base;

			if (typeof(ValueType) == typeof(int))
				newBase = (ValueType)(object)EditorGUILayout.IntField("Base", (int)(object)@base, GUILayout.Width(150));
			else if (typeof(ValueType) == typeof(float))
				newBase = (ValueType)(object)EditorGUILayout.FloatField("Base", (float)(object)@base, GUILayout.Width(150));
			else if (typeof(ValueType) == typeof(string))
				newBase = (ValueType)(object)EditorGUILayout.TextField("Base", (string)(object)@base, GUILayout.Width(200));
			else if (typeof(ValueType).IsEnum)
				newBase = (ValueType)(object)EditorGUILayout.EnumPopup("Base", (Enum)(object)@base, GUILayout.Width(200));
			else EditorGUILayout.LabelField($"Unsupported: {typeof(ValueType).Name}", GUILayout.Width(200));

			if (EditorGUI.EndChangeCheck())
			{
				@base = newBase;

				if (AttributeGUI.CurrentTarget is UnityEngine.Object uo)
				{
					Undo.RecordObject(uo, "Modify Attribute");
					EditorUtility.SetDirty(uo);
				}
			}

			GUILayout.Label("Value: " + Value?.ToString(), GUILayout.Width(150));

			// Inline foldout at end of line
			Rect foldoutRect = GUILayoutUtility.GetRect(15, EditorGUIUtility.singleLineHeight);
			showModifiers = EditorGUI.Foldout(foldoutRect, showModifiers, GUIContent.none);

			EditorGUILayout.EndHorizontal();

			if (showModifiers)
			{
				EditorGUI.indentLevel++;
				foreach (var modifier in modifiers)
				{
					EditorGUILayout.LabelField($"• {modifier.Key}");
				}
				EditorGUI.indentLevel--;
			}
		}
#endif
	}

#if UNITY_EDITOR

	[Serializable]
	public abstract class AttributeGUI
	{	
		public virtual void DoGUI(string label) { }

		public static object CurrentTarget { get; private set; }

		static public bool loadedAttributeHolderFoldoutToggle = false;

		public static void DoAttributesGUI<_AttributeHolderType>(_AttributeHolderType obj)
			where _AttributeHolderType : MonoBehaviour
		{
			CurrentTarget = obj;

			EditorGUILayout.BeginVertical();

			// Looping through all attached Attributes and making them editable

			Type objType = obj.GetType();

			foreach (var member in objType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object value = null;

				if (member is FieldInfo field && typeof(AttributeGUI).IsAssignableFrom(field.FieldType))
					value = field.GetValue(obj);
				else if (member is PropertyInfo property && typeof(AttributeGUI).IsAssignableFrom(property.PropertyType))
					value = property.GetValue(obj);

				if (value is AttributeGUI attribute)
					attribute.DoGUI(ConvertToReadableFormat(member.Name));
			};

			//


			// Saving, Loading, and Duplicating Attribute Presets

			GUILayout.BeginHorizontal();

			GameObject gameObject = (obj as MonoBehaviour).gameObject;

			string dir = Path.Combine(Application.dataPath, $"{typeof(_AttributeHolderType).Name} Presets");
			string path = Path.Combine(dir, gameObject.name + ".json");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			if (GUILayout.Button("Save"))
			{
				Save(obj, path);

				_AttributeHolderType[] attributeHolders = GameObject.FindObjectsByType<_AttributeHolderType>(FindObjectsSortMode.None);

				foreach (_AttributeHolderType attributeHolder in attributeHolders)
					if (attributeHolder.gameObject.name == gameObject.name &&
						attributeHolder.gameObject != gameObject)
						Load(attributeHolder, path);
			};

			if (GUILayout.Button("Duplicate"))
				GameObject.Instantiate(gameObject, gameObject.transform.parent).name = gameObject.name;

			GUILayout.EndHorizontal();

			if (loadedAttributeHolderFoldoutToggle = EditorGUILayout.Foldout(loadedAttributeHolderFoldoutToggle, "Saved Presets"))
			{
				foreach (string file in Directory.GetFiles(dir))
					if (!file.EndsWith(".meta"))
					{
						GUILayout.BeginHorizontal();

						GUILayout.TextField(Path.GetFileName(file));

						if (GUILayout.Button("Load", GUILayout.Width(100)))
							Load(obj, file);

						if (GUILayout.Button("X", GUILayout.Width(25)))
						{
							File.Delete(file);
							File.Delete(file.Replace(".json", ".meta"));
						};

						GUILayout.EndHorizontal();
					};
			};

			//

			EditorGUILayout.EndVertical();
			CurrentTarget = null;
		}

		private static string ConvertToReadableFormat(string name)
		{
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			var readableName = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(readableName);
		}

		static void Save<_AttributeHolderType>(_AttributeHolderType attributeHolder, string path)
		{
			string json = JsonUtility.ToJson(attributeHolder, true);

			File.WriteAllText(path, json);
		}

		static void Load<_AttributeHolderType>(_AttributeHolderType attributeHolder, string path)
		{
			if (!File.Exists(path))
				return;
			
			string json = File.ReadAllText(path);
			JsonUtility.FromJsonOverwrite(json, attributeHolder);
		}
	};

#endif
};
