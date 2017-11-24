using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/UserRatedApp")]
    public class UserRatedAppController : Controller
    {
		//Уведомить_о_переходе_в_магазин_поставить_оценку (id_сессии) => { user_state }
		[HttpGet]
		public Model.UserData Get(string sessionId) {
			return Model.CraneAPI.Create().UserRatedApp(sessionId);
		}
	}
}