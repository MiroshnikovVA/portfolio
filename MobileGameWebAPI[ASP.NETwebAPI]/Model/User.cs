using LinqToDB.Mapping;
using System;

namespace CraneWebAPI.Model
{
	/// <summary> Пользователи системы </summary>
	[Table(Name = "t_users")]
	public class User {
		/// <summary> Идентификатор пользователя </summary>
		[Column(Name = "id"), PrimaryKey, Identity]
		public int ID { get; set; }
		/// <summary> Логин </summary>
		[Column(Name = "login")]
		public string Login { get; set; }
		/// <summary> Пароль не в явном виде </summary>
		[Column(Name = "password_hash")]
		public string PasswordHash { get; set; }
		/// <summary> Логин пользователя, приславшего рефирал </summary>
		[Column(Name = "parent_login")]
		public string ParentLogin { get; set; }
		/// <summary> Первичная сумма на счету юзера из старой системы </summary>
		[Column(Name = "old_money")]
		public int OldMoney { get; set; }
		/// <summary> Идентификатор последней сессии </summary>
		[Column(Name = "session_id")]
		public string SessionID { get; set; }
		/// <summary> Колличество взломов отчёта времени </summary>
		[Column(Name = "scammer_actions_count")]
		public int ScammerActionsCount { get; set; }
		/// <summary> Количество монеток на счету </summary>
		[Column(Name = "coins")]
		public int Coins { get; set; }
		/// <summary> Количество монеток когда либо заработанных </summary>
		[Column(Name = "total_coins")]
		public int TotalCoins { get; set; }
		/// <summary> Количество кристаллов на счету </summary>
		[Column(Name = "crystals")]
		public int Crystals { get; set; }
		/// <summary> Номер ранга пользователя (считается с нуля) </summary>
		[Column(Name = "rank")]
		public int Rank { get; set; }
		/// <summary> оставлял ли пользователь оценку приложения </summary>
		[Column(Name = "is_rated_app")]
		public bool IsRatedApp { get; set; }
		/// <summary> время выполнения последнего дневного задания </summary>
		[Column(Name = "last_dayly_chalinge_time")]
		public DateTime LastDaylyChalingeTime { get; set; }
		/// <summary> время запроса последней рекламы </summary>
		[Column(Name = "last_ad_reward_time")]
		public DateTime LastAdRewardeTime { get; set; }
		/// <summary> время регистрации </summary>
		[Column(Name = "sign_up_time")]
		public DateTime SignUpTime { get; set; }
		/// <summary> время последнего входа в система </summary>
		[Column(Name = "sign_in_time")]
		public DateTime SignInTime { get; set; }
		/// <summary> количество монет, заработанное за сегодняшний день </summary>
		[Column(Name = "this_day_coins")]
		public int ThisDayCoins { get; set; }
		/// <summary> колличество очков за приглашенных людей </summary>
		[Column(Name = "invited_point")]
		public int InvitedPoint { get; set; }
		/// <summary> колличество использованных (за ежедневное задание) очков за приглашенных людей </summary>
		[Column(Name = "used_invited_point")]
		public int UsedInvitedPoint { get; set; }
		/// <summary> количество выполненных ежедневных заданий </summary>
		[Column(Name = "dayly_chalinge_day")]
		public int DaylyChalingeDay { get; set; }
		/// <summary> время запроса следующей рекламы </summary>
		[Column(Name = "next_ad_reward_time")]
		public DateTime NextAdRewardeTime { get; set; }

		public UserRating Rating;
	}

	[Table(Name = "t_global_state")]
	public class GlobalState {

		/// <summary> Идентификатор</summary>
		[Column(Name = "id"), PrimaryKey, Identity]
		public int ID { get; set; }

		/// <summary> Время последнего обновления рейтинга </summary>
		[Column(Name = "last_update_rating_date")]
		public DateTime LastUpdateRatingDate { get; set; }
	}

	[Table(Name = "t_users_rating")]
	public class UserRating {
		/// <summary> Идентификатор</summary>
		[Column(Name = "user_id"), PrimaryKey, Identity]
		public int UserID { get; set; }

		/// <summary> Место в таблице лидеров </summary>
		[Column(Name = "rating")]
		public int? Rating { get; set; }

		/// <summary> Вчерашнее место в таблице лидеров </summary>
		[Column(Name = "old_rating")]
		public int? OldRating { get; set; }
	}

	public static class UserHelper {

		public static int GetThisDayCoins(this User user) {
			var now = DateTime.Now;
			var nowDay = now.Year * 1000 + now.DayOfYear;
			var lastRewardTime = user.LastAdRewardeTime;
			var lastRewardTimeDay = lastRewardTime.Year * 1000 + lastRewardTime.DayOfYear;
			return lastRewardTimeDay != nowDay ? 0 : user.ThisDayCoins;
		}



		public static void GetDaylyChalingeDayAndAvaliable(this User user, out int daylyChalingeDay, out bool avaliableDaylyChalinge) {
			var now = DateTime.Now;
			var lastDaylyTime = user.LastDaylyChalingeTime;
			var deltaDayly = now.Subtract(user.LastDaylyChalingeTime).TotalSeconds;
			const int secondsInDay = 24 * 60 * 60;
			int rez = user.DaylyChalingeDay;
			avaliableDaylyChalinge = true;
			if (deltaDayly > secondsInDay * 2) {
				//Если прошло двое суток, то сбрасываем цепочку
				rez = 0;
			} else if (deltaDayly < secondsInDay) {
				//Если прошло менее суток, то задание выполнять еще нельзя
				avaliableDaylyChalinge = false;
			}
			daylyChalingeDay = rez;
		}


	}
}
