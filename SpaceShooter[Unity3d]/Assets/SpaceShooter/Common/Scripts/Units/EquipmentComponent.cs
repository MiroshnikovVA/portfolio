using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	public class EquipmentComponent : MonoBehaviour {

		public enum EquipmentSlot {
			Engine,
			ForwardGuns,
			RearGuns
		}

		public EquipmentSlot Slot;
		public string EquipmentID { get { return gameObject.name; } }
	}
}
