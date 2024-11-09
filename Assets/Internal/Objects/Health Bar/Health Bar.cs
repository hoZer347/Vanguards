using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class HealthBar : MonoBehaviour
	{
		static MeshFilter meshFilter;

		private void Start()
		{
			meshFilter = GetComponent<MeshFilter>();
		}


	};
};
