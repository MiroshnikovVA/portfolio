using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraneWebAPI.Model
{
    public class AuthenticationData {
		public string SessionID { get; set; }
		public UserData UserData { get; set; }
		public FinanceData FinanceData { get; set; }
		public string ErrorMessage { get; set; } = null;
		public string ErrorStackTrace { get; set; } = null;
	}
}
