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
		}

		#endregion

		static private void AddOption<T>()
			where T : St_Mp_Option, new()
		{
			GameObject option =
				Instantiate(
					main.optionTemplate,
					main.transform);

			foreach (Transform child in main.transform)
			{
				option.transform.position +=
					Vector3.up *
					child.GetComponent<RectTransform>().rect.height;
			};

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

		static public void GenerateOptions(Unit unit)
		{
			AddOption<Op_Wait>();
			AddOption<Op_Item>();
			AddOption<Op_Equip>();
			AddOption<Op_Attack>();
		}

		static public void Clear()
		{
			if (main != null)
				foreach (Transform child in main.transform)
					Destroy(child.gameObject);
		}
	};
};
