using UnityEngine;

namespace SpaceCrabDevelopment.Tools {
	public class GlobalManager : MonoBehaviour {
		public void Awake() {
			DontDestroyOnLoad(gameObject);
		}
	}
}
