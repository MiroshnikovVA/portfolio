using LinqToDB.Mapping;
using System;

namespace CraneWebAPI.Model
{

	[Table(Name = "t_reward_callbacks")]
	public class RewardCallbackData
    {
		[Column(Name = "id"), PrimaryKey, Identity]
		public int ID { get; set; }

		[Column(Name = "data1")]
		public string Data1 { get; set; }

		[Column(Name = "data2")]
		public string Data2 { get; set; }
	}
}
