using System;
using UnityEngine;


namespace Vanguards
{
	public class UnitOptionMenu : Menu
	{
		static UnitOptionMenu instance;

		private void Start() => instance = this;

		#region Unit Option Handling

		public void SetOptions(Unit unit)
		{
			HandleItemCategory<Weapon, Op_Attack>(unit);
			HandleItemCategory<Staff, Op_Staff>(unit);
			HandleItemCategory<Consumable, Op_Consumable>(unit);
			HandleItemCategory<Spell, Op_Spell>(unit);

			MakeButton("General: ", null);
			indentLevel++;
			MakeButton("Wait", () => State.SetState(new Op_Wait(unit)));
			indentLevel--;
		}

		private void HandleItemCategory<_ItemType, _OptionType>(Unit unit)
			where _ItemType : Item
			where _OptionType : St_Option
		{
			_ItemType[] items = unit.GetComponentsInChildren<_ItemType>();
			Array.Reverse(items);
			MenuButton optionButton;

			if (items.Length > 0)
			{
				MakeButton($"{typeof(_ItemType).Name}s: ", null);
				indentLevel++;

				foreach (_ItemType item in items)
				{
					if (item is Equippable)
					{
						string displayPrefix = "";
						Action onClick;

						if (item == unit.equipped)
						{
							displayPrefix += "Unequip: ";
							onClick = () => State.SetState(new Op_Equip(unit, null));
						}
						else
						{
							displayPrefix += "Equip:   ";
							onClick = () => State.SetState(new Op_Equip(unit, item as Equippable));
						};

						optionButton = MakeButton(displayPrefix + item.NAME.Value, onClick);
					}
					else
						optionButton = MakeButton("Use:   " + item.NAME.Value, () =>
						{
							_OptionType option = (_OptionType)Activator.CreateInstance(
								typeof(_OptionType),
								unit,
								item);

							State.SetState(option);
						});
				};

				indentLevel--;
			};
		}

		#endregion
	};
};
