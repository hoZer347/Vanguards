using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class DialogueBox : MonoBehaviour
	{
		[SerializeField]
		TextAsset textAsset;

		[SerializeField, Range(0.001f, 10)]
		float textSpeed;
		float timeElapsed;

		int charIndex;
		TextMeshProUGUI textMesh;

		private void OnValidate()
			=> textMesh = GetComponentInChildren<TextMeshProUGUI>();

		private void Update()
		{
			if (timeElapsed > textSpeed)
			{
				Advance();
				timeElapsed -= textSpeed;
			};

			timeElapsed += Time.deltaTime * 100;
		}

		public void Skip()
		{

		}

		public bool IsDone()
			=> charIndex < textAsset.text.Length;

		public void Clear()
		{
			charIndex = 0;
			textMesh.SetText("");
		}

		public void Advance()
		{
			if (textAsset == null || charIndex >= textAsset.text.Length)
				return;

			int start = charIndex;

			if (textAsset.text[charIndex] == '<')
			{
				while (charIndex < textAsset.text.Length &&
					textAsset.text[charIndex] != '>')
					charIndex++;

				if (charIndex < textAsset.text.Length)
					charIndex++;
			}
			else charIndex++;

			if (charIndex > textAsset.text.Length)
				textMesh.SetText("");

			if (charIndex <= textAsset.text.Length)
				textMesh.SetText(textAsset.text.Substring(0, charIndex));
		}

		static public void Play()
		{
			DialogueBox dialogueBox = GameObject.FindFirstObjectByType<DialogueBox>();
			dialogueBox.charIndex = 0;
			dialogueBox.textMesh.SetText("");
			State.SetState(new St_Dialogue("", StateMachine.Current));
		}

		static public void Stop()
		{
			DialogueBox dialogueBox = GameObject.FindFirstObjectByType<DialogueBox>();
			dialogueBox.charIndex = dialogueBox.textAsset.text.Length;
			dialogueBox.textMesh.SetText("");
			State.SetState(new St_Dialogue("", StateMachine.Current));
		}
	};

	#region GUI

#if UNITY_EDITOR

	[CustomEditor(typeof(DialogueBox))]
	public class DialogueBoxEditor : Editor
	{
		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			//DialogueBox dialogueBox = (DialogueBox)target;

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Play"))
				DialogueBox.Play();

			if (GUILayout.Button("Stop"))
				DialogueBox.Stop();

			GUILayout.EndHorizontal();
		}
	};

#endif

	#endregion
};
