using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public delegate void Modifier<_ValueType>(ref _ValueType value);

	[Serializable]
	public class Attribute<_ValueType> : AttributeBase
	{
		[SerializeField]
		_ValueType @base;

		static int ID = 0;

		public _ValueType Base => @base;

		public _ValueType Value
		{
			get
			{
				_ValueType value = @base;

				foreach ((string _, Modifier<_ValueType> modifier) in modifiers)
					modifier(ref value);

				return value;
			}
		}

		public void SetBase(_ValueType value)
			=> @base = value;
		
		public void SetModifier(string name, Modifier<_ValueType> modifier)
		{
			if (modifier == null)
				throw new ArgumentNullException(nameof(modifier));
			modifiers.TryAdd(name, modifier);
		}

		public string SetModifier(Modifier<_ValueType> modifier)
		{
			if (modifier == null)
				throw new ArgumentNullException(nameof(modifier));

			string rID = $"{ ++ID }";

			modifiers.Add(rID, modifier);

			return rID;
		}

		public void RmvModifier(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Modifier name cannot be null or empty.", nameof(name));
			modifiers.Remove(name);
		}

		public void ClearModifiers()
			=> modifiers.Clear();

		[SerializeField]
#if UNITY_EDITOR
		public
#endif
			Dictionary<string, Modifier<_ValueType>> modifiers = new();
	};

	public class AttributeBase
	{ };

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(AttributeBase), true)]
	public class AttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float labelWidth = EditorGUIUtility.labelWidth; // Removed 'const' keyword to fix CS0133

			const float foldoutWidth = 16f;
			const float spacing = 4f;

			// Define individual rects
			Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
			Rect fieldRect = new Rect(position.x + labelWidth - 10, position.y, position.width - labelWidth - foldoutWidth - spacing, position.height);
			Rect foldoutRect = new Rect(position.x + position.width - foldoutWidth, position.y, foldoutWidth, position.height);

			var baseProp = property.FindPropertyRelative("base");

			if (baseProp != null)
			{
				EditorGUI.LabelField(labelRect, label); // Draw label manually

				switch (baseProp.propertyType)
				{
					case SerializedPropertyType.Integer:
						baseProp.intValue = EditorGUI.IntField(fieldRect, GUIContent.none, baseProp.intValue);
						break;
					case SerializedPropertyType.Float:
						baseProp.floatValue = EditorGUI.FloatField(fieldRect, GUIContent.none, baseProp.floatValue);
						break;
					case SerializedPropertyType.String:
						baseProp.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, baseProp.stringValue);
						break;
					case SerializedPropertyType.Enum:
						baseProp.enumValueIndex = EditorGUI.Popup(fieldRect, baseProp.enumValueIndex, baseProp.enumDisplayNames);
						break;
					default:
						EditorGUI.PropertyField(fieldRect, baseProp, GUIContent.none);
						break;
				};
			}
			else EditorGUI.LabelField(fieldRect, label.text, "Field not found");

			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
		}
	};

#endif
};
