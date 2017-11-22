using UnityEngine;

namespace SpaceCrabDevelopment.Tools.UI {
	public class NotificationsCanvas : MonoBehaviour {

		static NotificationsCanvas _instance;
		public static NotificationsCanvas Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<NotificationsCanvas>();
				}
				return _instance;
			}
		}
	}
}
