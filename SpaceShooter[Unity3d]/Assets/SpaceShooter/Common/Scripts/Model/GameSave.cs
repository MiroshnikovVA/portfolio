using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SpaceShooter.Model {

	/// <summary> Данный класс отвечает за загрузку данных с учётом версионности </summary>
	class GameSave {

		[System.Serializable]
		class GameSaveVer1 {
			public string[] CompletedLevels = new string[0];
			public string[] OpenedLevels = new string[0];
			public string[] PlayerShipEquipments = new string[0];
		}

		public HashSet<string> CompletedLevels { get; set; }
		public HashSet<string> OpenedLevels { get; set; }
		public string[] PlayerShipEquipments { get; set; }

		public const int CurrentVersion = 1;

		public static GameSave Parse(int version, string jsonData) {
			GameSave save = new GameSave();
			switch (version) {
				case CurrentVersion:
					save.Init(JsonUtility.FromJson<GameSaveVer1>(jsonData));
					break;
				default:
					save.Init(new GameSaveVer1());
					if (version!=0) Debug.LogError("unknown GameSave version " + version);
					break;
			}
			return save;
		}

		void Init(GameSaveVer1 save) {
			CompletedLevels = new HashSet<string>(save.CompletedLevels);
			OpenedLevels = new HashSet<string>(save.OpenedLevels);
			PlayerShipEquipments = save.PlayerShipEquipments;
		}

		public string GetJsonData() {
			var save = new GameSaveVer1() {
				CompletedLevels = CompletedLevels.ToArray(),
				OpenedLevels = OpenedLevels.ToArray(),
				PlayerShipEquipments = PlayerShipEquipments
			};
			return JsonUtility.ToJson(save);
		}
	}


}
