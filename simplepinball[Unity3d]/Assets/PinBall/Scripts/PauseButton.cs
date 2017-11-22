using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall {

	[RequireComponent(typeof(Button))]
	public class PauseButton : MonoBehaviour {

		void Start() {
			var b = GetComponent<Button>();
			b.onClick.AddListener(Click);
		}

		void Click() {
			UIManager.Instance.CreateOptionsWindow();
		}

	}
}
