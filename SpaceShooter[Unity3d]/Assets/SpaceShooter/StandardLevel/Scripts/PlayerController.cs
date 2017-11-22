using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace SpaceShooter.StandardLevel {

	public class PlayerController : MonoBehaviour {

		public Camera Camera;
		public Ship PlayerShip;

		private Quaternion _leftAngle;
		private Quaternion _rightAngle;
		private bool _singleShot;

		public Vector3 CurrentSpeed;

		private void OnValidate() {
			if (!PlayerShip) Debug.LogError("PlayerShip is null", this);
			if (!Camera) Debug.LogError("Camera is null", this);
		}


		void Start() {
			Model.GameState.Instance.LoadPlayerShipEquipments(PlayerShip);
			_leftAngle = Quaternion.Euler(0f, 0f, 25f);
			_rightAngle = Quaternion.Euler(0f, 0f, -25f);
			PlayerShip.IsDead
				.Where(isDead => isDead)
				.Subscribe(_ => {
					PlayerShip.gameObject.SetActive(false);
				});
		}

		void Update() {
			if (PlayerShip.IsDead.Value) return;

			if (Input.GetKeyDown(KeyCode.Space)) {
				PlayerShip.StartFire();
			}
			else if (Input.GetKeyUp(KeyCode.Space)) {
				PlayerShip.EndFire();
			}

			float vert = Input.GetAxis("Vertical");
			float turn = Input.GetAxis("Horizontal");
			var targetAngle = Quaternion.Euler(10f * vert, 0f, -25f * turn);
			PlayerShip.transform.rotation=Quaternion.Slerp(PlayerShip.transform.rotation, targetAngle, 10f*Time.deltaTime);
			var moveDirection = Vector3.forward * vert + Vector3.right * turn;
			var screenPoint = Camera.WorldToScreenPoint(PlayerShip.transform.position);

			if (screenPoint.x > Screen.width - 25) moveDirection += Vector3.left * 10f;
			if (screenPoint.x < 25) moveDirection += Vector3.right * 10f;

			if (screenPoint.y > Screen.height - 25) moveDirection -= Vector3.forward * 10f;
			if (screenPoint.y < 25) moveDirection -= Vector3.back * 2f;

			moveDirection = moveDirection.normalized;

			var kspeed = vert == 0f && turn == 0f ? 1.5f : 1f;

			CurrentSpeed = Vector3.Lerp(CurrentSpeed, moveDirection, kspeed * 10f * Time.deltaTime);
			PlayerShip.transform.position = Vector3.Lerp(
				PlayerShip.transform.position,
				new Vector3(PlayerShip.transform.position.x, 0f, PlayerShip.transform.position.z),
				Time.deltaTime * 10f);
			PlayerShip.CharacterController.Move(CurrentSpeed * Time.deltaTime * 10f);
			//PlayerShip.transform.position += CurrentSpeed * Time.deltaTime * 10f;


		}
	}
}
