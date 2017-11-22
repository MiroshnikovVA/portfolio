using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	[RequireComponent(typeof(Rigidbody2D))]
	public class MaxSpeed : MonoBehaviour {

		public float Speed = 10f;
		Rigidbody2D _rigidbody2d;

		void Start() {
			_rigidbody2d = GetComponent<Rigidbody2D>();
		}

		void FixedUpdate() {
			if (_rigidbody2d.velocity.magnitude > Speed) {
				_rigidbody2d.velocity = _rigidbody2d.velocity.normalized * Speed;
			}
		}
	}
}
