using UnityEngine;
using System.Collections;

namespace SpaceCrabDevelopment.Examples {

	[RequireComponent(typeof(HealthBar.HealthBar))]
	public class Unit : MonoBehaviour {

		public int Health = 100;
		public int MaxHealth = 100;
		public int Fraction;

		HealthBar.HealthBar _healthBar;

		public void Awake() {
			_healthBar = GetComponent<HealthBar.HealthBar>();
			_healthBar.IsEnemy = IsEnemy();
		}

		public void Damage(int value) {
			Health -= value;
			if (Health < 0) {
				Die();
			}
			else {
				_healthBar.OnHealthChanged(Health / (float)MaxHealth);
			}
		}

		internal bool IsEnemy() {
			return Fraction != LevelConfig.Instance.PlayerFraction;
		}

		void Die() {
			Destroy(gameObject);
		}

		public void OnMouseDown() {
			Damage(5);
		}

		public void OnMouseEnter() {
			_healthBar.IsActive = true;
		}

		public void OnMouseExit() {
			_healthBar.IsActive = false;
		}
	}

}