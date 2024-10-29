using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;


namespace Vanguards
{
	public delegate void Modifier(ref int value);

	public class Attributes : MonoBehaviour
	{
		#region Private Functionality

		[SerializeField]
		List<Attribute> attributeList = new();
		Dictionary<string, Attribute> attributes = new();

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			attributes.Clear();

			foreach (var attribute in attributeList)
				attributes[attribute.Name] = attribute;
		}

		#endregion

		public int GetBase(string name)
		{
			if (!attributes.ContainsKey(name))
				return -1;

			return attributes[name].Base;
		}

		public int GetValue(string name)
		{
			if (!attributes.ContainsKey(name))
				return -1;

			return attributes[name].Value;
		}

		public Attribute GetAttribute(string name)
		{
			if (!attributes.ContainsKey(name))
				return null;

			return attributes[name];
		}

		public void SetModifier(
			string name,
			Modifier modifier)
		{
			if (!attributes.ContainsKey(name))
				return;

			attributes[name].SetModifier(name, modifier);
		}

		public void RmvModifier(string name)
		{
			attributes[name].RmvModifier(name);
		}

		public void RmvAttribute(string name)
		{
			attributes.Remove(name);
		}
	};

	[Serializable]
	public class Attribute
	{
		public void SetBase(int newBase) => @base = newBase;

		public void SetModifier(
			string name,
			Modifier modifier)
			=> modifiers[modifier.Method.Name] = modifier;

		public void RmvModifier(string name)
		{
			modifiers.Remove(name);
		}

		[SerializeField]
		string name;
		public string Name => name;

		public int Base => @base;
		public int Value
		{
			get
			{
				int value = @Base;

				foreach (var modifier in modifiers.Values)
					modifier(ref value);

				return value;
			}
		}
		
		[SerializeField]
		int @base;

		Dictionary<string, Modifier> modifiers = new();
	};
};
