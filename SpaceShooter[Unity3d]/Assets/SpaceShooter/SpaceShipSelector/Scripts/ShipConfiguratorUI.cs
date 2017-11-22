using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.SpaceShipSelector {
	public class ShipConfiguratorUI : MonoBehaviour {

		public Ship PlayerShip;
		public Button ForwardGunsLeftButton;
		public Button ForwardGunsRightButton;
		public Button RearGunsLeftButton;
		public Button RearGunsRightButton;
		public Button EngineRightButton;

		private void OnValidate() {
			if (!PlayerShip) Debug.Log("PlayerShip is null", this);
			if (!ForwardGunsLeftButton) Debug.Log("ForwardGunsLeftButton is null", this);
			if (!ForwardGunsRightButton) Debug.Log("ForwardGunsRightButton is null", this);
			if (!RearGunsLeftButton) Debug.Log("RearGunsLeftButton is null", this);
			if (!RearGunsRightButton) Debug.Log("RearGunsRightButton is null", this);
			if (!EngineRightButton) Debug.Log("EngineRightButton is null", this);
		}

		void Start() {
			Model.GameState.Instance.LoadPlayerShipEquipments(PlayerShip);

			ForwardGunsLeftButton.onClick.AddListener(() => ChangeEquipment(false, EquipmentComponent.EquipmentSlot.ForwardGuns));
			ForwardGunsRightButton.onClick.AddListener(() => ChangeEquipment(true, EquipmentComponent.EquipmentSlot.ForwardGuns));

			RearGunsLeftButton.onClick.AddListener(() => ChangeEquipment(false, EquipmentComponent.EquipmentSlot.RearGuns));
			RearGunsRightButton.onClick.AddListener(() => ChangeEquipment(true, EquipmentComponent.EquipmentSlot.RearGuns));

			EngineRightButton.onClick.AddListener(() => ChangeEquipment(true, EquipmentComponent.EquipmentSlot.Engine));
		}

		void ChangeEquipment(bool next, EquipmentComponent.EquipmentSlot slot) {
			PlayerShip.ChangeEquipment(next, slot);
			Model.GameState.Instance.SavePlayerShipEquipments(PlayerShip);
		}
	}
}
