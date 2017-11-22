using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinBall {
	[RequireComponent(typeof(Image))]
	public class ProgressBar : MonoBehaviour {

		[SerializeField]
		[Range(0f, 1f)]
		float _progress = 0f;
		Image _image;

		public float Progress {
			get {
				return _progress;
			}
			set {
				_image.fillAmount = value;
				_progress = value;
			}
		}

		void OnValidate() {
			_image = GetComponent<Image>();
			Progress = _progress;
		}

		void Start() {
			OnValidate();
		}
	}
}
