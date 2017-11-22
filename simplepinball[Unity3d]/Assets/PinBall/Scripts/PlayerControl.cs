using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class PlayerControl : MonoBehaviour {

		List<ControlComponent> _controllers = new List<ControlComponent>();

		void Start() {
			_controllers.Add(new Flipper());
			_controllers.Add(new Spring());
			_controllers.Add(new Menu());
		}

		void Update() {
			foreach (var controller in _controllers) {
				controller.Update();
			}
		}

		abstract class ControlComponent {
			public abstract void Update();
		}

		class Flipper : ControlComponent {

			public Flipper() {
				Input.multiTouchEnabled = true;
			}

			bool _leftFlipper = true;
			bool _rightFlipper = true;


			void changeFromX(float posX, ref bool leftFlipper, ref bool rightFlipper) {
				if (posX < Screen.width * 0.1) {
					//not action
				}
				else if (posX < Screen.width * 0.3) {
					leftFlipper = true;
				}
				else if (posX < Screen.width * 0.7) {
					leftFlipper = true;
					rightFlipper = true;
				}
				else if (posX < Screen.width * 0.9) {
					rightFlipper = true;
				}
				else {
					//not action
				}
			}

			public override void Update() {
				bool leftFlipper = false;
				bool rightFlipper = false;

				for (int i=0; i<Input.touchCount; i++) {
					var touch = Input.GetTouch(i);
					var posX = touch.position.x;
					changeFromX(posX, ref leftFlipper, ref rightFlipper);
				}

				if (Input.GetMouseButton(0)) {
					changeFromX(Input.mousePosition.x, ref leftFlipper, ref rightFlipper);
				}

				if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
					leftFlipper = true;
				}
				if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
					rightFlipper = true;
				}

				if (_leftFlipper != leftFlipper) {
					_leftFlipper = leftFlipper;
					ControlManager.Instance.LeftFlipperChangeState.Invoke(leftFlipper);
				}

				if (_rightFlipper != rightFlipper) {
					_rightFlipper = rightFlipper;
					ControlManager.Instance.RightFlipperChangeState.Invoke(rightFlipper);
				}
			}
		}

		class Spring : ControlComponent {

			float _power = 0f;
			bool _startKeyboard = false;
			float _keyboardPower = 0;
			bool _downStart = true;
			Vector3 _startMousePosition;

			void KeyboardControl() {
				_startKeyboard = true;
				if (Input.GetKey(KeyCode.DownArrow)) _keyboardPower = Mathf.MoveTowards(_keyboardPower, 1f, Time.deltaTime * 0.5f);
				if (Input.GetKey(KeyCode.UpArrow)) _keyboardPower = Mathf.MoveTowards(_keyboardPower, 0f, Time.deltaTime * 0.5f);
				SetPower(_keyboardPower);
			}

			void SetPower(float power) {
				if (_power != power) {
					_power = power;
					ControlManager.Instance.SpringChangeState.Invoke(power);
				}
			}

			void Fire(float power) {
				ControlManager.Instance.SpringFire.Invoke(power);
			}

			void OnMouseDown() {
				_startMousePosition = Input.mousePosition;
				if (_startMousePosition.y > 10) {
					_downStart = true;
				}
			}

			void OnDownMouseMove() {
				float power = Mathf.Clamp((_startMousePosition.y - Input.mousePosition.y) / _startMousePosition.y, 0f, 1f);
				SetPower(power);
				if (Input.GetMouseButtonUp(0)) {
					_downStart = false;
					Fire(power);
				}
			}

			void TryKeyboardFire() {
				if (_startKeyboard) {
					SetPower(_keyboardPower);
					Fire(_keyboardPower);
					_keyboardPower = 0f;
					_startKeyboard = false;
				}
			}

			public override void Update() {
				if (Input.GetMouseButtonDown(0)) {
					OnMouseDown();
				}
				else if (_downStart) {
					OnDownMouseMove();
				}

				if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) {
					KeyboardControl();
				}
				else TryKeyboardFire();
			}


		}

		class Menu : ControlComponent {
			public override void Update() {
				UIManager.Instance.TryLeaveGame();
				if (Input.GetKeyDown(KeyCode.F12)) {
					ScoreManager.Instance.AddScore(50);
				}
			}
		}
	}
}
