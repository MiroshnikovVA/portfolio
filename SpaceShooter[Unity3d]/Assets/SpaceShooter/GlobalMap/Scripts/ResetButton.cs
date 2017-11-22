using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI {

	[RequireComponent(typeof(Button))]
	public class ResetButton : MonoBehaviour {
		void Start() {
			GetComponent<Button>().onClick.AddListener(() => {
				SpaceCrabDevelopment.Tools.UIManager.Instance.CreateDialogWindow(
					"Reset all data",
					"Are you sure you want to delete all data?",
					() => {
						Model.GameState.Instance.Reset();
						UnityEngine.SceneManagement.SceneManager.LoadScene(0);
					},
					() => { },
					() => { }
				);
			});
		}

	}
}
