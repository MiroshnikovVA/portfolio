using UnityEngine;
using UnityEngine.UI;

namespace SpaceCrabDevelopment.Tools.UI {
	public class NotificationWindow : MonoBehaviour {

		public Text TextCaption;
		public Text TextMessage;
		public Button ButtonClose;

		public void Init(string caption, string message) {
			TextCaption.text = caption;
			TextMessage.text = message;
			ButtonClose.onClick.AddListener(Close);
		}

		void Close() {
			Destroy(gameObject);
		}
	}
}
