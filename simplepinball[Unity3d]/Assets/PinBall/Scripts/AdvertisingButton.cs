using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall {

	[RequireComponent(typeof(Button))]
	public class AdvertisingButton : MonoBehaviour {

		void Start() {
			var b = GetComponent<Button>();
			b.onClick.AddListener(Click);
		}

		void Click() {
			ScoreManager.Instance.AddScore(100);
			if (ScoreManager.Instance.Score >= LevelManager.Instance.ScoreToNextLevel) {
				var b = GetComponent<Button>();
				b.enabled = false;
			}
		}
	}
}
