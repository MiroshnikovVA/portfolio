using System;
using System.Linq;

namespace SpaceShooter.Model {

	public class GameState {

		static GameState _instance;
		GameSave _save;

		string _currentLevel;
		private AsteroidSpawnPattern _currentAsteroidSpawnPattern;

		public static GameState Instance {
			get {
				if (_instance==null) {
					_instance = new GameState();
					_instance.LoadData();
				}
				return _instance;
			}
		}

		public void Reset() {
			UnityEngine.PlayerPrefs.DeleteKey("SpaceShooter.StateVersion");
			UnityEngine.PlayerPrefs.DeleteKey("SpaceShooter.State");
			LoadData();
		}

		void LoadData() {
			var version = UnityEngine.PlayerPrefs.GetInt("SpaceShooter.StateVersion", 0);
			var jsonData = UnityEngine.PlayerPrefs.GetString("SpaceShooter.State", null);
			_save = GameSave.Parse(version, jsonData);
		}

		void SaveData() {
			UnityEngine.PlayerPrefs.SetInt("SpaceShooter.StateVersion", GameSave.CurrentVersion);
			var jsonData = _save.GetJsonData();
			UnityEngine.PlayerPrefs.SetString("SpaceShooter.State", jsonData);
			UnityEngine.PlayerPrefs.Save();
		}

		public void LoadPlayerShipEquipments(Ship ship) {
			if (_save.PlayerShipEquipments == null || _save.PlayerShipEquipments.Length == 0) {
				_save.PlayerShipEquipments = ship.GetCurrentEquipments().Select(q=>q.EquipmentID).ToArray();
				ship.SetEquipments(_save.PlayerShipEquipments);
			}
			else {
				ship.SetEquipments(_save.PlayerShipEquipments);
			}
		}

		public void SavePlayerShipEquipments(Ship ship) {
			_save.PlayerShipEquipments = ship.GetCurrentEquipments().Select(q => q.EquipmentID).ToArray();
			SaveData();
		}

		public bool LevelIsCompleted(string levelName) {
			return _save.CompletedLevels.Contains(levelName);
		}

		public bool TryNewLevelOpened(string levelName) {
			if (_save.OpenedLevels.Contains(levelName)) return false;
			_save.OpenedLevels.Add(levelName);
			SaveData();
			return true;
		}

		public void SetCurrentLevel(string levelName, AsteroidSpawnPattern asteroidSpawnPattern) {
			_currentLevel = levelName;
			_currentAsteroidSpawnPattern = asteroidSpawnPattern;
		}

		public AsteroidSpawnPattern GetCurrentAsteroidSpawnPattern() {
			return _currentAsteroidSpawnPattern;
		}

		public void Victory() {
			_save.CompletedLevels.Add(_currentLevel);
			SaveData();
		}
	}
}
