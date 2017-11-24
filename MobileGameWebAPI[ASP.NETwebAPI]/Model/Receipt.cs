using LinqToDB.Mapping;
using System;

namespace CraneWebAPI.Model
{
	/// <summary> Квитанции </summary>
	[Table(Name = "t_receipts")]
	public class Receipt {
		[Column(Name = "id"), PrimaryKey, Identity]
		public int ID { get; set; }
		[Column(Name = "coins")]
		public int Coins { get; set; }
		[Column(Name = "account_data")]
		public string AccountData { get; set; }
		[Column(Name = "request_time")]
		public DateTime RequestTime { get; set; }
		[Column(Name = "success")]
		public bool? Success { get; set; }
		[Column(Name = "responce_time")]
		public DateTime? ResponceTime { get; set; }
		[Column(Name = "responce_comment")]
		public string ResponceComment { get; set; }
		[Column(Name = "user_id")]
		public int UserID { get; set; }
	}
}
