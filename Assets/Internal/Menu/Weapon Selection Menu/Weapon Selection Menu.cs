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
							ClearOptions();
							receiver.HP.RmvModifier("Damage Preview");
							State.SetState(new An_Attack(attacker, receiver));
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

							receiver.HP.SetModifier("Damage Preview",
								(ref int hp) =>
								{
									hp -= weapon.CalculateDamage(attacker, receiver);
								});
							receiver.GetComponentInChildren<HealthBar>().Refresh();

							indentLevel--;
						};

					optionButton.onUnHover =
						() =>
						{
							receiver.HP.RmvModifier("Damage Preview");
							receiver.GetComponentInChildren<HealthBar>().Refresh();
							ClearOptions(MenuButton.TYPE.TEMPORARY);
						};
				};

			indentLevel--;
		}

		private void DisplayWeaponOption(
			Unit attacker,
			Unit receiver,
			Weapon weapon)
		{
			int defenderDefense = receiver.DEF.Value;
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
