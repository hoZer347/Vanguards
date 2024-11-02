using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class StateMachine : MonoBehaviour
	{
		[SerializeField]
		string currentState;
		static private State current;
		static private StateMachine main;
		static public State Current => current;

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
			if (current != null)
				current.OnUpdate();
		}

		static public void Set<_State>()
			where _State : State, new()
		{
			if (current != null)
				current.OnLeave();

			current = new _State();

			if (main != null)
				main.currentState = typeof(_State).Name;

			if (current != null)
				current.OnEnter();
		}

		static public ref State Get()
		{
			return ref current;
		}
	};

	public class State
	{
		static public void SetState<_State>()
			where _State : State, new()
				=> StateMachine.Set<_State>();

		public virtual void OnEnter() { }
		public virtual void OnUpdate() { }
		public virtual void OnLeave() { }
	}
};
