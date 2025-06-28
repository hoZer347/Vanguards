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
			float labelWidth = EditorGUIUtility.labelWidth;
			const float foldoutWidth = 16f;
			const float spacing = 4f;

			Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
			Rect fieldRect = new Rect(position.x + labelWidth - 10, position.y, position.width - labelWidth - foldoutWidth - spacing, position.height);
			Rect foldoutRect = new Rect(position.x + position.width - foldoutWidth, position.y, foldoutWidth, position.height);

			SerializedProperty baseProp = property.FindPropertyRelative("base");

			EditorGUI.LabelField(labelRect, label);

			if (baseProp != null)
			{
				EditorGUI.showMixedValue = baseProp.hasMultipleDifferentValues;

				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(fieldRect, baseProp, GUIContent.none, true);
				if (EditorGUI.EndChangeCheck())
					baseProp.serializedObject.ApplyModifiedProperties();

				EditorGUI.showMixedValue = false;
			}
			else EditorGUI.LabelField(fieldRect, "base", "Field not found");
			
			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
		}
	};

#endif
};
