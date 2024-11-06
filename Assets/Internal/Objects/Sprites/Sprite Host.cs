using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Vanguards
{
	public class SpriteHost : MonoBehaviour
	{
		[SerializeField]
		Texture2D texture;

		SpriteRenderer spriteRenderer = null;

		#region Refresh

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			if (texture != null)
			{
				Object[] assets =
					AssetDatabase.LoadAllAssetRepresentationsAtPath(
						AssetDatabase.GetAssetPath(texture));

				sprites = new Sprite[assets.Length];
				for (int i = 0; i < assets.Length; i++)
					sprites[i] = assets[i] as Sprite;

#if UNITY_EDITOR
				EditorApplication.delayCall += () =>
#endif
				UpdateSpriteIndex();
			};
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

		void UpdateSpriteIndex()
			=> spriteRenderer.sprite = sprites[
				spriteSetOffset * spriteSetSize +
				currentSprite % spriteSetSize];

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
				UpdateSpriteIndex();

				timeElapsed -= 1 / frameRate;

				currentSprite++;
			};

			timeElapsed += Time.deltaTime;
		}
	};
};
