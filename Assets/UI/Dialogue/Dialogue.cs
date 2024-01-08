using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EasyButtons;
using Unity.VisualScripting;


namespace Vanguards
{
	internal class Dialogue : MonoBehaviour
	{
		[SerializeField]
		internal TextAsset textInput = null;
		internal TextMeshProUGUI textOutput = null;
		internal string currChunk = "";
		internal List<string> chunks = new();
		internal RectTransform rectTransform;
		internal float textButtonMoveSpeed = 300.0f;

		long fsIndex = 0;

		bool running = false;
		bool Running
		{
			get => running;
			set
			{
				running = value;
				GetComponentInChildren<Button>().interactable = value;
			}
		}

		[SerializeField]
		float textSpeed = 1.0f;
		float timeElapsed = 0.0f;

		Button buttonObject = null;

		private void Start()
		{
			buttonObject = GetComponentInChildren<Button>();
			textOutput = GetComponentInChildren<TextMeshProUGUI>();
			rectTransform = buttonObject.GetComponent<RectTransform>();

			buttonObject.GetComponent<Button>().onClick.AddListener(delegate ()
			{
				if (chunks.Count <= 0)
				{
					OnFinished();
					return;
				};

				textOutput.text = chunks.First();
				textOutput.ForceMeshUpdate();

				chunks.Remove(chunks.First());
			});
		}

		private void Update()
		{
			Vector3 goalPosition;

			if (running)
				goalPosition = new(0, 0, 0);
			else
				goalPosition = new(0, -rectTransform.rect.height, 0);

			Vector3 movementDirection = (goalPosition - buttonObject.transform.position).normalized * Time.deltaTime * textButtonMoveSpeed;

			if (Vector3.Distance(goalPosition, buttonObject.transform.position) < Time.deltaTime * textButtonMoveSpeed)
				buttonObject.transform.position = goalPosition;
			else
				buttonObject.transform.Translate(movementDirection);
		}

		private void OnValidate()
		{
			if (!running && textInput != null)
			{
				chunks = textInput.text.Split('\n').ToList();
				Running = true;
			}
			else if (running && textInput == null)
			{
				chunks.Clear();
				Running = false;
			};
		}

		void OnFinished()
		{
			Running = false;
			textInput = null;
		}
	};
};
