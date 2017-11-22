using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class BallRespawn : MonoBehaviour {

		public int MinYPos = -5;
		public AudioClip RespawnClip;
		public int Penalty = 10; 

		Vector3 _respawnPosition;

		// Use this for initialization
		void Start() {
			_respawnPosition = transform.position;
			StartCoroutine(TryRespawn());
		}

		IEnumerator TryRespawn() {
			while (isActiveAndEnabled) {
				yield return new WaitForSeconds(0.1f);
				if (transform.position.y < MinYPos) {
					var trail = GetComponent<TrailRenderer>();
					if (trail) trail.enabled = false;
					ScoreManager.Instance.PenaltyScore(Penalty);
					Penalty = Penalty > 1 ? Penalty - 1 : 0;
					yield return new WaitForSeconds(1.0f);
					SoundManager.Play(RespawnClip);
					yield return new WaitForSeconds(0.5f);
					transform.position = _respawnPosition;
					if (trail) trail.enabled = true;
				}
			}
		}

	}
}
