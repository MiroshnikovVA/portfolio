using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PinBall.UI {
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
