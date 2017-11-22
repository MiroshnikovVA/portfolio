using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Damageable))]
	public class Asteroid : MonoBehaviour {

		public Damageable Damageable { get; private set; }
		public Vector3 Velocity { get; set; } 
		private Rigidbody _rigidbody;


		private void Awake() {
			_rigidbody = GetComponent<Rigidbody>();
			Damageable = GetComponent<Damageable>();
		}

		public void Kill() {
			Damageable.Damage(Damageable.MaxHealth);
			gameObject.SetActive(false);
		}

		void Update() {
			_rigidbody.MovePosition(transform.position += Velocity * Time.deltaTime);
		}

		private void OnCollisionEnter(Collision collision) {
			var ship = collision.collider.GetComponent<Ship>();
			if (ship) {
				ship.AsteroidCollision();
				Kill();
			}
		}
	}
}
