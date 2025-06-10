using System;
using UnityEngine;


namespace Vanguards
{
	public class TopLeftMenu : Menu
	{
		static public TopLeftMenu instance;

		private void Start()
			=> instance = this;

		#region Item Displaying

		static public void DisplayItem<_ItemType>(_ItemType item)
			where _ItemType : Item
		{
			if (item is Consumable consumable)
				DisplayConsumable(consumable);
			else if (item is Weapon weapon)
				DisplayWeapon(weapon);
			else if (item is Staff staff)
				DisplayStaff(staff);
			else
				Debug.LogWarning($"ObjectDisplayer: DisplayItem called with unsupported item type {typeof(_ItemType).Name}");
		}

		static public void DisplayWeapon(Weapon weapon)
		{
			
		}

		static public void DisplayStaff(Staff staff)
		{

		}

		static public void DisplayConsumable(Consumable consumable)
		{

		}

		public void DisplayAttack(Unit attacker, Unit receiver)
		{
			MakeButton("Attack", null);
			indentLevel++;

			int gridDistance =
				Mathf.Abs(attacker.coords.x - receiver.coords.x) +
				Mathf.Abs(attacker.coords.y - receiver.coords.y);

			Weapon[] weapons = attacker.GetComponentsInChildren<Weapon>();

			foreach (Weapon weapon in weapons)
				if (weapon.MIN_RNG.Value >= gridDistance &&
					weapon.RNG.Value <= gridDistance)
				{
					MenuButton optionButton = MakeButton(weapon.NAME.Value,
						() =>
						{
							attacker.equipped = weapon;
							weapon.transform.SetAsLastSibling();
							State.SetState(new An_Attack(attacker, receiver));
							ClearOptions();
						});

					optionButton.onHover =
						() =>
						{
							// TODO: Replace with stats

							indentLevel++;

							foreach (var item in weapons)
								MakeButton(item.NAME.Value, () => { }, MenuButton.TYPE.TEMPORARY);

							indentLevel--;
						};

					optionButton.onUnHover =
						() => ClearOptions(MenuButton.TYPE.TEMPORARY);
				};

			indentLevel--;
		}

		#endregion
	};
};
