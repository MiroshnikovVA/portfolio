using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SpaceShooter.Model {
	[System.Serializable]
	public class AsteroidSpawnPattern {
		public int MaxAsteroidsNumberInWave = 5;
		public int MinAsteroidsNumberInWave = 3;
		public int AsteroidsCount = 50;
		public float WaveInterval = 2f;
		public float SpeedModificator = 1f;
	}
}
