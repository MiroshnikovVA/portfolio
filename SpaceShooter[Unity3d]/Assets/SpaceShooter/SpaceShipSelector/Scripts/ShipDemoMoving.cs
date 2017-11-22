using System.Collections;
using UnityEngine;
using System;

namespace SpaceShooter.SpaceShipSelector {
	public class ShipDemoMoving : MonoBehaviour {

		public Transform Ship;
		int _directionIndex = 0;
		Quaternion[] _angleDirections;
		Vector3[] _targetpoistions;

		private void OnValidate() {
			if (!Ship) Debug.LogError("Ship is null", this);
		}

		void Start() {
			_angleDirections = new Quaternion[4];
			_angleDirections[0] = Quaternion.Euler(-90, 27f, 0f);
			_angleDirections[1] = Quaternion.Euler(-90, 0f, 0f);
			_angleDirections[2] = Quaternion.Euler(-90, -27f, 0f);
			_angleDirections[3] = Quaternion.Euler(-90, 0f, 0f);

			var startPos = Ship.position;
			_targetpoistions = new Vector3[4];
			_targetpoistions[0] = new Vector3(startPos.x - 0.6f, startPos.y, startPos.z);
			_targetpoistions[1] = new Vector3(startPos.x - 0.6f, startPos.y, startPos.z);
			_targetpoistions[2] = new Vector3(startPos.x + 0.6f, startPos.y, startPos.z);
			_targetpoistions[3] = new Vector3(startPos.x + 0.6f, startPos.y, startPos.z);
			StartCoroutine(ChangeDirection());
		}

		private IEnumerator ChangeDirection() {
			while (true) {
				yield return new WaitForSeconds(3f);
				_directionIndex = (_directionIndex + 1) % _angleDirections.Length;
			}
		}

		void Update() {
			if (Math.Abs(Ship.rotation.eulerAngles.y) > 15f) Ship.position = Vector3.Slerp(Ship.position, _targetpoistions[_directionIndex], Time.deltaTime * 0.55f);
			Ship.rotation = Quaternion.Slerp(Ship.rotation, _angleDirections[_directionIndex], Time.deltaTime * 1.45f);
		}
	}
}
