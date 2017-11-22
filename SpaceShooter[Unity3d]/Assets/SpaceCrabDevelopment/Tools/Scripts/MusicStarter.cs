using UnityEngine;

namespace SpaceCrabDevelopment.Tools {
	public class MusicStarter : MonoBehaviour {

		public AudioClip Music;

		private void OnEnable() {
			MusicManager.Instance.Play(Music);
		}
	}
}
