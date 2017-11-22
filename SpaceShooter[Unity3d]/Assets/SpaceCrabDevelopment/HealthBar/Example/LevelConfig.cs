using UnityEngine;
using System.Collections;

namespace SpaceCrabDevelopment.Examples {
	public class LevelConfig : MonoBehaviour {

		static private LevelConfig _instance;
		static public LevelConfig Instance {
			get {
				if (!_instance) {
					_instance = FindObjectOfType<LevelConfig>();
				}
				return _instance;
			}
		}

		/// <summary> фракция игрока и его союзников </summary>
		public int PlayerFraction;

	}
}
