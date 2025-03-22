using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;


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
		public void SetBase(ValueType newBase) => @base = newBase;

		public void SetModifier(
			string name,
			Modifier<ValueType> modifier)
			=> modifiers[name] = modifier;

		public void RmvModifier(string name)
		{
			modifiers.Remove(name);
		}

		[SerializeField]
		string name;
		public string Name => name;

		public ValueType Base => @base;
		public ValueType Value
		{
			get
			{
				ValueType value = @Base;

				foreach (var modifier in modifiers.Values)
					modifier(ref value);

				return value;
			}
		}
		
		[SerializeField]
		ValueType @base;

		Dictionary<string, Modifier<ValueType>> modifiers = new();

#if UNITY_EDITOR

		#region Editor GUI

		protected bool showModifiers = false;

		public override void DoGUI()
		{
			EditorGUIUtility.labelWidth = 50;

			// Display the name label
			EditorGUILayout.LabelField(name, GUILayout.Width(100));

			if (typeof(ValueType) == typeof(int))
			{
				EditorGUILayout.LabelField("Base:", GUILayout.Width(40));
				int oldBase = (int)(object)@base;
				@base = (ValueType)(object)EditorGUILayout.IntField(oldBase, GUILayout.Width(50));
			}

			else if (typeof(ValueType) == typeof(float))
			{
				EditorGUILayout.LabelField("Base:", GUILayout.Width(40));
				float oldBase = (float)(object)@base;
				@base = (ValueType)(object)EditorGUILayout.FloatField(oldBase, GUILayout.Width(50));
			};

			EditorGUILayout.LabelField("Value: " + Value.ToString(), GUILayout.Width(100));

			if (showModifiers = EditorGUILayout.Foldout(showModifiers, "Modifiers"))
			{
				EditorGUILayout.BeginVertical();

				EditorGUI.indentLevel++;

				Dictionary<string, Modifier<ValueType>> newModifiers = new(modifiers);

				foreach (var modifier in newModifiers)
				{
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField(modifier.Key);

					EditorGUILayout.EndHorizontal();
				};

				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
			};
		}

		#endregion

#endif
	};


#if UNITY_EDITOR

	[Serializable]
	abstract public class AttributeGUI
	{
		virtual public void DoGUI()
		{ }

		public static void DoAttributesGUI<AttributeHolderType>(
			AttributeHolderType obj)
		{
			EditorGUILayout.BeginVertical();

			// Get the type of the provided object
			Type objType = obj.GetType();

			// Iterate through all fields and properties of the object
			foreach (var member in objType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object value = null;

				// Check if the member is a field
				if (member is FieldInfo field && typeof(AttributeGUI).IsAssignableFrom(field.FieldType))
					value = field.GetValue(obj);
				// Check if the member is a property
				else if (member is PropertyInfo property && typeof(AttributeGUI).IsAssignableFrom(property.PropertyType))
					value = property.GetValue(obj);

				// If the member is of type AttributeGUI, call DoGUI
				if (value is AttributeGUI attribute)
				{
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField(ConvertToReadableFormat(member.Name) + ": ", GUILayout.Width(100));

					attribute.DoGUI();

					EditorGUILayout.EndHorizontal();
				};
			};

			EditorGUILayout.EndVertical();
		}

		private static string ConvertToReadableFormat(string name)
		{
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			// Regex to insert spaces before capital letters, ignoring all-uppercase words
			var readableName = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");

			// Capitalize the first letter of each word
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(readableName);
		}
	};

#endif
};
