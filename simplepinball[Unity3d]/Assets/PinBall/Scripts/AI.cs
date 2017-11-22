using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class AI : MonoBehaviour {

		static private AI _instance;
		static public AI Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<AI>();
				}
				return _instance;
			}
		}

		bool _springed = false;

		void OnDisable() {
			if (ControlManager.Instance) {
				ControlManager.Instance.BallTriggerEvent.RemoveListener(this);
			}
		}

		void OnEnable() {
			if (ControlManager.Instance) {
				StartCoroutine(Spring());
				ControlManager.Instance.BallTriggerEvent.AddListener(this, TriggerEvent);
			}
		}

		static System.Random rand = new System.Random();

		void TriggerEvent(ControlManager.TriggerEvent trigger) {

			switch (trigger) {
				case ControlManager.TriggerEvent.BallNearestSpring:
					if (!_springed) {
						StartCoroutine(Spring());
					}
					break;
				case ControlManager.TriggerEvent.BallNearestLeftFlipper:
					StartCoroutine(LeftFlipper());
					break;
				case ControlManager.TriggerEvent.BallNearestRightFlipper:
					StartCoroutine(RightFlipper());
					break;
				default:
					break;
			}
		}

		void Awake() {
			Refresh();
		}

		IEnumerator LeftFlipper() {
			yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
			ControlManager.Instance.LeftFlipperChangeState.Invoke(true);
			yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
			ControlManager.Instance.LeftFlipperChangeState.Invoke(false);
		}

		public void Refresh() {
			enabled = PlayerPrefs.GetFloat("Options.AI", 0f) > 0.5f;
		}

		IEnumerator RightFlipper() {
			yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
			ControlManager.Instance.RightFlipperChangeState.Invoke(true);
			yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
			ControlManager.Instance.RightFlipperChangeState.Invoke(false);
		}

		IEnumerator Spring() {
			_springed = true;
			float targetPower = Random.Range(0.6f, 1.0f);
			float power = 0f;
			float speed = 0.5f;
			while (power < targetPower) {
				yield return null;
				power = Mathf.MoveTowards(power, targetPower, Time.deltaTime * speed);
				ControlManager.Instance.SpringChangeState.Invoke(power);
			}
			yield return null;
			ControlManager.Instance.SpringFire.Invoke(targetPower);
			_springed = false;
		}
	}
}
