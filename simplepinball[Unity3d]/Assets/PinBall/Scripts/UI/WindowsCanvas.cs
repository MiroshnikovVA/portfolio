using UnityEngine;
using System.Collections;

namespace PinBall.UI {
	public class WindowsCanvas : MonoBehaviour {

		static WindowsCanvas _instance;
		public static WindowsCanvas Instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<WindowsCanvas>();
				}
				return _instance;
			}
		}
	}
}
