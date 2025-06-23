using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class HealthBar : MonoBehaviour
	{
		MeshFilter meshFilter;
		MeshRenderer meshRenderer;

		[SerializeField]
		Unit unit = null;

		private void Start()
		{
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
			unit = GetComponentInParent<Unit>();

			meshFilter.mesh = new Mesh
			{
				name = "Health Bar",
				vertices = new Vector3[]
				{
					new Vector3(-0.4f, 0.9f, 0.0f),
					new Vector3( 0.4f, 0.9f, 0.0f),
					new Vector3( 0.4f, 1.0f, 0.0f),
					new Vector3(-0.4f, 1.0f, 0.0f)
				},
				triangles = new int[] { 0, 2, 1, 3, 2, 0 },
				uv = new Vector2[]
				{
					new Vector2(1.0f, 0.0f),
					new Vector2(0.0f, 0.0f),
					new Vector2(0.0f, 1.0f),
					new Vector2(1.0f, 1.0f)
				}
			};

			Refresh();
		}

		public void Refresh()
		{
			float damageRatio = (float)(unit.HP.Base - unit.HP.Value) / Mathf.Max(unit.HP.Base, 1.0f);

			meshRenderer.material.SetFloat(
				"_DamageRatio",
				damageRatio);

			if (damageRatio == 0.0f)
				meshRenderer.enabled = false;
			else meshRenderer.enabled = true;
		}
	};
};
