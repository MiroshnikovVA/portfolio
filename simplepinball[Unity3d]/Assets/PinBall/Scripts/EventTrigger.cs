using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {

	public class EventTrigger : MonoBehaviour {

		public LayerMask Layer;

		public ControlManager.TriggerEvent TriggerEvent;

		void OnTriggerEnter2D(Collider2D collision) {
			if (1<<collision.gameObject.layer == Layer.value) {
				ControlManager.Instance.BallTriggerEvent.Invoke(TriggerEvent);
			}
		}
	}
}
