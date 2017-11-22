using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceCrabDevelopment.Tools {
	[RequireComponent(typeof(AudioSource))]
	public class MusicManager : MonoBehaviour {

		static private MusicManager _instance;
		static public MusicManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<MusicManager>();
				}
				return _instance;
			}
		}

		public void Play(AudioClip music) {
			var source = GetComponent<AudioSource>();
			if (source.clip != music) {
				source.clip = music;
				source.loop = true;
				source.Play();
			}
		}

		internal void Refresh() {
			var source = GetComponent<AudioSource>();
			source.volume = PlayerPrefs.GetFloat("Options.Music", 1f);
		}

		void Awake() {
			Refresh();
		}
	}
}
