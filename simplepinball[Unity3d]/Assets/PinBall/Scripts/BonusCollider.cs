using UnityEngine;

namespace PinBall {
	[RequireComponent(typeof(Collider2D))]
	public class BonusCollider : MonoBehaviour {

		public AudioClip[] Clips;
		public int Reward = 0;
		
		void OnCollisionEnter2D(Collision2D collision) {
			SoundManager.PlayRandom(Clips);
			if (Reward != 0) ScoreManager.Instance.AddScore(Reward);
		}
	}
}
