using System.Collections;
using System.Collections.Generic;

namespace CraneWebAPI.Model {

	public class TaskList {
		public class Task {
			public string Title { get; set; }
			public string Description { get; set; }
			public int TaskType { get; set; }
			public bool Complete { get; set; }
		}

		public int CrystalReward { get; set; }

		public List<Task> Tasks { get; set; }
		public TaskList(IEnumerable<Task> tasks) {
			Tasks = new List<Task>(tasks);
		}
		public TaskList() {
			Tasks = new List<Task>();
		}


		public Task this[int i] {
			get {
				return Tasks[i];
			}
		}

		public enum TaskType {
			Coins = 0,
			Friend = 1
		}
	}
	public class UserData {
		public string Login { get; set; }
		public string RefLink { get; set; }
		public int Rating { get; set; }
		public int DeltaRating { get; set; }
		public int Nims { get; set; }
		public int Crystals { get; set; }
		public int Rank { get; set; }
		public int NimOutLimit { get; set; }
		public bool RankUpdated { get; set; }
		public bool TaskUpdated { get; set; }
		public bool CanGetReward { get; set; }
		public int WeeklyProgress { get; set; }
		public TaskList Tasks { get; set; }
		public int NextDaylyChalingeTime { get; set; }
		public int NextAdTime { get; set; }
		
	};

}