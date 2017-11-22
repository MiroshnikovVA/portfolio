using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.StandardLevel {
	public class PlayerStateView : MonoBehaviour {

		public Ship PlayerShip;
		public RectTransform HealthView;
		public GameObject HPIconTemplate;
		public Image AsteroidsProgress;
		public AsteroidSpawner AsteroidSpawner;
		public AudioClip VictoryClip;
		public AudioClip LostClip;
		public GameObject VictoryWindow;
		public AudioClip PlayerDamage;


		List<GameObject> _hpIcons = new List<GameObject>();
		private bool _endGame;

		void OnValidate() {
			if (!PlayerShip) Debug.LogError("PlayerShip is null", this);
			if (!HealthView) Debug.LogError("HealthView is null", this);
			if (!HPIconTemplate) Debug.LogError("HPIconTemplate is null", this);
			if (!AsteroidsProgress) Debug.LogError("AsteroidsProgress is null", this);
			if (!AsteroidSpawner) Debug.LogError("AsteroidSpawner is null", this);
			if (!VictoryClip) Debug.LogError("VictoryClip is null", this);
			if (!LostClip) Debug.LogError("LostClip is null", this);
			if (!VictoryWindow) Debug.LogError("VictoryWindow is null", this);
			if (!PlayerDamage) Debug.LogError("PlayerDamage is null", this);
		}

		void Start() {
			PlayerShip.IsDead
				.Where(isDead => isDead)
				.Subscribe(_ => {
					if (_endGame) return;
					_endGame = true;
					SpaceCrabDevelopment.Tools.SoundManager.Play(LostClip);
					SpaceCrabDevelopment.Tools.UIManager.Instance.CreateDialogWindow(
						"You lost",
						"Start again?",
						() => SceneManager.LoadScene(SceneManager.GetActiveScene().name),
						() => { },
						() => SceneManager.LoadScene(0)
					);
				});

			PlayerShip.CurrentHp.Subscribe(x => {
				while (_hpIcons.Count<x) {
					var icon = Instantiate<GameObject>(HPIconTemplate, HealthView);
					icon.SetActive(true);
					_hpIcons.Add(icon);
				}
				while (_hpIcons.Count > Mathf.Max(0, x)) {
					Destroy(_hpIcons[_hpIcons.Count-1]);
					_hpIcons.RemoveAt(_hpIcons.Count - 1);
					SpaceCrabDevelopment.Tools.SoundManager.Play(PlayerDamage);
				}
			});

			AsteroidSpawner.Progress.Subscribe(progress => AsteroidsProgress.fillAmount = progress);

			AsteroidSpawner.Progress.Where(progress => progress >= 0.95f).Subscribe(_ => StartCoroutine(Victory()));
		}


		private IEnumerator Victory() {
			if (_endGame) yield break;
			_endGame = true;
			SpaceCrabDevelopment.Tools.SoundManager.Play(VictoryClip);
			Model.GameState.Instance.Victory();
			VictoryWindow.SetActive(true);
			yield return new WaitForSeconds(5f);
			SceneManager.LoadScene(0);
		}
	}
}
