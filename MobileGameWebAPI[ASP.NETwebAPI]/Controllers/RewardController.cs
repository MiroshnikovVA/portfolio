using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Reward")]
    public class RewardController : Controller{

		//��������_��������_������� (id_������, ������_��_�������) => { user_state }
		[HttpGet]
		public HttpResponseMessage Get(string data1, string data2) {
			Model.CraneAPI.Create().RewardCallback(data1, data2);
			return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
		}

	}
}