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
		//���������_�_��������_�_�������_���������_������ (id_������) => { user_state }
		[HttpGet]
		public Model.UserData Get(string sessionId) {
			return Model.CraneAPI.Create().UserRatedApp(sessionId);
		}
	}
}