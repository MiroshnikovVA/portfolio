using UnityEngine;
using System.Collections;

namespace PinBall.UI {
	public class NotificationsCanvas : MonoBehaviour {

		static NotificationsCanvas _instance;
		public static NotificationsCanvas Instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<NotificationsCanvas>();
				}
				return _instance;
			}
		}
	}
}
