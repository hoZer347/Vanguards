using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	[RequireComponent(typeof(Attributes))]
	public class Item : MonoBehaviour
	{
		[SerializeField]
		Texture2D icon;

		public Attributes attributes;

		#region Refresh

		void Refresh()
		{
			attributes = GetComponent<Attributes>();
		}

		private void Start() => Refresh();

		private void OnValidate() => Refresh();

		#endregion

		virtual public void OnEquip()
		{ }

		virtual public void OnUnequip()
		{ }
	};
};
