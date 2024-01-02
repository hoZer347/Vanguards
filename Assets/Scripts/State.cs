using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	internal class State<T>
	{
		static internal State<T> Current
		{
			get => current;
			set
			{
				if (current != null)
					current.OnLeave();
				current = value;
				current.OnEnter();
			}
		}
		static private State<T> current = new();

		static internal void Update() => current.OnUpdate();

		virtual protected void OnEnter() { }
		virtual protected void OnUpdate() { }
		virtual protected void OnLeave() { }
	};
};
