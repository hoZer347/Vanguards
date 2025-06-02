using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Vanguards
{
	public class OptionMenu : MonoBehaviour
	{
		[SerializeField]
		OptionButton optionButtonTemplate;

		[SerializeField]
		float indentSize = 25.0f;

		float yTranslation = 0.0f;
		int indentLevel = 0;

		public void SetOptions(Unit unit)
		{
			HandleItemCategory<Weapon, Op_Attack>(unit);
			HandleItemCategory<Staff, Op_Staff>(unit);
			HandleItemCategory<Consumable, Op_Consumable>(unit);

			MakeButton("General: ", null);
			indentLevel++;
			MakeButton("Wait", () => State.SetState(new Op_Wait(unit)));

			indentLevel = 0;
			yTranslation = 0;
		}

		private void HandleItemCategory<_ItemType, _OptionType>(Unit unit)
			where _ItemType : Item
			where _OptionType : St_Option
		{
			_ItemType[] items = unit.GetComponentsInChildren<_ItemType>();
			Array.Reverse(items);

			if (items.Length > 0)
			{
				MakeButton($"{typeof(_ItemType).Name}s: ", null);
				indentLevel++;

				foreach (_ItemType item in items)
					if (item is Equippable)
					{
						string displayPrefix = "";
						Action onClick;

						if (item == unit.equipped)
						{
							displayPrefix += "Use:   ";
							onClick =
								() =>
								{
									_OptionType option = (_OptionType)Activator.CreateInstance(
										typeof(_OptionType),
										unit,
										item);

									State.SetState(option);
								};
						}
						else
						{
							displayPrefix += "Equip: ";
							onClick = () => State.SetState(new Op_Equip(unit, item as Equippable));
						};

						MakeButton(displayPrefix + item.NAME.Value, onClick);
					}
					else
						MakeButton("Use:   " + item.NAME.Value, () =>
						{
							_OptionType option = (_OptionType)Activator.CreateInstance(
								typeof(_OptionType),
								unit,
								item);

							State.SetState(option);
						});

				indentLevel--;
			};
		}

		private void MakeButton(
			string name,
			Action onClick)
		{
			float effectiveIndent = indentLevel * indentSize;

			GameObject go = Instantiate(optionButtonTemplate.gameObject, transform);
			go.name = name;

			RectTransform rectTransform = go.GetComponent<RectTransform>();
			rectTransform.Translate(new Vector3(effectiveIndent, yTranslation, 0));

			TextMeshProUGUI textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
			textMesh.text = name;
			textMesh.ForceMeshUpdate();

			Button button = go.GetComponentInChildren<Button>();

			if (onClick != null)
				button.onClick.AddListener(() => onClick());
			else button.enabled = false;

			yTranslation -= rectTransform.rect.height;
		}

		public void ClearOptions()
		{
			OptionButton[] optionButtons = GetComponentsInChildren<OptionButton>();

			foreach (OptionButton optionButton in optionButtons)
				Destroy(optionButton.gameObject);
		}
	};
};
