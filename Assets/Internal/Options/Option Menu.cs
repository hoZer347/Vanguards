using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Vanguards
{
	public delegate void OptionCallback();

	public class OptionMenu : MonoBehaviour
	{
		static public OptionMenu main = null;

		[SerializeField]
		GameObject optionTemplate;

		#region Refresh

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			main = this;

			Clear();
		}

		#endregion

		static private void AddOption<_State>(string displayName, _State state)
			where _State : St_Mp_Option
		{
			if (GameObject.Find(typeof(_State).Name) != null)
				return;

			GameObject option =
				Instantiate(
					main.optionTemplate,
					main.transform);

			option.name = displayName;

			foreach (Transform child in main.transform)
				option.transform.position +=
					Vector3.up *
					child.GetComponent<RectTransform>().rect.height;

			TextMeshProUGUI textMesh = option.GetComponentInChildren<TextMeshProUGUI>();
			textMesh.SetText(displayName, true);

			option.GetComponent<Button>().
				onClick.AddListener(
					() =>
					{
						State.SetState(state);
						Clear();
					});
		}

		static public void EnableOptions(Unit unit)
		{
			AddOption("Wait", new Op_Wait(unit));
			
			foreach (Consumable consumable in unit.GetComponentsInChildren<Consumable>())
				AddOption("Consumable: ", new Op_Consumable(unit));

			if (unit.equipped != null)
				AddOption("Unequip: " + unit.equipped.NAME.Value, new Op_Equip(unit, (Equippable)null));

			foreach (Equippable equippable in unit.GetComponentsInChildren<Equippable>())
				if (unit.equipped != equippable)
					AddOption("Equip: " + equippable.NAME.Value, new Op_Equip(unit, equippable));

			if (unit.equipped is Staff staff)
				AddOption("Staff: " + unit.equipped.NAME.Value, new Op_Staff(unit, staff));

			if (unit.equipped is Weapon weapon)
				AddOption("Attack: " + unit.equipped.NAME.Value, new Op_Attack(unit, weapon));	
		}

		static public void Clear()
		{
			if (main != null)
				foreach (Transform child in main.transform)
					//child.gameObject.SetActive(false);
					Destroy(child.gameObject);
		}
	};
};
