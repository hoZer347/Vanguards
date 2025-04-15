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
		};

		[SerializeField]
		Texture2D icon;

		[HideInInspector] public Attribute<string> NAME;
		[HideInInspector] public Attribute<Class> CLASS;

		#region Refresh

		void Refresh()
		{

		}

		private void Start() => Refresh();

		private void OnValidate() => Refresh();

		#endregion
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
