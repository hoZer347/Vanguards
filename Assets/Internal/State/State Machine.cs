using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class StateMachine : MonoBehaviour
	{
		static private State current;

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
