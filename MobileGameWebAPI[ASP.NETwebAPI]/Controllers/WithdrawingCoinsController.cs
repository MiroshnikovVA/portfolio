using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/WithdrawingCoins")]
    public class WithdrawingCoinsController : Controller
    {
		//���������_�����_������� (id_������, ���_�_�������, �����) => { fin_state }
		[HttpGet]
		public Model.AuthenticationData Get(string sessionId, string accountData, int value) {
			return Model.CraneAPI.Create().WithdrawingCoins(sessionId, accountData, value);
		}
	}
}