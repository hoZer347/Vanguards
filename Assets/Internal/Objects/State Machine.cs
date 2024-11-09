using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class StateMachine : MonoBehaviour
	{
		[SerializeField]
		string currentStateName;
		static private StateMachine main;

		static public State current = null;
		static public State Current => current;

		static Stack<State> stateStack = new();
		static Stack<State> memoryStack = new();

		#region Refresh

		private void OnValidate() => Refresh();
		private void Start() => Refresh();

		void Refresh()
		{
			main = this;
		}

		#endregion

		private void Update()
		{
			if (Current != null)
				Current.OnUpdate();
			else Debug.Log("State is Null");
		}

		static public void SetState<_State>()
			where _State : State, new()
		{
			_State newState = new _State();
			State oldState = Current;

			memoryStack.Clear();

			newState.name = typeof(_State).ToString();

			newState.OnEnter();

			current = newState;

			stateStack.Push(current);

			if (oldState != null)
				oldState.OnLeave();

			if (main != null)
				main.currentStateName = current.name;
		}

		static public void Undo()
		{
			if (stateStack.Count > 1)
			{
				memoryStack.Push(stateStack.Pop());
				stateStack.Peek().OnEnter();
				current = stateStack.Peek();
				main.currentStateName = stateStack.Peek().name;
			};
		}

		static public void Redo()
		{
			if (memoryStack.Count > 0)
			{
				stateStack.Push(memoryStack.Pop());
				stateStack.Peek().OnEnter();
				current = stateStack.Peek();
				main.currentStateName = stateStack.Peek().name;
			};
		}
	};

	public class State
	{
		static public void SetState<_State>()
			where _State : State, new()
				=> StateMachine.SetState<_State>();

		public string name = "";

		virtual public void OnEnter() { }
		virtual public void OnUpdate() { }
		virtual public void OnLeave() { }
	};
};
