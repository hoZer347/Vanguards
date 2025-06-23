using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class Item : Saveable
	{
		public enum Class
		{
			SWORD,
			BOW,
			DAGGER,
			POTION,
		};

		public St_Option useOption;
		public List<St_Option> alternateUseOptions = new();

		[SerializeField]
		string fileOrigin = string.Empty;

		[SerializeField]
		Texture2D icon;

		public Attribute<string> NAME;
		public Attribute<Class> CLASS;

		private void OnValidate()
			=> NAME.SetBase(name);
	};
};
