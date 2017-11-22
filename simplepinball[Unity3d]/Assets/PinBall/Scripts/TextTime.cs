using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall {
	[RequireComponent(typeof(Text))]
	public class TextTime : MonoBehaviour {

		// Use this for initialization
		void Start() {

			GetComponent<Text>().text = TimeToString(Time.time);
		}

		string TimeToString(float t) {
			var s = (int)t;
			var m = s / 60;
			s = s % 60;
			var h = m / 60;
			m = m % 60;
			return h + ":" + m + ":" + s;
		}
	}
}
