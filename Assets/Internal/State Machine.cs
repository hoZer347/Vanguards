using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class StateMachine : MonoBehaviour
	{
		static public State Current => stateStack.Peek();

		static public Stack<State> stateStack = new();

		private void Update()
			=> Current.OnUpdate();

		static public void SetState<_State>(_State state)
			where _State : State
		{
			State oldState;
			if (stateStack.Count > 0) oldState = Current;
			else oldState = null;

			state.type = typeof(_State);

			if (oldState != null)
				oldState.OnLeave();

			state.OnEnter();

			stateStack.Push(state);
		}

		static public void FallBack<_State>(bool undo = false)
			where _State : State
		{
			while (stateStack.Count > 0)
			{
				if (stateStack.Peek().type == typeof(_State))
				{
					Current.OnEnter();
					return;
				};

				stateStack.Pop();
			};
		}
	};

	[Serializable]
	public class State
	{
		static public void SetState<_State>(_State state)
			where _State : State
				=> StateMachine.SetState<_State>(state);

		static public void FallBack<_State>(bool undo = false)
			where _State : State
			=> StateMachine.FallBack<_State>(undo);

		static public State Current => StateMachine.Current;

		public Type type = typeof(State);

		virtual public void OnEnter() { }
		virtual public void OnUpdate() { }
		virtual public void OnLeave() { }
	};

#if UNITY_EDITOR
	[CustomEditor(typeof(StateMachine))]
	public class StateMachineEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			StateMachine stateMachine = ((StateMachine)target);

			GUILayout.BeginVertical();

			foreach (State state in StateMachine.stateStack)
				EditorGUILayout.LabelField(state.type.FullName);

			GUILayout.EndVertical();
		}
	};
#endif
};
