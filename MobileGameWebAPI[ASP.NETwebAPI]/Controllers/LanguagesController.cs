using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Languages")]
    public class LanguagesController : Controller {
		[HttpGet]
		public string Get() {
			return Model.CraneAPI.Create().GetLanguages().GetStringLines();
		}
	}
}