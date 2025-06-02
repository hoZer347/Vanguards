using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class Item : MonoBehaviour
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

		[HideInInspector] public Attribute<string> NAME;
		[HideInInspector] public Attribute<Class> CLASS;

		private void OnValidate()
		{
			name = NAME.Value;
		
			
		}
	};

#if UNITY_EDITOR
	[CustomEditor(typeof(Item), true)]
	public class ItemEditor : Editor
	{
		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			Item item = (Item)target;

			AttributeGUI.DoAttributesGUI(item);
		}
	};
#endif
};
