using UnityEngine;
using System.Collections;
using SpaceCrabDevelopment.Tools.UI;

namespace SpaceCrabDevelopment.Tools {
	public class UIManager : MonoBehaviour {
		public static UIManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<UIManager>();
					if (!_instance) {
						var gameObject = new GameObject();
						gameObject.name = "UIManager";
						_instance = gameObject.AddComponent<UIManager>();
					}
				}
				return _instance;
			}
		}

		#region prefab
		static UIManager _instance;
		void LoadAsync<TWindow>(string resource, MonoBehaviour parent, System.Action<TWindow> callback) where TWindow : MonoBehaviour {
			StartCoroutine(InnerLoad<TWindow>(
				resource,
				(prefab) => {
					var obj = Instantiate(prefab, parent.transform) as TWindow;
					obj.transform.position = parent.transform.position;
					obj.transform.localScale = Vector3.one;
					Canvas.ForceUpdateCanvases();
					callback(obj);
				}
			));
		}

		IEnumerator InnerLoad<TWindow>(string resource, System.Action<TWindow> callback) where TWindow : MonoBehaviour {
			var req = Resources.LoadAsync<TWindow>(resource);
			while (!req.isDone && !req.asset) {
				yield return null;
			}
			callback(req.asset as TWindow);
		}
		#endregion

		public void CreateNotificationWindow(string caption, string message) {
			LoadAsync<NotificationWindow>(
				"Windows/NotificationWindow",
				NotificationsCanvas.Instance,
				(obj) => obj.Init(caption, message)
			);
			Debug.Log("Notificatio : " + caption + "\n" + message);
		}

		public void CreateDialogWindow(string caption, string message, System.Action okCallback, System.Action updateCallback, System.Action cancelCallback) {
			LoadAsync<DialogWindow>(
				"Windows/DialogWindow",
				NotificationsCanvas.Instance,
				(obj) => obj.Init(caption, message, okCallback, updateCallback, cancelCallback)
			);
		}

		public void CreateAudioOptionsWindow() {
			LoadAsync<AudioOptionsWindow>(
				"Windows/AudioOptionsWindow",
				WindowsCanvas.Instance,
				(obj) => obj.Init()
			);
		}


		bool LeaveDialoged = false;
		public void TryLeaveGame() {
			if (LeaveDialoged)
				return;
			if (Input.backButtonLeavesApp || Input.GetKeyDown(KeyCode.Escape)) {
				LeaveDialoged = true;
				var pauseDoubleBack = true;
				CreateDialogWindow("Leave game", "Do you wish to retire this game?",
					() => {
						LeaveDialoged = false;
						Application.Quit();
					},
					() => {
						var b = (Input.backButtonLeavesApp || Input.GetKeyDown(KeyCode.Escape));
						if (pauseDoubleBack) {
							pauseDoubleBack = b;
							return;
						}
						if (b) {
							//LeaveDialoged = false;
							Application.Quit();
						}
					},
					() => {
						LeaveDialoged = false;
					});
			}
		}
	}
}
