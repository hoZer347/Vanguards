using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Vanguards
{
	[ExecuteAlways]
	internal class FPSShower : MonoBehaviour
	{
		[SerializeField] int resolution;

		TextMeshProUGUI text;

		private void Start()
		{
			text = GetComponentInChildren<TextMeshProUGUI>();
		}

		float average;
		List<float> times = new();

		private void Update()
		{
			average += Time.deltaTime;
			times.Add(Time.deltaTime);

			if (times.Count > resolution)
			{
				average -= times.First();
				times.Remove(times.First());
			};

			text.text = "  FPS: " + (int)(1 / (average / resolution));
			text.ForceMeshUpdate();
		}
	}
};
