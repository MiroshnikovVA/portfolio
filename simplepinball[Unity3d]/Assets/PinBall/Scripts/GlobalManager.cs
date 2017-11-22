using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class GlobalManager : MonoBehaviour {
		public void Awake() {
			DontDestroyOnLoad(gameObject);
		}
	}
}
