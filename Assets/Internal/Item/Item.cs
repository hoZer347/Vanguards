using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class Item : MonoBehaviour
	{
		[SerializeField]
		Texture2D icon;

		#region Refresh

		void Refresh()
		{

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
