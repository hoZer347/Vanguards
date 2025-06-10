using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Vanguards
{
	public class MenuButton :
		MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler
	{
		public enum TYPE
		{
			STATIC,
			DYNAMIC,
			TEMPORARY,
			ALL,
		};

		public TYPE type = TYPE.STATIC;

		public Action onHover	= () => { };
		public Action onUnHover = () => { };

		public void OnPointerEnter(PointerEventData eData)
			=> onHover();

		public void OnPointerExit(PointerEventData eData)
			=> onUnHover();
	};
};
