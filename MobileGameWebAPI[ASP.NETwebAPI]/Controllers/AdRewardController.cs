using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/AdReward")]
    public class AdRewardController : Controller
    {
		//Успешный_просмотр_рекламы (id_сессии, данные_по_рекламе) => { user_state }
		[HttpGet]
		public Task<Model.UserData> Get(string sessionId, bool isVideo) {
			return Model.CraneAPI.Create().AdReward(sessionId, isVideo);
		}
	}
}