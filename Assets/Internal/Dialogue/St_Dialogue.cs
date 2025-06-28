using System;
using System.IO;
using TMPro;
using UnityEngine;


namespace Vanguards
{
	public class St_Dialogue : WeakState
	{
		DialogueBox dialogueBox;
		string path;
		State stateAfter;

		public St_Dialogue(string path, State stateAfter)
		{
			this.path = path;
			this.stateAfter = stateAfter;
		}

		override public void OnEnter()
		{
			dialogueBox = GameObject.FindFirstObjectByType<DialogueBox>();
			dialogueBox.Load(path);
		}

		override public void OnUpdate()
		{
			dialogueBox.Advance();

			if (dialogueBox.IsDone())
			{
				dialogueBox.Clear();
				SetState(stateAfter);
			};
		}

		override public void OnUndo()
		{
			dialogueBox.Clear();
		}
	};
};
