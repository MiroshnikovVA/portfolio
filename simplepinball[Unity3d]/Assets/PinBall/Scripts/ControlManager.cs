using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class ControlManager : MonoBehaviour {

		static private ControlManager _instance;
		static public ControlManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<ControlManager>();
				}
				return _instance;
			}
		}

		public PinBall.SafeEvent<bool> LeftFlipperChangeState = new SafeEvent<bool>();
		public PinBall.SafeEvent<bool> RightFlipperChangeState = new SafeEvent<bool>();
		public PinBall.SafeEvent<float> SpringChangeState = new SafeEvent<float>();
		public PinBall.SafeEvent<float> SpringFire = new SafeEvent<float>();
		
		public PinBall.SafeEvent BallRespawned = new SafeEvent();

		public PinBall.SafeEvent<TriggerEvent> BallTriggerEvent = new SafeEvent<TriggerEvent>();

		public enum TriggerEvent {
			BallNearestSpring,
			BallNearestLeftFlipper,
			BallNearestRightFlipper
		}
	}
}
