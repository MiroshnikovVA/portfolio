using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class LevelManager : MonoBehaviour {

		static private LevelManager _instance;
		static public LevelManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<LevelManager>();
				}
				return _instance;
			}
		}

		public AudioClip Music;
		public int ScoreToNextLevel = 200;
		public string NextLevel;

		bool _loadSceneStarted = false;

		void OnValidate() {
			if (ScoreToNextLevel<=0) Debug.LogError("ScoreToNextLevel <= 0", this);
		}

		void Start() {
			MusicManager.Instance.Play(Music);
			ScoreManager.Instance.ScoreChanged.AddListener(this, () => {
				if (ScoreManager.Instance.Score > ScoreToNextLevel) {
					StartLoadScene();
				}
			});
		}

		void StartLoadScene() {
			if (_loadSceneStarted) return;
			_loadSceneStarted = true;
			if (string.IsNullOrEmpty(NextLevel)) {
				//[TODO] отобразить победу
			}
			else {
				//[TODO] отобразить надпись что грузим новый уровень
				var loadingOperator = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(NextLevel);
			}
		}
	}
}
