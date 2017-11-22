using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall {

	public class ScoreView : MonoBehaviour {

		float _currentViewScore = 0;
		public Text Text;
		public ProgressBar ProgressBar;

		void OnValidate() {
			if (!ProgressBar) Debug.LogError("ProgressBar is null", this);
			if (!Text) Debug.LogError("Text is null", this);
		}

		void Start() {
			ScoreManager.Instance.ScoreChanged.AddListener(this, () => this.enabled = true);
		}

		void Update() {
			var target = ScoreManager.Instance.Score;
			_currentViewScore = Mathf.Lerp(_currentViewScore, target, 5f * Time.deltaTime);
			var intCurrentViewScore = (int)Mathf.Round(_currentViewScore);
			Text.text = intCurrentViewScore.ToString();
			ProgressBar.Progress = target / (float)LevelManager.Instance.ScoreToNextLevel;
			if (intCurrentViewScore == target)
				this.enabled = false;
		}
	}
}
