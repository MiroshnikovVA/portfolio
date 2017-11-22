using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace PinBall {
	public class FlipperController : MonoBehaviour {

		public bool LeftFlipper;
		public HingeJoint2D Motor;

		void OnValidate() {
			if (!Motor) Debug.LogError("Motor is null", this);
		}

		void Start() {
			if (LeftFlipper) {
				ControlManager.Instance.LeftFlipperChangeState.AddListener(this, (active) => Motor.useMotor = active);
			}
			else {
				ControlManager.Instance.RightFlipperChangeState.AddListener(this, (active) => Motor.useMotor = active);
			}
		}
	}
}
