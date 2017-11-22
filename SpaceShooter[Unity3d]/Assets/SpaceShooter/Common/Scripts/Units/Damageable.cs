using SpaceCrabDevelopment.HealthBar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {

	[RequireComponent(typeof(HealthBar))]
	public class Damageable : MonoBehaviour {

		public int Health = 100;
		public int MaxHealth = 100;
		public AudioClip[] BulletsImpacts;

		HealthBar _healthBar;
		private float _nextSoundEffect;

		public void Damage(int value) {
			if (Time.time > _nextSoundEffect) {
				_nextSoundEffect = Time.time + 0.5f;
				SpaceCrabDevelopment.Tools.SoundManager.PlayRandom(BulletsImpacts);
			}
			Health -= value;
			if (Health < 0) {
				gameObject.SetActive(false);
			}
			else {
				_healthBar.OnHealthChanged(Health / (float)MaxHealth);
			}
		}

		void OnEnable() {
			_healthBar = GetComponent<HealthBar>();
			Health = MaxHealth;
			_healthBar.OnHealthChanged(1f);
		}

		public void OnMouseEnter() {
			_healthBar.IsActive = true;
		}

		public void OnMouseExit() {
			_healthBar.IsActive = false;
		}
	}
}
