using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
			=> asyncOperation = SceneManager.LoadSceneAsync("Test Scene");		

		override public void OnUpdate()
		{
			if (asyncOperation.isDone)
			{
				StateMachine.stateStack.Clear();
				SetState(new St_Mp_InitialState());
			};
		}
	}
};
