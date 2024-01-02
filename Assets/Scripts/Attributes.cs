using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	using Modifiers = System.Collections.Generic.List<Modifier>;
	using MarkedModifiers = System.Collections.Generic.Dictionary<MType, Modifier>;

	delegate void Modifier(ref byte attributeBase);

	enum MType
	{

	};

	[Serializable]
	internal class Attributes : MonoBehaviour
	{
		internal Attribute this[string name]
		{
			get
			{
				foreach (Attribute attribute in attributes)
					if (attribute.name == name)
						return attribute;

				return null;
			}
		}
		internal byte GetValue(string name) => this[name].Value;
		internal byte GetBase(string name) => this[name].Base;
		internal Attribute GetAttribute(string name) => this[name];
		[Button] void ShowUpdatedModifiers()
		{
			foreach (Attribute attribute in attributes)
				attribute.ShowUpdatedModifiers();
		}

		[SerializeField]
		protected List<Attribute> attributes;
	};

	[Serializable]
	internal class Attribute
	{
		[SerializeField]
		internal string name;

		internal byte Value
		{
			get
			{
				byte _value = _base;

				foreach (var modifier in modifiers)
					modifier(ref _value);

				foreach (var modifier in markedModifiers.Values)
					modifier(ref _value);

				return _value;
			}
		}
		internal byte Base => _base;

		internal void AddModifier(Modifier modifier)
		{
			modifiers.Add(modifier);
		}

		internal void SetMarkedModifier(MType modifierType, Modifier modifier)
		{
			markedModifiers[modifierType] = modifier;
		}

		[Button]
		internal void ShowUpdatedModifiers()
		{
			//modifierNames = new();
			//markedModifierNames = new();

			//foreach (Modifier modifier in modifiers)
			//	modifierNames.Add(modifier.ToString());

			//foreach (var modifier in markedModifiers)
			//	markedModifierNames.Add(modifier.Key.ToString() + " : " + modifier.Value.ToString());
		}

		//[SerializeField]
		//List<string> modifierNames;

		//[SerializeField]
		//List<string> markedModifierNames;

		Modifiers modifiers = new();
		MarkedModifiers markedModifiers = new();

		[SerializeField] byte _base;
	};
};
