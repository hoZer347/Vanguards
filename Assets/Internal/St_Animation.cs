using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class St_Animation : State
	{
		
	};

	public class An_Attack : St_Animation
	{
		Unit attacker = null;
		Unit attackee = null;

		public An_Attack(
			Unit attacker,
			Unit attackee)
		{
			this.attacker = attacker;
			this.attackee = attackee;
		}

		override public void OnEnter()
		{
			
		}

		override public void OnUpdate()
		{

		}
	};
};
