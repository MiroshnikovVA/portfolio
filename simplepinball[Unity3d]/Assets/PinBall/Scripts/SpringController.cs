using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace PinBall {
	public class SpringController : MonoBehaviour {

		public Transform SpringBody;
		public Collider2D SpringPlatform;
		public LayerMask BallLayer;
		public GameObject TipObject;
		public float MaxImpulse = 40f;

		Coroutine _TryShowTipCoroutine;

		void OnValidate() {
			if (!SpringBody) Debug.LogError("SpringBody is null", this);
			if (!SpringPlatform) Debug.LogError("SpringPlatform is null", this);
			if (!TipObject) Debug.LogError("TipObject is null", this);
			if (BallLayer == 0) Debug.LogError("BallLayer is 0", this);
		}

		public void OnDisable() {
			TipObject.SetActive(false);
			StopCoroutine(_TryShowTipCoroutine);
			if (this && ControlManager.Instance) {
				ControlManager.Instance.SpringFire.RemoveListener(this);
				ControlManager.Instance.SpringChangeState.RemoveListener(this);
			}
		}

		public void OnEnable() {
			_TryShowTipCoroutine = StartCoroutine(TryShowTip());
			ControlManager.Instance.SpringFire.AddListener(this, Fire);
			ControlManager.Instance.SpringChangeState.AddListener(this, SetPower);
		}

		IEnumerator TryShowTip() {
			while (isActiveAndEnabled) {
				yield return new WaitForSeconds(0.5f);
				var ball = FindBall();
				if (ball) {
					TipObject.SetActive(true);
					ControlManager.Instance.BallTriggerEvent.Invoke(ControlManager.TriggerEvent.BallNearestSpring);
				}
				else {
					TipObject.SetActive(false);
				}
			}
		}

		Rigidbody2D FindBall() {
			var hit = Physics2D.Raycast(SpringPlatform.transform.position, Vector2.up, SpringPlatform.bounds.size.y, BallLayer.value);
			return hit.rigidbody;
		}

		void Fire(float power) {
			SpringBody.transform.DOScaleY(1f, 0.1f);
			var ball = FindBall();
			if (ball) {
				ball.AddForce(Vector3.up * MaxImpulse * power, ForceMode2D.Impulse);
			}
		}

		void SetPower(float power) {
			SpringBody.transform.localScale = new Vector3(1f, 1f - power * 0.75f, 1f);
		}
	}
}
