using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Authentication")]
    public class AuthenticationController : Controller {

		//Автоматическая_аутентификация (логин, пароль) => { id_сессии, user_state, fin_state, новые_пользователи_по_ивайту }
		[HttpGet]
		public Model.AuthenticationData Get(string login, string password) {
			try {
				return Model.CraneAPI.Create().Authentication(login, password);
			}
			catch (ArgumentException excep) {
				return new Model.AuthenticationData() {
					ErrorMessage = excep.Message,
					ErrorStackTrace = excep.StackTrace
				};
			}
		}
	}
}