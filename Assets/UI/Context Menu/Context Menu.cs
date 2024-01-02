using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vanguards
{
	internal class _ContextMenu : MonoBehaviour
	{
		static internal _ContextMenu main;

		static internal Unit loadedUnit;

		[SerializeField]
		internal GameObject defaultDisplaySlotTemplate;

		static internal void Clear()
		{
			currXDisplacement = 0;
			currYDisplacement = 0;

			foreach (CanvasRenderer canvasRenderer in main.GetComponentsInChildren<CanvasRenderer>())
				Destroy(canvasRenderer.transform.gameObject);
		}

		static float currXDisplacement = 0.0f;
		static float currYDisplacement = 0.0f;

		static internal Vector2 currDisplacement => new Vector2(currXDisplacement, currYDisplacement);

		static internal void LoadAttributes(Unit unit, bool clear = true)
		{
			if (loadedUnit == unit) return;
			loadedUnit = unit;

			if (clear) Clear();

			LoadAttribute("HP",  unit.GetAttribute("HP"));
			LoadAttribute("STR", unit.GetAttribute("STR"));
			LoadAttribute("MAG", unit.GetAttribute("MAG"));
			LoadAttribute("SPD", unit.GetAttribute("SPD"));
			LoadAttribute("SKL", unit.GetAttribute("SKL"));
			LoadAttribute("LCK", unit.GetAttribute("LCK"));
			LoadAttribute("DEF", unit.GetAttribute("DEF"));
			LoadAttribute("RES", unit.GetAttribute("RES"));

			currYDisplacement = 0.0f;
		}

		static internal void LoadAttribute(string name, Attribute attribute)
		{
			GameObject gameObject = Instantiate(main.defaultDisplaySlotTemplate, main.transform);

			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = Vector3.up * currYDisplacement + Vector3.right * currXDisplacement;
			currYDisplacement -= rectTransform.rect.height;

			TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
			text.text = name + ": " + attribute.Value;
		}

		static internal void LoadUnitOptions(Unit unit, bool clear = true)
		{
			if (clear) Clear();

			currYDisplacement = 0.0f;

			foreach (Option option in unit.GetOptions())
				LoadOption(option);
		}

		static internal void LoadOption(Option option, string name = "")
		{
			GameObject gameObject = Instantiate(main.defaultDisplaySlotTemplate, main.transform);
			gameObject.name = option.GetType().ToString().Split('.').Last();

			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = Vector3.up * currYDisplacement;
			currYDisplacement -= rectTransform.rect.height;

			Button button = gameObject.GetComponent<Button>();
			button.onClick.AddListener(delegate () { MapState.Current = option; });

			TextMeshProUGUI textMesh = button.GetComponentInChildren<TextMeshProUGUI>();
			if (name == "") textMesh.text = gameObject.name;
			else textMesh.text = name;
		}

		static internal void LoadAttackUI(Unit attacker, Unit defender)
		{
			Clear();

			LoadAttribute("HP",  attacker.GetAttribute("HP"));
			LoadAttribute("STR", attacker.GetAttribute("STR"));
			LoadAttribute("MAG", attacker.GetAttribute("MAG"));
			LoadAttribute("SPD", attacker.GetAttribute("SPD"));
			LoadAttribute("SKL", attacker.GetAttribute("SKL"));
			LoadAttribute("LCK", attacker.GetAttribute("LCK"));
			LoadAttribute("DEF", attacker.GetAttribute("DEF"));
			LoadAttribute("RES", attacker.GetAttribute("RES"));

			currYDisplacement -= main.defaultDisplaySlotTemplate.GetComponent<RectTransform>().rect.height;
			LoadOption(new AttackAnimation(defender), "Attack!");

			currXDisplacement = main.defaultDisplaySlotTemplate.GetComponent<RectTransform>().rect.width;
			currYDisplacement = 0.0f;

			LoadAttribute("HP",  defender.GetAttribute("HP"));
			LoadAttribute("STR", defender.GetAttribute("STR"));
			LoadAttribute("MAG", defender.GetAttribute("MAG"));
			LoadAttribute("SPD", defender.GetAttribute("SPD"));
			LoadAttribute("SKL", defender.GetAttribute("SKL"));
			LoadAttribute("LCK", defender.GetAttribute("LCK"));
			LoadAttribute("DEF", defender.GetAttribute("DEF"));
			LoadAttribute("RES", defender.GetAttribute("RES"));

			currXDisplacement = 0.0f;
			currYDisplacement = 0.0f;
		}

		private void Start() { main = this; }
		private void Awake() { main = this; }
	};
};
