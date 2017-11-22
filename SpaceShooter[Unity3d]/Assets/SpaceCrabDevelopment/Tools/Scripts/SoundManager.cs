﻿using UnityEngine;

namespace SpaceCrabDevelopment.Tools {
	[RequireComponent(typeof(AudioSource))]
	public class SoundManager : MonoBehaviour {

		AudioSource _source;
		static System.Random _rand = new System.Random();

		AudioClip _oldClip;
		float _oldTime;

		static private SoundManager _instance;
		static public SoundManager Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<SoundManager>();
				}
				return _instance;
			}
		}
		static public void Play(AudioClip clip) {
			if (Instance._oldClip == clip && Instance._oldTime == Time.time) return;
			Instance._oldClip = clip;
			Instance._oldTime = Time.time;
			if (clip) Instance._source.PlayOneShot(clip);
		}
		static public void PlayRandom(AudioClip[] clips) {
			Play(clips[_rand.Next(clips.Length)]);
		}

		public void Awake() {
			_source = GetComponent<AudioSource>();
			Refresh();
		}

		internal void Refresh() {
			var source = GetComponent<AudioSource>();
			source.volume = PlayerPrefs.GetFloat("Options.Sound", 1f);
		}
	}
}
