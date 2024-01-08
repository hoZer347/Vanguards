using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Buh : MonoBehaviour
{
	MeshRenderer meshRenderer = null;


	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			GetComponent<MeshRenderer>().material.SetVector("_Cursor_Position", hit.point);
		};
	}
}
