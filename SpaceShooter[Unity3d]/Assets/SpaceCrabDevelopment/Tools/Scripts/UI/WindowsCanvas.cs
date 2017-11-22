using UnityEngine;
using System.Collections;

namespace SpaceCrabDevelopment.Tools.UI {
	public class WindowsCanvas : MonoBehaviour {

		static WindowsCanvas _instance;
		public static WindowsCanvas Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<WindowsCanvas>();
				}
				return _instance;
			}
		}
	}
}
