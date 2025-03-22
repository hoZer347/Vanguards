using System;
using System.IO;
using UnityEngine;


namespace Vanguards
{
	public class Dialogue : State
	{
		string line = "";
		string[] lines;
		int currLine, currChar;
		Action onFinish;
		DialogueBox dialogueBox;
		float accumulatedTime;

		public Dialogue(string file_name, Action onFinish)
		{
			dialogueBox = GameObject.FindAnyObjectByType<DialogueBox>();
			
			currLine = 0;
			lines = File.ReadAllLines(file_name);
			line = lines[0];
			this.onFinish = onFinish;

			dialogueBox.Clear();
		}

		override public void OnUpdate()
		{
			DialogueBox dialogueBox = GameObject.FindAnyObjectByType<DialogueBox>();

			if (currLine >= lines.Length)
			{
				SetState<St_Mp_InitialState>();
				return;
			};

			line = lines[currLine];

			if (currChar >= line.Length)
			{
				currLine++;
				currChar = 0;
				return;
			};

			if (line[currChar] == '<')
			{
				int i = line.IndexOf('>', currChar);

				string toPush = line.Substring(currChar, i - currChar + 1);
				dialogueBox.Push(toPush);
				currChar += toPush.Length;

				return;
			};

			if (accumulatedTime > 1)
			{
				int iTime = (int)accumulatedTime;

				dialogueBox.Push(line.Substring(currChar, iTime));

				currChar += iTime;
				accumulatedTime -= iTime;
			};

			accumulatedTime += Time.deltaTime * dialogueBox.textSpeed;
		}
	};
};
