using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall.UI {
	public class OptionsWindow : MonoBehaviour {

		public Button OKButton;
		public Scrollbar AI;
		public Scrollbar Sound;
		public Scrollbar Music;

		void OnValidate() {
			if (!OKButton) Debug.LogError("OKButton is null", this);
			if (!AI) Debug.LogError("Scrollbar AI is null", this);
			if (!Sound) Debug.LogError("Scrollbar Sound is null", this);
			if (!Music) Debug.LogError("Scrollbar Music is null", this);
		}

		public void Init() {
			Time.timeScale = 0f;

			Sound.value = PlayerPrefs.GetFloat("Options.Sound", 1f);
			Music.value = PlayerPrefs.GetFloat("Options.Music", 1f);
			AI.value = PlayerPrefs.GetFloat("Options.AI", 0f);

			OKButton.onClick.AddListener(() => {
				Time.timeScale = 1f;
				Destroy(gameObject);
			});
			Sound.onValueChanged.AddListener((v) => {
				PlayerPrefs.SetFloat("Options.Sound", v);
				SoundManager.Instance.Refresh();
			});
			Music.onValueChanged.AddListener((v) => {
				PlayerPrefs.SetFloat("Options.Music", v);
				MusicManager.Instance.Refresh();
			});
			AI.onValueChanged.AddListener((v) => {
				PlayerPrefs.SetFloat("Options.AI", v);
				PinBall.AI.Instance.Refresh();
			});
		}
	}
}
