using System;
using UnityEngine;


namespace Vanguards
{
	public class WeaponSelectionMenu : Menu
	{
		static public WeaponSelectionMenu instance;

		private void Start()
			=> instance = this;

		#region Item Displaying

		public void DisplayWeaponOptions(Unit attacker, Unit receiver)
		{
			yTranslation -= optionButtonTemplate.GetComponent<RectTransform>().rect.height;

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

							foreach (Weapon weapon in weapons)
								if (weapon.MIN_RNG.Value >= gridDistance &&
									weapon.RNG.Value <= gridDistance)
										DisplayWeaponOption(attacker, receiver, weapon);

							indentLevel--;
						};

					optionButton.onUnHover =
						() => ClearOptions(MenuButton.TYPE.TEMPORARY);
				};

			indentLevel--;
		}

		private void DisplayWeaponOption(
			Unit attacker,
			Unit attackee,
			Weapon weapon)
		{
			int defenderDefense = attackee.DEF.Value;
			int attackerStrength = attacker.STR.Value;
			int weaponPower = weapon.PWR.Value;
			
			int totalDmg =
				weaponPower +
				attackerStrength -
				defenderDefense;

			string display = $"Using { weapon.NAME.Value }, Damage: { totalDmg }";

			indentLevel++;

			MenuButton menuButton = MakeButton(display, null, MenuButton.TYPE.TEMPORARY);

			indentLevel--;
		}

		#endregion
	};
};
