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
	/// <summary>
	/// Represents a method that modifies a value of the specified type.
	/// </summary>
	/// <typeparam name="ValueType">The type of the value to be modified.</typeparam>
	/// <param name="value">A reference to the value to be modified.</param>
	public delegate void Modifier<ValueType>(ref ValueType value);

	/// <summary>
	/// Holds a base value and a set of modifiers that can modify the base value.
	/// <typeparamref name="ValueType"/> is the type of the attribute's value, which can be int, float, string, or an enum type.
	/// </summary>
	[Serializable]
	public class Attribute<ValueType>
#if UNITY_EDITOR
		: AttributeGUI
#endif
	{
		string name;

		/// <summary>
		/// Represents the base value of the attribute.
		/// </summary>
		public ValueType Base => @base;

		/// <summary>
		/// Gets the computed value after applying all active modifiers.
		/// </summary>
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

		/// <summary>
		/// Sets the base value of the attribute.
		/// </summary>
		/// <param name="newBase">The new base</param>
		public void SetBase(ValueType newBase) => @base = newBase;

		/// <summary>
		/// Sets a modifier for this attribute.
		/// On returning the "Value" property, the base value will be modified by all modifiers.
		/// </summary>
		/// <param name="identifier">String ID for referencing the modifier later</param>
		/// <param name="modifier">Lambda that changes the modifier in some way</param>
		public void SetModifier(string identifier, Modifier<ValueType> modifier)
			=> modifiers[identifier] = modifier;

		/// <summary>
		/// Sets a modifier for this attribute. Returns a unique ID for the modifier.
		/// On returning the "Value" property, the base value will be modified by all modifiers.
		/// </summary>
		/// <param name="modifier">Lambda that changes the modifier in some way</param>
		/// <returns>A unique identifier for referencing the modifier later</returns>
		public string SetModifier(Modifier<ValueType> modifier)
		{
			string name = Guid.NewGuid().ToString("N");
			SetModifier(name, modifier);
			return name;
		}

		/// <summary>
		/// Removes a modifier by its name.
		/// </summary>
		/// <param name="identifier">String ID of the modifier</param>
		public void RmvModifier(string identifier) => modifiers.Remove(identifier);

		#region Private Functionality

		Dictionary<string, Modifier<ValueType>> modifiers = new();

#if UNITY_EDITOR
		protected bool showModifiers = false;
#endif
		[SerializeField]
		ValueType @base;

#if UNITY_EDITOR
		override public void DoGUI(string label)
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
		#endregion
	}

#if UNITY_EDITOR

	[Serializable]
	public abstract class AttributeGUI
	{
		virtual public void DoGUI(string label) { }

		static public object CurrentTarget { get; private set; }

		static public bool loadedAttributeHolderFoldoutToggle = false;

		static public void DoAttributesGUI<_AttributeHolderType>(_AttributeHolderType obj)
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

		static private string ConvertToReadableFormat(string name)
		{
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			var readableName = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(readableName);
		}

		/// <summary>
		/// Saves the attribute holder to a JSON file at the specified path.
		/// Path: Assets/{ AttributeHolderType } Presets/{ AttributeHolderType }.json
		/// </summary>
		static void Save<_AttributeHolderType>(_AttributeHolderType attributeHolder, string path)
		{
			string json = JsonUtility.ToJson(attributeHolder, true);

			File.WriteAllText(path, json);
		}

		/// <summary>
		/// Loads the attribute holder from a JSON file at the specified path.
		/// </summary>
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
