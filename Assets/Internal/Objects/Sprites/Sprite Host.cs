using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


namespace Vanguards
{
	public class SpriteHost : MonoBehaviour
	{
		SpriteRenderer spriteRenderer;

		#region Refresh

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		#endregion

		#region GrayScale / Action Used Management

		public bool IsGrayScale
		{
			get => spriteRenderer.material.GetInt("_GrayScale") == 1;
			set => spriteRenderer.material.SetInt("_GrayScale", value ? 1 : 0);
		}

		#endregion

		public void SetOffset(int newOffset)
		{
			spriteSetOffset = newOffset;
		}

		[SerializeField, Range(1, 32), Tooltip("Amount of sprites in an animation clip")]
		int spriteSetSize = 4;

		[SerializeField, Range(0, 32), Tooltip("Animation Clip Offset")]
		int spriteSetOffset = 0;

		[SerializeField, Range(0, 32), Tooltip("Framerate of Animation Clip")]
		float frameRate = 16;

		[SerializeField]
		Sprite[] sprites;

		int currentSprite = 0;
		float timeElapsed = 0;

		private void Update()
		{
			if (timeElapsed > 1 / frameRate)
			{
				spriteRenderer.sprite =
					sprites[
						spriteSetOffset * spriteSetSize +
						currentSprite % spriteSetSize];

				timeElapsed -= 1 / frameRate;

				currentSprite++;
			};

			timeElapsed += Time.deltaTime;
		}
	};
};
