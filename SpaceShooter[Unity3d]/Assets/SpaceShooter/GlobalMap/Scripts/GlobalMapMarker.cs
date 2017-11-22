using SpaceShooter.Model;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using SpaceCrabDevelopment.Tools;

namespace SpaceShooter.GlabalMap {

	[RequireComponent(typeof(Button))]
	public class GlobalMapMarker : MonoBehaviour {

		public string SceneName;
		public AudioClip NewLevelClip;
		public AudioClip ClickClip;
		public GameObject BackgroundComplete;
		/// <summary> Требование, какой уровень должен быть пройден, для доступности данного </summary>
		/// <remarks> содержится в GlobalMapMarker, а не в GameState, так как это напрямую связано с раположением маркеров на карте </remarks>
		public GlobalMapMarker RequiredLevel;
		private Button _button;
		public AsteroidSpawnPattern AsteroidSpawnPattern;

		void OnValidate() {
			if (!BackgroundComplete) Debug.LogError("BackgroundComplete not found", this);
		}

		void OnDrawGizmos() {
			if (RequiredLevel) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(this.transform.position, RequiredLevel.transform.position);
			}
		}

		void Start() {
			Init();
		}

		bool IsReadyToPlay {
			get {
				if (!RequiredLevel) return true;
				return GameState.Instance.LevelIsCompleted(RequiredLevel.LevelName);
			}
		}

		string LevelName { get { return gameObject.name; } }

		void Init() {
			if (!IsReadyToPlay) {
				gameObject.SetActive(false);
				return;
			}
			if (GameState.Instance.TryNewLevelOpened(LevelName)) {
				if (NewLevelClip)  SoundManager.Play(NewLevelClip);
				transform.localScale = Vector3.zero;
				DOTween.Sequence()
					.Append(transform.DOScale(1.3f, 1f))
					.Append(transform.DOScale(1f, 1f))
					.Play();
			}
			BackgroundComplete.SetActive(GameState.Instance.LevelIsCompleted(LevelName));
			_button = GetComponent<Button>();
			_button.onClick.AddListener(TryStartPlay);
		}

		void TryStartPlay() {
			if (IsReadyToPlay) {
				if (ClickClip) SoundManager.Play(ClickClip);
				_button.onClick.RemoveListener(TryStartPlay);
				GameState.Instance.SetCurrentLevel(LevelName, AsteroidSpawnPattern);
				UnityEngine.EventSystems.EventSystem.current.enabled = false;
				DOTween.Sequence().Append(transform.DOScale(2f, 1f)).AppendCallback(() => SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single));
			}
		}
	}
}
