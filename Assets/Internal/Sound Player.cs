using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vanguards
{
	public class SoundPlayer : MonoBehaviour
	{
		[SerializeField] int m_masterVolume;
		[SerializeField] int m_musicVolume;
		[SerializeField] int m_effectsVolume;

		public enum SoundType
		{
			Music,
			Effect,
		};


		void PlaySound(string fileName, SoundType soundType)
		{

		}
	};
};
