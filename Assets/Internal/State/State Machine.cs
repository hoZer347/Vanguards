using System;
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
		static public Stack<State> redoStack = new();

		private void Update()
		{
			if (stateStack.Count == 0)
				return;

			Current.OnUpdate();
		}

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

			redoStack.Clear();
		}

		static public void FallBack<_State>()
			where _State : State
		{
			while (stateStack.Count > 1)
			{
				redoStack.Push(stateStack.Peek());
				stateStack.Peek().OnUndo();
				stateStack.Pop();

				if (stateStack.Peek().type == typeof(_State))
				{
					Current.OnEnter();
					return;
				};
			};

			if (stateStack.Count == 0)
				SetState(new St_Mp_InitialState());
		}

		static public void Undo()
		{
			if (stateStack.Count == 1)
				return;

			do
			{
				redoStack.Push(stateStack.Peek());
				stateStack.Peek().OnUndo();
				stateStack.Pop();
			}
			while (stateStack.Peek() is WeakState);

			stateStack.Peek().OnEnter();
		}

		//static public void Redo()
		//{
		//	if (redoStack.Count == 0)
		//		return;

		//	redoStack.Peek().OnRedo();
		//	State state = redoStack.Pop();
		//	stateStack.Push(state);
		//	state.OnEnter();
		//}
	};

	[Serializable]
	public class State
	{
		static public void SetState<_State>(_State state)
			where _State : State
				=> StateMachine.SetState<_State>(state);

		static public void FallBack<_State>()
			where _State : State
			=> StateMachine.FallBack<_State>();

		static public void Undo() => StateMachine.Undo();
		
		//static public void Redo() => StateMachine.Redo();

		static public State Current => StateMachine.Current;

		public Type type = typeof(State);

		virtual public void OnEnter() { }
		virtual public void OnUpdate() { }
		virtual public void OnLeave() { }

		virtual public void OnUndo() { }
		virtual public void OnRedo() { }
	};

	public class WeakState : State
	{ }

#if UNITY_EDITOR
	
	[CustomEditor(typeof(StateMachine))]
	public class StateMachineEditor : Editor
	{
		static bool showUndo = true;
		static bool showRedo = true;

		override public void OnInspectorGUI()
		{
			StateMachine stateMachine = (StateMachine)target;

			GUILayout.BeginVertical();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			GUILayout.BeginHorizontal();

			EditorGUI.BeginDisabledGroup(StateMachine.stateStack.Count < 2);
			if (GUILayout.Button("Undo"))
				State.Undo();
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(StateMachine.redoStack.Count == 0);
			//if (GUILayout.Button("Redo"))
			//	State.Redo();
			EditorGUI.EndDisabledGroup();

			GUILayout.EndHorizontal();

			EditorGUI.EndDisabledGroup();

			if (StateMachine.stateStack.Count > 0 && 
				(showUndo = EditorGUILayout.Foldout(showUndo, $"Undo: { StateMachine.stateStack.Peek() }")))
				foreach (State state in StateMachine.stateStack)
					EditorGUILayout.LabelField(state.type.FullName);

			if (StateMachine.redoStack.Count > 0 &&
				(showRedo = EditorGUILayout.Foldout(showRedo, $"Redo: { StateMachine.redoStack.Peek() }")))
				foreach (State state in StateMachine.redoStack)
					EditorGUILayout.LabelField(state.type.FullName);

			GUILayout.BeginHorizontal();

			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
	};

#endif
};
