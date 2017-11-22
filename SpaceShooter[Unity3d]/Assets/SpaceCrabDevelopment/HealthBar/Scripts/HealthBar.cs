using UnityEngine;
using System.Collections;
using System;

namespace SpaceCrabDevelopment.HealthBar {
	[AddComponentMenu("SpaceCrabDevelopment/UnitsComponent/HealthBar")]
	public class HealthBar : MonoBehaviour {

		/// <summary> Активный ли юнит? У активного отображается рамка с хитами всегда. Можно использовать при наведении мыши, например </summary>
		public bool IsActive;

		/// <summary> Враг ли юнит игроку? </summary>
		public bool IsEnemy;

		[Range(0f, 1f)]
		/// <summary> Шкала хитов у юнита </summary>
		public float Health;

		[Range(0f, 1f)]
		/// <summary> Шкала ресурсов у юнита </summary>
		public float Resource;

		float _lastDammageTime = -1000f;
		bool _selected;

		/// <summary> Точки, обозначающие контуры юнита </summary>
		public BoundaryPoint[] BoundaryPoints { get; set; }

		void OnDisable() {
			if (CameraHealthBarManager.Instance) {
				CameraHealthBarManager.Instance.AllUnits.Remove(this);
			}
		}

		void OnEnable() {
			BoundaryPoints = GetComponentsInChildren<BoundaryPoint>();
			CameraHealthBarManager.Instance.AllUnits.Add(this);
		}

		/// <summary> Обработчик события, что жизни изменились </summary>
		public void OnHealthChanged(float newHealth) {
			Health = newHealth;
			_lastDammageTime = Time.time;
		}

		/// <summary> Обработчик события, что жизни изменились </summary>
		[ContextMenu("OnHealthChanged")]
		public void OnHealthChanged() {
			_lastDammageTime = Time.time;
		}

		/// <summary> Были ли хиты недавно изменены </summary>
		public bool IsRecentlyDamaged() {
			return Time.time - _lastDammageTime < 5f;
		}



		public void Select(bool value) {
			_selected = value;
			IsActive = value;
		}

		public void Preview(bool value) {
			IsActive = _selected || value;
		}
	}
}
