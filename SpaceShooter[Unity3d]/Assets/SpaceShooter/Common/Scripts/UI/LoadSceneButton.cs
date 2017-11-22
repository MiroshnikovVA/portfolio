using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI {

	[RequireComponent(typeof(Button))]
	public class LoadSceneButton : MonoBehaviour {

		public string SceneName;

		private void OnValidate() {
			if (string.IsNullOrEmpty(SceneName)) Debug.LogError("SceneName is empty", this);
		}

		void Start() {
			GetComponent<Button>().onClick.AddListener(() => 
				UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName)
			);
		}
	}
}
