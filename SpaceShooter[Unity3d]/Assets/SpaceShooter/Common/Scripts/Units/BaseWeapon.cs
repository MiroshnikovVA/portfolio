using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	public abstract class BaseWeapon : MonoBehaviour {

		public abstract void StartFire();

		public abstract void EndFire();

	}
}
