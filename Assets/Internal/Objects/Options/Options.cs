using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class St_Mp_Option : St_MapState
	{
		public override void OnUpdate()
		{
			// TODO: Move to Op_Wait

			selectedUnit.ActionUsed = true;
			originalCell.SetUnit(null);
			Map.main[selectedUnit.transform.position].SetUnit(selectedUnit);
			originalCell = Map.main[selectedUnit.transform.position];

			SetState<St_Mp_InitialState>();
		}
	};

	public class Op_Wait : St_Mp_Option
	{ };

	public class Op_Attack : St_Mp_Option
	{ };

	public class Op_Equip : St_Mp_Option
	{ };

	public class Op_Item : St_Mp_Option
	{ };
};
