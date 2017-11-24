using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CraneWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/SignUp")]
    public class SignUpController : Controller
    {
		//Регистрация (логин, пароль, [реферал], старая_сумма_на_счету) => return { id_сессии, user_state, fin_state }
		[HttpGet]
		public Model.AuthenticationData Get(string login, string password, int oldCoins, string referal=null) {
			try {
				return Model.NimsesAPI.Create().SignUp(login, password, referal, oldCoins);
			} catch (Exception excep) {
				return new Model.AuthenticationData() {
					ErrorMessage = excep.Message,
					ErrorStackTrace = excep.StackTrace
				};
			}
		}


	}
}