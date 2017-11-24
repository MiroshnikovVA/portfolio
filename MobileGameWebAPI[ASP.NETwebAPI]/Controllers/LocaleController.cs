using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers {
	[Produces("application/json")]
	[Route("api/Locale")]
	public class LocaleController : Controller {

		[HttpGet]
		public string Get(string language) {
			return Model.CraneAPI.Create().GetLocale(language).GetStringLines();
		}
	}
}