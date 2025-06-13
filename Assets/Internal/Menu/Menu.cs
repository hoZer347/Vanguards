using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Vanguards
{
	abstract public class Menu : MonoBehaviour
	{
		#region Core Functionality

		[SerializeField]
		protected MenuButton optionButtonTemplate;

		[SerializeField]
		protected float indentSize = 25.0f;
		protected int indentLevel = 0;

		protected float yTranslation = 0.0f;

		protected MenuButton MakeButton(
			string name,
			Action onClick,
			MenuButton.TYPE type = MenuButton.TYPE.STATIC)
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

			MenuButton optionButton = go.GetComponent<MenuButton>();
			optionButton.type = type;

			return optionButton;
		}

		public void ClearOptions(MenuButton.TYPE type = MenuButton.TYPE.ALL)
		{
			MenuButton[] optionButtons = GetComponentsInChildren<MenuButton>();

			foreach (MenuButton optionButton in optionButtons)
				if (type == optionButton.type ||
					type == MenuButton.TYPE.ALL)
				{
					Destroy(optionButton.gameObject);
					yTranslation += optionButton.GetComponent<RectTransform>().rect.height;
				};

			if (type == MenuButton.TYPE.ALL)
				yTranslation = 0;
		}

		#endregion

		#region Cell Handling

		public void DisplayCell(Cell cell)
		{

		}

		#endregion
	};
};
