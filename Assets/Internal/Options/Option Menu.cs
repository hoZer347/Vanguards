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

		static private void AddOption<T>(string displayName, params dynamic[] args)
			where T : St_Mp_Option
		{
			if (GameObject.Find(typeof(T).Name) != null)
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
						State.SetState<T>(args);
						Clear();
					});
		}

		static public void EnableOptions(Unit unit)
		{
			AddOption<Op_Wait>("Wait", unit);
			
			foreach (Consumable consumable in unit.GetComponentsInChildren<Consumable>())
				AddOption<Op_Consumable>("Consumable: ", consumable);

			if (unit.equipped != null)
				AddOption<Op_Equip>("Unequip: " + unit.equipped.NAME.Value, (Equippable)null);

			foreach (Equippable equippable in unit.GetComponentsInChildren<Equippable>())
				if (unit.equipped != equippable)
					AddOption<Op_Equip>("Equip: " + equippable.NAME.Value, unit, equippable);

			if (unit.equipped is Staff staff)
				AddOption<Op_Staff>("Staff: " + unit.equipped.NAME.Value, staff);

			if (unit.equipped is Weapon weapon)
				AddOption<Op_Attack>("Attack: " + unit.equipped.NAME.Value, weapon);	
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
