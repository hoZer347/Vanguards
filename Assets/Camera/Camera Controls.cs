using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	internal class CameraControls : MonoBehaviour
	{
		enum Mode
		{
			TopDown,
		};

		[SerializeField]
		Mode mode;

		[SerializeField]
		float cameraStrafeSpeed;

		[SerializeField]
		float cameraRotationSpeed;
		float cameraSpin = 0.0f;
		float cameraTilt = 0.0f;

		[SerializeField, Range(minHeight, maxHeight)]
		float height;
		const float minHeight = 5.0f;
		const float maxHeight = 10.0f;

		[SerializeField]
		internal Transform target = null;
		Vector3 targetPosition = new(0.0f, 0.0f, 0.0f);
		Vector3 cameraPosition;

		Vector3 currMousePosition;
		Vector3 lastMousePosition;

		private void Start()
		{
			transform.position = Map.main.transform.position + Vector3.up * height;
			transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
		}

		private void Update()
		{
			switch (mode)
			{
				case Mode.TopDown:
					DoTopDown();

					break;



				//...
			}
		}

		void DoTopDown()
		{
			// Managing Rotation
			if (Input.GetMouseButtonDown(2))
			{
				lastMousePosition = Input.mousePosition;
			}
			else if (Input.GetMouseButton(2))
			{
				currMousePosition = Input.mousePosition;
				cameraSpin += currMousePosition.x - lastMousePosition.x;
				cameraTilt -= currMousePosition.y - lastMousePosition.y;
				lastMousePosition = currMousePosition;
			}

			// Managing Zoom
			height -= Input.mouseScrollDelta.y;
			height = Mathf.Clamp(height, minHeight, maxHeight);

			// Managing WASD Movement
			Vector3 movement = new(0, 0, 0);
			if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
			if (Input.GetKey(KeyCode.A)) movement -= Vector3.right;
			if (Input.GetKey(KeyCode.S)) movement -= Vector3.forward;
			if (Input.GetKey(KeyCode.D)) movement += Vector3.right;
			targetPosition += Quaternion.Euler(0.0f, cameraSpin, 0.0f) * movement * Time.deltaTime * cameraStrafeSpeed;

			targetPosition.x = Mathf.Clamp(targetPosition.x, 0, Map.main.width);
			targetPosition.z = Mathf.Clamp(targetPosition.z, 0, Map.main.height);

			cameraTilt = Mathf.Clamp(cameraTilt, 0, 40);

			transform.position = targetPosition + Quaternion.Euler(cameraTilt, cameraSpin, 0.0f) * (Vector3.up * height + -Vector3.forward * height);
			transform.LookAt(targetPosition);
		}
	};
};
