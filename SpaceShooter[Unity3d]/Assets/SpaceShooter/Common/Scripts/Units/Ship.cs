using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace SpaceShooter {
	[RequireComponent(typeof(CharacterController))]
	public class Ship : MonoBehaviour {

		public CharacterController CharacterController;
		private BaseWeapon[] _weapons;
		public int InitialHp = 3;

		public IReactiveProperty<int> CurrentHp { get; private set; }
		public IReactiveProperty<bool> IsDead { get; private set; }

		private void Awake() {
			CharacterController = GetComponent<CharacterController>();
			CurrentHp = new ReactiveProperty<int>(InitialHp);
			IsDead = CurrentHp.Select(x => x <= 0).ToReactiveProperty();
		}

		public void ChangeEquipment(bool next, EquipmentComponent.EquipmentSlot slot) {
			var equipments = GetAvailableEquipments(slot).ToList();
			var currentEq = GetCurrentEquipment(slot);
			var index = equipments.IndexOf(currentEq);
			if (next) index = (index + 1) % equipments.Count;
			else index = index == 0 ? equipments.Count - 1 : index - 1;
			currentEq.gameObject.SetActive(false);
			equipments[index].gameObject.SetActive(true);
		}

		public IEnumerable<EquipmentComponent> GetAvailableEquipments(EquipmentComponent.EquipmentSlot slotType) {
			var availableEquipments = GetComponentsInChildren<EquipmentComponent>(true);
			return availableEquipments.Where(q=>q.Slot == slotType);
		}

		public void AsteroidCollision() {
			CurrentHp.Value--;
		}

		public void EndFire() {
			foreach (var w in _weapons) w.EndFire();
		}

		public void StartFire() {
			foreach (var w in _weapons) w.StartFire();
		}

		public EquipmentComponent GetCurrentEquipment(EquipmentComponent.EquipmentSlot slotType) {
			var availableEquipments = GetComponentsInChildren<EquipmentComponent>();
			return availableEquipments.FirstOrDefault(q => q.Slot == slotType);
		}

		public IEnumerable<EquipmentComponent> GetCurrentEquipments() {
			return GetComponentsInChildren<EquipmentComponent>();
		}

		public void SetEquipments(IEnumerable<string> activeEquipments) {
			var availableEquipments = GetComponentsInChildren<EquipmentComponent>(true);
			foreach (var eq in availableEquipments) {
				eq.gameObject.SetActive(activeEquipments.Contains(eq.EquipmentID));
			}
			_weapons = GetComponentsInChildren<BaseWeapon>();
		}
	}
}
