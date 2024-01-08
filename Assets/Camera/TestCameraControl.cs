using UnityEngine;
using Unity;

public class TestCameraControl : MonoBehaviour
{
	public float rotationSpeed = 1.0f;
	public float zoomSpeed = 1.0f;
	public float minZoomDistance = 2.0f;
	public float maxZoomDistance = 20.0f;

	private Vector3 target;
	private float currentZoomDistance;

	private Transform sphere = null;

	void Start()
	{
		target = Vector3.zero;
		transform.LookAt(target);
		sphere = transform.GetChild(0);

		currentZoomDistance = Vector3.Distance(transform.position, target);
	}

	[SerializeField]
	internal bool buh = false;

	void Update()
	{
		// Rotation with middle mouse button
		if (Input.GetMouseButton(2))
		{
			float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
			float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

			// Rotate around the target
			transform.RotateAround(target, Vector3.up, mouseX);
			transform.RotateAround(target, transform.right, mouseY);
		};

		// Zoom with scroll wheel
		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(scrollWheel) > 0.0f)
		{
			currentZoomDistance = Mathf.Clamp(currentZoomDistance - scrollWheel * zoomSpeed, minZoomDistance, maxZoomDistance);
			Vector3 newPosition = target - transform.forward * currentZoomDistance;
			transform.position = newPosition;
		};

		// Lock the camera to the ray hit
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				target = hit.point;
			}
			else
			{
				target = Vector3.zero;
			};
		};

		// Always look at the target
		transform.LookAt(target);
	}
}
