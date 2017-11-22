using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class ScoreManager : MonoBehaviour {

		static private ScoreManager _instance;
		static public ScoreManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<ScoreManager>();
				}
				return _instance;
			}
		}

		public SafeEvent ScoreChanged = new SafeEvent();
		public int Score {	get;private set; }

		public void AddScore(int reward) {
			Score += reward;
			ScoreChanged.Invoke();
		}

		public void PenaltyScore(int penalty) {
			if (Score == 0) return;
			Score = Score > penalty ? Score - penalty: 0;
			ScoreChanged.Invoke();
		}
	}
}
