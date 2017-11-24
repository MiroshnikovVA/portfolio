using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraneWebAPI.Model
{
    public class FinanceData {
		
		public int MinCoinsLimit { get; set; }
		public List<Receipt> Reciepts { get; set; }

		public class Receipt {
			/// <summary> -1,0,1 </summary>
			public int Status { get; set; } 
			/// <summary> Название заявки </summary>
			public string Title { get; set; } 
			/// <summary> Описание заявки </summary>
			public string Description { get; set; }
			/// <summary> Сообщение о причине отказа </summary>
			public string ExtendedDescription { get; set; }
			/// <summary> Колличество монет в заявке </summary>
			public int Coins{ get; set; }
			/// <summary> Номер заявки </summary>
			public int ID { get; set; }

			public DateTime RequestDate { get; set; }

			public DateTime? ResponceDate { get; set; }

			public string AccountData { get; set; }
		}
	}
}
