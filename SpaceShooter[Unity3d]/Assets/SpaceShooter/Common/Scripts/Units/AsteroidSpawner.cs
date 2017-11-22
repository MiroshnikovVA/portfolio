using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace SpaceShooter {
	public class AsteroidSpawner : MonoBehaviour {

		public Asteroid[] AsteroidPrefabs;
		public GameObject ExplosionsPrefab;
		public float FrontLength = 4f;
		//public int MaxAsteroidsNumberInWave = 5;
		//public int MinAsteroidsNumberInWave = 3;
		//public int AsteroidsCount = 50;
		//public float WaveInterval = 5f;
		public float FlightDistance = 50f;
		public int AsteroidsRemainCount;
		public float ExplosionLivesTime = 2f;
		public Model.AsteroidSpawnPattern AsteroidSpawnPattern;

		public IReactiveProperty<float> Progress = new ReactiveProperty<float>(0f);
		public IReactiveProperty<int> Frags = new ReactiveProperty<int>(0);

		private ObjectPool<Asteroid> _asteroids;
		private ObjectPool<Explosion> _explosions;
		private Coroutine _coroutin;

		private void OnValidate() {
			if (!ExplosionsPrefab) Debug.Log("ExplosionsPrefab is null", this);
			if (AsteroidPrefabs.Length==0) Debug.Log("AsteroidPrefabs is empty array", this);
		}

		private void OnDrawGizmosSelected() {
			var delta = transform.right * FrontLength * 0.5f;
			var start = transform.position - delta;
			var finish = transform.position + delta;
			Gizmos.DrawLine(start, finish);
			var distance = transform.forward * FlightDistance;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(start + distance, finish + distance);
			Gizmos.DrawLine(start, start + distance);
			Gizmos.DrawLine(finish, finish + distance);
			Gizmos.color = Color.red;
			var c = (int)(FrontLength);
			for (int i=1; i<c; i++) {
				var arrowPoint0 = start + transform.right * i;
				var arrowPoint1 = arrowPoint0 + transform.forward * 1f;
				var arrowPoint2 = arrowPoint0 + transform.forward * 0.8f + transform.right * 0.2f;
				var arrowPoint3 = arrowPoint0 + transform.forward * 0.8f - transform.right * 0.2f;
				Gizmos.DrawLine(arrowPoint0, arrowPoint1);
				Gizmos.DrawLine(arrowPoint1, arrowPoint2);
				Gizmos.DrawLine(arrowPoint1, arrowPoint3);
			}
		}

		protected class Explosion : MonoBehaviour {
			public float EndLivesTime;
		}

		private void OnEnable() {
			Progress.Value = 0f;
			Frags.Value = 0;
			var pattern = Model.GameState.Instance.GetCurrentAsteroidSpawnPattern();
			if (pattern != null) AsteroidSpawnPattern = pattern;
			AsteroidsRemainCount = AsteroidSpawnPattern.AsteroidsCount;
			_asteroids = new ObjectPool<Asteroid>(this.transform, AsteroidPrefabs);
			_explosions = new ObjectPool<Explosion>(this.transform, ExplosionsPrefab);
			_coroutin = StartCoroutine(SpawnCoroutine());
		}

		private void OnDisable() {
			StopCoroutine(_coroutin);
			_asteroids.Destroy();
			_asteroids = null;
		}

		protected virtual IEnumerator SpawnCoroutine() {
			while (AsteroidsRemainCount > 0) {
				var asteroidsNumberInWav = Mathf.Min(AsteroidsRemainCount, Random.Range(AsteroidSpawnPattern.MinAsteroidsNumberInWave, 
					AsteroidSpawnPattern.MaxAsteroidsNumberInWave + 1));
				AsteroidsRemainCount -= asteroidsNumberInWav;
				for (int i = 0; i < asteroidsNumberInWav; i++) {
					var proj = _asteroids.GetObject();
					proj.Velocity = (transform.forward * Random.Range(1f, 5f) + transform.right * Random.Range(-1f, 1f)) 
						* AsteroidSpawnPattern.SpeedModificator;
					proj.transform.position = transform.position + transform.right * Random.Range(-FrontLength * 0.5f, FrontLength * 0.5f);
				}
				yield return new WaitForSeconds(AsteroidSpawnPattern.WaveInterval);
			}
		}

		void Update() {
			int i = 0;
			while (i < _asteroids.ActiveObjects.Count) {
				var asteroid = _asteroids.ActiveObjects[i];
				var distance = (asteroid.transform.position - transform.position);
				var maxSqrDistance = FrontLength* FrontLength * 0.25 + FlightDistance * FlightDistance;
				if (distance.sqrMagnitude > maxSqrDistance) {
					_asteroids.ReturnObject(asteroid);
					Progress.Value = 1f - (float)(AsteroidsRemainCount + _asteroids.ActiveObjects.Count)/(float)AsteroidSpawnPattern.AsteroidsCount;
				}
				else if (!asteroid.isActiveAndEnabled) {
					_asteroids.ReturnObject(asteroid);
					Progress.Value = 1f - (float)(AsteroidsRemainCount + _asteroids.ActiveObjects.Count) / (float)AsteroidSpawnPattern.AsteroidsCount;
					var explosion = _explosions.GetObject();
					explosion.EndLivesTime = Time.time + ExplosionLivesTime;
					explosion.transform.position = asteroid.transform.position;
					Frags.Value++;
				}
				else {
					i++;
				}
			}

			i = 0;
			while (i < _explosions.ActiveObjects.Count) {
				var explosion = _explosions.ActiveObjects[i];
				if (Time.time > explosion.EndLivesTime) {
					_explosions.ReturnObject(explosion);
				}
				else {
					i++;
				}
			}
		}
	}
}
