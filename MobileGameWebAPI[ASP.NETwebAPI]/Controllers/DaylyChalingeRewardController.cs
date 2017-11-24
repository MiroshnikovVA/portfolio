using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/DailyChalingeReward")]
    public class DailyChalingeRewardController : Controller
    {
		//Получить_награду_за_задание (id_сессии, task_name) => { user_state }
		[HttpGet]
		public Model.UserData Get(string sessionId, int clientDaylyChalingeday) {
			return Model.CraneAPI.Create().DailyChalingeReward(sessionId, clientDaylyChalingeday);
		}
	}
}