using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PinBall.UI {
	public class DialogWindow : MonoBehaviour {

		public Text TextCaption;
		public Text TextMessage;
		public Button ButtonOk;
		public Button ButtonCancel;
		System.Action okCallback;
		System.Action updateCallback;
		System.Action cancelCallback;

		public void Init(string caption, string message, System.Action okCallback, System.Action updateCallback, System.Action cancelCallback) {
			TextCaption.text = caption;
			TextMessage.text = message;
			ButtonOk.onClick.AddListener(Ok);
			ButtonCancel.onClick.AddListener(Cancel);
			this.okCallback = okCallback;
			this.updateCallback = updateCallback;
			this.cancelCallback = cancelCallback;
		}

		void TryCall(System.Action action) {
			if (action != null)
				action();
		}

		void Ok() {
			Destroy(gameObject);
			TryCall(okCallback);
		}

		void Update() {
			TryCall(updateCallback);
		}

		void Cancel() {
			Destroy(gameObject);
			TryCall(cancelCallback);
		}
	}
}
