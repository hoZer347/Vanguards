using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;


namespace Vanguards
{
	public class MainMenuButton : MonoBehaviour
	{
		Button button;

		private void Start()
		{
			button = GetComponent<Button>();
			button.onClick.AddListener(
				() =>
				{
					ScreenFader.FadeToBlack(new MapLoadState());
				});
		}
	};

	public class MapLoadState : State
	{
		AsyncOperation asyncOperation;

		public override void OnEnter()
		{
			asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Assets/Scenes/Test Scene.unity");
		}

		override public void OnUpdate()
		{
			if (asyncOperation.isDone)
				SetState(new St_Mp_InitialState());
		}
	}
};
