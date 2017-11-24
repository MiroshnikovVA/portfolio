using LinqToDB.Mapping;
using System;
namespace CraneWebAPI.Model
{
	/// <summary> Покупки за реал </summary>
	[Table(Name = "t_purchases")]
	public class Purchase {
		[Column(Name = "id"), PrimaryKey, Identity]
		public int ID { get; set; }
		[Column(Name = "product_id")]
		public int ProductID { get; set; }
		[Column(Name = "user_id")]
		public int UserID { get; set; }
		[Column(Name = "time")]
		public DateTime Time { get; set; }
	}
}
