using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PinBall {
	public class EndSceneManager : MonoBehaviour {

		public AudioClip EndSound;

		void Start() {
			if (EndSound) SoundManager.Play(EndSound);
		}
	} }
