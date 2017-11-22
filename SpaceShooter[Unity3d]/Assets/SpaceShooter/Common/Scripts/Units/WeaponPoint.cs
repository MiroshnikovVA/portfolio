using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	public class WeaponPoint : MonoBehaviour {

		public GameObject WeaponPrefab;

		GameObject _object;

		private void OnEnable() {
			_object = Instantiate<GameObject>(WeaponPrefab);
			_object.transform.SetParent(transform);
			_object.transform.localPosition = Vector3.zero;
			_object.transform.localRotation = Quaternion.identity;
			_object.transform.localScale = Vector3.one;
		}

		private void OnDisable() {
			Destroy(_object);
		}
	}
}
