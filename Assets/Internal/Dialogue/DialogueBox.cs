using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class DialogueBox : MonoBehaviour
	{
		[SerializeField] public float textSpeed;
		[SerializeField, Range(0.5f, 1)] float xRatio;
		[SerializeField, Range(0.1f, 0.5f)] float yRatio;

		TextMeshProUGUI textMesh;
		Queue<string> charQueue = new();

#if UNITY_EDITOR

		[SerializeField]
		public TextAsset textAsset;

		private void OnValidate()
		{
			textMesh = GetComponentInChildren<TextMeshProUGUI>();

			EditorApplication.delayCall +=
				() =>
				{
					RectTransform rectTransform = textMesh.rectTransform;
					rectTransform.sizeDelta =
						new Vector2(
							Screen.width * xRatio,
							Screen.height * yRatio);
				};
		}

#endif

		private void Start()
		{
			textMesh = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void Push(string text)
		{
			textMesh.text += text;
		}


		public void Clear()
		{
			textMesh.text = "";

			charQueue.Clear();
		}


		static public void End()
		{

		}
	};

	#region GUI

#if UNITY_EDITOR

	[CustomEditor(typeof(DialogueBox))]
	public class DialogueBoxEditor : Editor
	{
		DialogueBox dialogueBox;

		override public void OnInspectorGUI()
		{
			dialogueBox = (DialogueBox)target;

			DrawDefaultInspector();

			EditorGUI.BeginDisabledGroup(
				!Application.isPlaying &&
				dialogueBox.textAsset != null);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Play"))
				Play();

			if (GUILayout.Button("Stop"))
				Stop();

			EditorGUILayout.EndHorizontal();

			EditorGUI.EndDisabledGroup();
		}

		void Play()
		{
			State.SetState(new Dialogue(
				AssetDatabase.GetAssetPath(dialogueBox.textAsset),
				new Action(() => State.SetState(new St_Mp_InitialState()))));
		}

		void Stop()
		{
			DialogueBox.End();

			State.SetState(new St_Mp_InitialState());
		}
	};

#endif

	#endregion
};
