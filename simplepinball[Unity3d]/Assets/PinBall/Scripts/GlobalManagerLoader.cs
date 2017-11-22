using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class GlobalManagerLoader : MonoBehaviour {

		public GlobalManager Prefab;
		public string Name = "[GlobalManager]";

		void OnValidate() {
			if (!Prefab) Debug.LogError("Prefab is null", this);
		}

		void Awake() {
			var manager = FindObjectOfType<GlobalManager>();
			if (!manager) {
				manager = Instantiate(Prefab);
				manager.name = "[GlobalManager]";
			}
			Destroy(this.gameObject);
		}
	}
}
