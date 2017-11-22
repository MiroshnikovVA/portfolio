using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace PinBall {
	public class DownMoveAnimation : MonoBehaviour {

		public Transform[] AnimatedObjects;
		public float OffSet = 100f;
		public float Velocity = 10f;

		float _maxY;
		float _minY;

		public void OnValidate() {
			if (AnimatedObjects.Length == 0) {
				Debug.LogError("AnimatedObjects Length is 0", this);
			}
		}

		void Awake() {
			_maxY = AnimatedObjects.Max(q => q.position.y) + OffSet;
			_minY = AnimatedObjects.Min(q => q.position.y) - OffSet;
		}

		void Update() {
			for (int i = 0; i < AnimatedObjects.Length; i++) {
				var obj = AnimatedObjects[i];
				obj.transform.position += Vector3.down * Velocity * Time.deltaTime;
				if (obj.position.y < _minY) {
					var deltaY = _minY - obj.position.y;
					obj.position = new Vector3(obj.position.x, _maxY - deltaY, obj.position.z);
				}
			}
		}
	}
}
