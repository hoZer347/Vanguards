using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class Surface : MonoBehaviour
	{
		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			gameObject.layer = LayerMask.NameToLayer("Surface");
		}
	};
};
