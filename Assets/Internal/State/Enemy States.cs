using UnityEngine;


namespace Vanguards
{
	public class St_EnemyState : WeakState
	{
		Unit currentUnit;

		public St_EnemyState(Unit currentUnit) : base()
			=> this.currentUnit = currentUnit;
	};

	public class St_En_BeginTurn : St_EnemyState
	{
		public St_En_BeginTurn() : base(null)
		{ }

		override public void OnUpdate()
			=> SetState(new St_Mp_InitialState());
	};
};
