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

		static private void AddOption<T>()
			where T : St_Mp_Option, new()
		{
			if (GameObject.Find(typeof(T).Name) != null)
				return;

			GameObject option =
				Instantiate(
					main.optionTemplate,
					main.transform);

			option.name = typeof(T).Name;

			foreach (Transform child in main.transform)
				option.transform.position +=
					Vector3.up *
					child.GetComponent<RectTransform>().rect.height;

			TextMeshProUGUI textMesh = option.GetComponentInChildren<TextMeshProUGUI>();
			textMesh.SetText(typeof(T).Name, true);

			option.GetComponent<Button>().
				onClick.AddListener(
					() =>
					{
						State.SetState<T>();
						Clear();
					});
		}

		static public void EnableOptions(Unit unit)
		{
			//for (int i = 0; i < main.transform.childCount; i++)
			//	main.transform.GetChild(i).gameObject.SetActive(true);

			// TODO: Make this enable the options instead of create / destroy them

			AddOption<Op_Wait>();
			AddOption<Op_Item>();
			AddOption<Op_Equip>();
			AddOption<Op_Attack>();
			AddOption<Op_Staff>();
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
