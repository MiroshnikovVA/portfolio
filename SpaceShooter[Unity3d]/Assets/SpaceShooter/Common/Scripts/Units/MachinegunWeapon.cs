using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	public class MachinegunWeapon : BaseWeapon {

		public GameObject ProjectilePrefab;
		public GameObject ImpactPrefab;
		public GameObject Muzzle;
		public GameObject FireEffect;
		public float Cooldown = 0.1f;
		public float MuzzleEffectTime = 0.1f;
		public float RandomCooldown = 0f;
		public AudioClip FireSound;
		public float ImpactLivesTime = 0.5f;
		public int Damage = 3;
		public int ProjectileCount = 1;
		public float RandomAngle = 1f;
		public float ProjectileRandomAnglePerSceonds = 0f;

		private bool _fireStarted;
		private Coroutine _fireCoroutine;
		private ObjectPool<Projectile> _projectiles;
		private ObjectPool<Explosion> _explosions;
		private bool _firing;
		private float _nextFireTime;

		protected class Projectile : MonoBehaviour {

		}

		protected class Explosion : MonoBehaviour {
			public float EndLivesTime;
		}

		private void OnValidate() {
			if (!ProjectilePrefab) Debug.LogError("ProjectilePrefab is null", this);
			if (!Muzzle) Debug.LogError("Muzzle is null", this);
			if (!ImpactPrefab) Debug.LogError("ImpactPrefab is null", this);
			if (!FireSound) Debug.LogError("FireSound is null", this);
		}

		public override void StartFire() {
			if (_firing) return;
			if (!isActiveAndEnabled) return;
			_firing = true;
			_fireCoroutine = StartCoroutine(FireCoroutine());
		}

		public override void EndFire() {
			_firing = false;
			if (!isActiveAndEnabled) return;
			if (FireEffect) FireEffect.SetActive(false);
			Muzzle.SetActive(false);
			StopCoroutine(_fireCoroutine);
		}

		protected virtual IEnumerator FireCoroutine() {
			if (Time.time<_nextFireTime) yield return new WaitForSeconds(_nextFireTime - Time.time);
			if (FireEffect) FireEffect.SetActive(true);
			while (true) {
				Muzzle.SetActive(true);
				for (int i=0; i< ProjectileCount; i++) {
					var proj = _projectiles.GetObject();
					proj.transform.Rotate(0f, UnityEngine.Random.Range(-RandomAngle, RandomAngle), 0f);
				}
				SpaceCrabDevelopment.Tools.SoundManager.Play(FireSound);
				var randomCooldown = UnityEngine.Random.Range(0f, RandomCooldown);
				_nextFireTime = Time.time + MuzzleEffectTime + Cooldown + randomCooldown;
				yield return new WaitForSeconds(MuzzleEffectTime);
				Muzzle.SetActive(false);
				yield return new WaitForSeconds(Cooldown + randomCooldown);
			}
		}

		private void OnEnable() {
			_projectiles = new ObjectPool<Projectile>(this.transform, ProjectilePrefab);
			_explosions = new ObjectPool<Explosion>(this.transform, ImpactPrefab);
			Muzzle.SetActive(false);
		}

		private void OnDisable() {
			_projectiles.Destroy();
			_projectiles = null;
			_explosions.Destroy();
			_explosions = null;
		}

		void Update() {
			int i = 0;
			while (i < _projectiles.ActiveObjects.Count) {
				var bullet = _projectiles.ActiveObjects[i];
				if (bullet.transform.position.z > 100f) {
					_projectiles.ReturnObject(bullet);
				}
				else {
					if (ProjectileRandomAnglePerSceonds>0) {
						bullet.transform.Rotate(
							0f,
							UnityEngine.Random.Range(-ProjectileRandomAnglePerSceonds, ProjectileRandomAnglePerSceonds) * Time.deltaTime,
							0f
						);
					}
					var delta = bullet.transform.forward * Time.deltaTime * 50f;
					bullet.transform.position += delta;
					RaycastHit hit;
					if (Physics.Raycast(bullet.transform.position, delta, out hit, delta.magnitude)) {
						var damageable = hit.collider.GetComponentInParent<Damageable>();
						if (damageable) damageable.Damage(Damage);
						var explosion = _explosions.GetObject();
						explosion.EndLivesTime = Time.time + ImpactLivesTime;
						explosion.transform.position = hit.point;
						_projectiles.ReturnObject(bullet);
					}
					else {
						i++;
					}
				}
			}

			i = 0;
			while (i < _explosions.ActiveObjects.Count) {
				var explosion = _explosions.ActiveObjects[i];
				if (Time.time > explosion.EndLivesTime) {
					_explosions.ReturnObject(explosion);
				} else {
					i++;
				}
			}
		}
	}
}
