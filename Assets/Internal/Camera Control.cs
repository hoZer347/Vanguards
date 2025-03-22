using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class CameraControl : MonoBehaviour
	{
		enum Mode
		{
			FREECAM,
		};

		Mode mode = Mode.FREECAM;

		private void Start()
		{
			
		}

		private void Update()
		{
			switch (mode)
			{
				case Mode.FREECAM:
					FreeCam();
					break;
			};
		}

		private Vector2 prevMousePosition;
		private Vector2 currMousePosition;
		private Vector2 delta;

		void FreeCam()
		{
			Vector3 movement = Vector3.zero;
			
			if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
			if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
			if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
			if (Input.GetKey(KeyCode.D)) movement += Vector3.right;
			if (Input.GetKey(KeyCode.Q)) movement += Vector3.down;
			if (Input.GetKey(KeyCode.E)) movement += Vector3.up;

			if (Input.GetMouseButtonDown(1))
				prevMousePosition = Input.mousePosition;

			else if (Input.GetMouseButton(1))
			{
				currMousePosition = Input.mousePosition;

				delta = currMousePosition - prevMousePosition;

				transform.rotation = Quaternion.Euler(
					transform.rotation.eulerAngles.x - delta.y * Time.deltaTime * 100,			
					transform.rotation.eulerAngles.y + delta.x * Time.deltaTime * 100,
					transform.rotation.eulerAngles.z);

				prevMousePosition = currMousePosition;
			};

			transform.position +=
				Quaternion.Euler(
					0,
					transform.rotation.eulerAngles.y,
					0) * movement * Time.deltaTime * 10.0f;
		}
	};
};
