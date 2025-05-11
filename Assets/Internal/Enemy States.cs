using UnityEngine;


namespace Vanguards
{
	public class St_EnemyState : State
	{
		Unit currentUnit;

		public St_EnemyState(Unit currentUnit) : base()
			=> this.currentUnit = currentUnit;
	};
};
