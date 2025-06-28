using UnityEngine;


namespace Vanguards
{
	public class Spell : Item
	{
		public enum Element
		{
			Fire,
			Ice,
			Thunder,
			Wind,
			Holy,
			Dark
		};

		public Attribute<int> RNG;
		public Attribute<int> DAMAGE;
		public Attribute<Element> ELEMENT;
	};
};
