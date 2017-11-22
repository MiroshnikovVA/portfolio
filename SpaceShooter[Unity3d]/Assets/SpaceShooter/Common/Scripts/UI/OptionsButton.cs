using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI {

	[RequireComponent(typeof(Button))]
	public class OptionsButton : MonoBehaviour {
		void Start() {
			GetComponent<Button>().onClick.AddListener(SpaceCrabDevelopment.Tools.UIManager.Instance.CreateAudioOptionsWindow);
		}
	}
}
