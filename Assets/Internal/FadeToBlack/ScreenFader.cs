using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	// Enacts a screen fade out by initiating a St_Fade state
	// that fades in or out a rawImage over the screen
	public class ScreenFader : MonoBehaviour
	{
		static ScreenFader screenFader;

		[SerializeField]
		Color color;

		[SerializeField]
		float speed;

		[SerializeField]
		bool drawOverUI; // TODO: Implement this

		[SerializeField]
		State afterState;

		RawImage image;

		static public void FadeToBlack<_AfterState>(_AfterState afterState)
			where _AfterState : State
		{
			if (screenFader == null)
				screenFader = FindAnyObjectByType<ScreenFader>();

			if (screenFader != null)
				State.SetState(new St_Fade(
					afterState,
					new Color(
						screenFader.color.r,
						screenFader.color.g,
						screenFader.color.g,
						0),
					screenFader.color,
					screenFader.speed));
		}

		static public void FadeFromBlack<_AfterState>(_AfterState afterState)
			where _AfterState : State
		{
			if (screenFader == null)
				screenFader = FindAnyObjectByType<ScreenFader>();
			if (screenFader != null)
				State.SetState(new St_Fade(
					afterState,
					screenFader.color, 
					new Color(
						screenFader.color.r,
						screenFader.color.g,
						screenFader.color.g,
						0),
					screenFader.speed));
		}
	};

	public class St_Fade : State
	{

		RawImage rawImage;
		Color from, to;
		float speed;
		dynamic[] args;
		State afterState;

		public St_Fade(
			State afterState,
			Color from,
			Color to,
			float speed,
			params dynamic[] args)
		{
			this.afterState = afterState;
			this.from = from;
			this.to = to;
			this.speed = speed;

			rawImage = GameObject.FindObjectOfType<ScreenFader>()
					.GetComponentInChildren<RawImage>();

			rawImage.color = from;

			this.args = args;
		}

		public override void OnEnter() { }

		public override void OnUpdate()
		{
			// Set anchors to stretch in all directions (full screen)
			RectTransform rectTransform = rawImage.rectTransform;
			rectTransform.anchorMin = Vector2.zero;   // Bottom-left
			rectTransform.anchorMax = Vector2.one;    // Top-right
			rectTransform.offsetMin = Vector2.zero;   // Remove padding (left/bottom)
			rectTransform.offsetMax = Vector2.zero;   // Remove padding (right/top)

			if (from == to)
				SetState(afterState);
			else
			{
				from = Color.Lerp(from, to, speed * Time.deltaTime);
				rawImage.color = from;
			};
		}

		public override void OnLeave() { }
	};

	#region GUI

#if UNITY_EDITOR

	[CustomEditor(typeof(ScreenFader))]
	public class FadeToBlackEditor : Editor
	{
		override public void OnInspectorGUI()
		{
			DrawDefaultInspector();

			ScreenFader screenFader = (ScreenFader)target;

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Fade to Black"))
				ScreenFader.FadeToBlack(new St_Mp_InitialState());

			if (GUILayout.Button("Fade from Black"))
				ScreenFader.FadeFromBlack(new St_Mp_InitialState());
			
			EditorGUILayout.EndHorizontal();

			EditorGUI.EndDisabledGroup();
		}
	};

#endif

	#endregion
};
