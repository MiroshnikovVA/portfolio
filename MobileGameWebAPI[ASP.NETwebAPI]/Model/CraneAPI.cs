using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using System.IO;
using System.Collections.Concurrent;

namespace CraneWebAPI.Model
{
    public class CraneAPI {

		static class Сonditions {
			/// <summary> Награда в кристалах за инвайт </summary>
			public const int InvitationCrystals = 20;
			/// <summary> Награда в кристалах за применение инвайта при регистрации </summary>
			public const int UsingInvitationCrystals = 20;
			/// <summary> Стартовый Пак Нимов </summary>
			public const int StartNimsPack = 100;

			/// <summary> Награда в кристалах за переход в магазин </summary>
			public const int UserRatedCrystals = 5;
			/// <summary> Награда в монетках за переход в магазин </summary>
			public const int UserRatedCoins = 50;

			public const int StandartVideoReward = 100;
			public const int StandartInterstitialReward = 30;

			public const int InterstitialTime = StandartInterstitialReward * 6;
			public const int VideoRewardTime = StandartVideoReward * 6;


			public static readonly int[] AddVideoReward			= new int[] {  0,   1,   3,   7,  10,  10,  20,  50,   80,  100,  150,  250,  350,  400,  500 };
			public static readonly int[] AddInterstitialReward	= new int[] {  0,   1,   1,   2,   3,   3,   6,  15,   24,   30,   45,   75,   90,  120,  150 };
			public static readonly int[] RankCrystalPrice		= new int[] {  0,  40, 100, 200, 300, 400, 600, 800, 1000, 1200, 1500, 2500, 3500, 4500, 6000 };
			public static readonly int[] Limit					= new int[] {500, 500, 500, 500, 500, 450, 400, 350,  300,  250,  200,  150,  100,   50,   10 }
				.Select(q=>q*1000).ToArray();

			public static readonly int[] DailyChalingeCoins =		  new int[] { 500, 500, 1000, 1500, 2000, 2500, 3000 };
			public static readonly int[] DailyChalingeCrystalReward = new int[] {   1,   2,    3,    5,    5,   10,   10 + CompletedDaylyChalingeCrystalReward };
			public static readonly bool[] DailyChalingeInvitation = new bool[] { false, false, false, true, false, true, true };
			const int CompletedDaylyChalingeCrystalReward = 55;
		}

		static ConcurrentBag<string> _rewardBag = new ConcurrentBag<string>();

		public void RewardCallback(string data1, string data2) {
			var encryption_key = "!#@";

			byte[] key;
			using (var sha256 = SHA256.Create()) {
				key = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(encryption_key));
			}
			var iv = StringToByteArray(data1);
			var encrypted = StringToByteArray(data2); //cipherT
			var query = AesDecodeData(key, iv, encrypted);

			var dic = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query.Replace("+","%2B"));
			string res;
			string user_id = "";
			try {
				user_id = dic["user_id"].ToString();
				var decryptedLogin = LoginDecode(user_id);
				if (decryptedLogin[0] == '\x01') BotWatchReport();
				else UserWatchReport(decryptedLogin);
				res = decryptedLogin;
			} catch (Exception excep) {
				res = excep.Message;
			}

			using (var db = DB.GetDBConnection()) {
				db.ExecuteProc("write_log_reward_callback",
					new DataParameter("data1value", query),
					new DataParameter("data2value", res)
				);
			}
		}

		private void UserWatchReport(string decryptedLogin) {
			_rewardBag.Add(decryptedLogin);
		}

		private void BotWatchReport() {

		}

		string LoginDecode(string encryptedLogin) {
			var closeKey = "<RSAKeyValue><Modulus>zcRQJYRAdQwzm5JZkc0G1UPaQzkLjJ50WxFi6rg94DD6Kjh5kxBjRHY02VMmZObqRhsxFrw2jje76eFSjCr+WYOwq0XYXZ61YP+yBTX5LpVlGJzJArgnKHvhpSo6Az4H5xlwNKkK1DXQm51LQVIfFX6uacQFJ3Oz8VG2+W6sFYzB4y0J0r70jWh5LTDCra6fCKXcxLniep9fQhb6+1YTYQkqmM0JaKS7Xj1naxNYWcRlKx5tvHzST413gxGPUDHoMEJkbzP2M7wb21J/FSQ1WR6W5Bh8HQeKV2KdTwySKLJEizJBJBXGt/yfUV8Egk1bNA841E1utKbbCk8WxEt8wQ==</Modulus><Exponent>AQAB</Exponent><P>2Sb05tPUn4gx+WK/njPfgmCqZ2qTzW/86kdttz0r249orZKvapMuqe7vtU4Wfpj61KL+8jDZUUFhupqBK5q+Ju62dXr78yHTLTTt8cj3x5Li+8VX8cGzITZTgU9eLDjrpVxeojhbZ/r5sKuu0GNLWs1PRmLy6sGzQJHrckiSQ1M=</P><Q>8pPvoeGMXerLX99HriHGanUGqpHKGSlrrLtem/ZcVWS7tUw2i++AH4PQykDmkKXPp7T8XvFR8JjEDrx5tvPXFvZX8YnjXp1rvCqfM5kUzudf/AXqVn6YICqUGLjRQiKxkbY905JekBFwxdFYPRbCuK9WBGBwbKQ3Pk0OkEomsRs=</Q><DP>nD3NY+/yQj7KRjdoy7ljDfnjYblrUxtKjH6MUJVw4u8SUCEEmdgAcUB7tKXUxY3om+oTKcs/8ZjrakoUun0CaBzFp02vzkX+Hb7BnYAN18i2DJT3K/lEm5btClNC3OqvkjEd3fZSvkP0N8uvYjyvUUv7yBcV1Rc9lC3pjDbvFgU=</DP><DQ>jifPuKg64BbmlAp/MTat1lk6TN8e9lvls9Yh/XgEaC6eKgK9vIfgJ5fR1ZTvCTmCVZ+kfechw8NisdgV4/dFxzkaFHxf+GR8bEr1/QDqxs2k3EaDK6kIcWFCZLJ6Py2hreiEluYh8H+n/OVPto0OE7j8yZfSing5v5LcxAYQYp0=</DQ><InverseQ>Mwo95KXsaRQhgnLXkQIKdhYwLFoO6S9RJCgFnE7M29da2JYxdXld8Ljmag+3EMtDzuy0wNZLJvz1/jpw+3ZAyvDeSc83mxZh++Ipy3QHcaAxU7NTCSZ6AXPARmCc6jnTsQDs4gMMNAxaytdXJb02t46zGAdwZz/VnCzVhQ6/zrc=</InverseQ><D>pjJ4IbVOcrvmOzPcEL2vVTY25rD+uReLF7tVUAx8PcaRaOSwna5Q340yZo6Ypks00mdIIProfbpaly+dgwV+50JMwinp9sQn8C7W1QqhN5UHUPfiC8Y+Kp7UxJj0voWt0GVlkcMvsfO9iNNlY183KB9oNJlVKGZ14uaDibhwWf4hkKvvTY8F9VqtLz4QDk8Hz8yUtXxGV8PtveFI30NWLWokP+16AC/EtNXwWH3PknBkM5ValvJmYc3ailXON5uYmKFngCjZpBUzX3EpYFQlyA9P07m/rfyOWjdFkEkxj4X/mXQ0CPr0VPP68ngSWaDzBhwTY6oQJVJSh9VyOBAI+Q==</D></RSAKeyValue>";
			var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048);
			rsa.FromXmlString(closeKey);
			var encryptedArray = System.Convert.FromBase64String(encryptedLogin);
			return Encoding.UTF8.GetString(rsa.Decrypt(encryptedArray, false));
		}

		string AesDecodeData(byte[] key, byte[] iv, byte[] encrypted) {
			using (var aesAlg = AesCryptoServiceProvider.Create()) {
				aesAlg.Key = key;
				aesAlg.IV = iv;
				aesAlg.Mode = CipherMode.CBC;
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(); //aesAlg.Key, aesAlg.IV
				using (var msDecrypt = new MemoryStream(encrypted)) {
					using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
						using (var srDecrypt = new StreamReader(csDecrypt)) {
							return srDecrypt.ReadToEnd();
						}
					}
				}
			}
		}

		byte[] StringToByteArray(string hex) {
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}

		public IDictionary<string, string> GetLanguages() {
			using (var db = DB.GetDBConnection()) {
				return db.GetTable<Language>().ToDictionary((lang) => lang.Name, (lang) => lang.LocalName);
			}
		}

		class TranslationResult {
			[Column(Name = "ph_code")]
			public string Code { get; set; }

			[Column(Name = "translation")]
			public string Translation { get; set; }
		}

		public IDictionary<string, string> GetLocale(string language) {
			using (var db = DB.GetDBConnection()) {
				var langId = db.GetTable<Language>().First(q => q.Name == language).ID;
				return db.GetTable<Translation>()
					.Where(t => t.LanguageID == langId)
					.ToDictionary(
						t => t.Code,
						t => t.Value
					);
			}
		}

		public UserData UserRatedApp(string sessionId) {
			var user = GetUser(sessionId);
			if (!user.IsRatedApp) {
				user.IsRatedApp = true;
				user.Coins += Сonditions.UserRatedCoins;
				user.TotalCoins += Сonditions.UserRatedCoins;
				user.Crystals += Сonditions.UserRatedCrystals;
				using (var db = DB.GetDBConnection()) {
					db.Update(user);
				}
			}
			return GetUserDataFromUser(user);
		}

		public Model.AuthenticationData WithdrawingCoins(string sessionId, string accountData, int value) {
			var user = GetUser(sessionId);

			if (user.Coins >= Сonditions.Limit[user.Rank] && user.Coins >= value) {

				using (var db = DB.GetDBConnection()) {
					user.Coins -= value;
					db.Update(user);
					db.InsertWithIdentity(new Receipt() {
						Coins = value,
						AccountData = accountData,
						RequestTime = DateTime.Now,
						ResponceComment = null,
						ResponceTime = null,
						Success = null,
						UserID = user.ID
					});
				}
			}
			return GetAuthenticationDataFromUser(user);
		}

		public static CraneAPI Create() {
			return new CraneAPI();
		}

		#region private
		static DateTime _defaultDateTime = new DateTime(2017, 06, 01);

		static string MD5Hash(string input) {
			using (var md5 = MD5.Create()) {
				var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
				return Convert.ToBase64String(result);// Encoding.ASCII.GetString(result);
			}
		}

		static string GetHashPassword(string password) {
			var h = MD5Hash(password);
			var p1 = h.Substring(1, h.Length / 2);
			var p2 = h.Substring(5, h.Length / 2 - 7);
			var hash = MD5Hash(p2 + password + p1);
			return hash;
		}

		static string Base64Encode(string plainText) {
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		static string Base64Decode(string base64EncodedData) {
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}

		static string GetLoginFromReferal(string referal) {
			try {
				return String.IsNullOrEmpty(referal) ? null : Base64Decode(referal);
			}
			catch {
				throw new ArgumentException(nameof(referal));
			}
		}

		void UserLoadRating(User user, DataConnection db) {
			if (user != null) {
				var globalState = db.GetTable<GlobalState>().FirstOrDefault();
				if (globalState != default(GlobalState)) {
					var oldDate = globalState.LastUpdateRatingDate;
					var now = DateTime.Now;
					if (oldDate.Date != now.Date) {
						db.ExecuteProc("update_ratings");
						globalState.LastUpdateRatingDate = DateTime.Now;
						db.Update(globalState);
					}
				}
				else {
					db.ExecuteProc("update_ratings");
					globalState = new GlobalState() {
						LastUpdateRatingDate = DateTime.Now,
						ID = 0
					};
					db.Insert(globalState);
				}
				user.Rating = db.GetTable<UserRating>().FirstOrDefault(q=>q.UserID == user.ID);
			}
		}

		User GetUser(string sessionID) {
			User user;
			using (var db = DB.GetDBConnection()) {
				user = db.GetTable<User>().Where(u => u.SessionID == sessionID).FirstOrDefault();
				UserLoadRating(user, db);
			}
			if (user == null) throw new ArgumentException(nameof(sessionID));
			return user;
		}

		#endregion

		AuthenticationData GetAuthenticationDataFromUser(User user) {
			return new AuthenticationData() {
				FinanceData = GetFinanceDataFromUser(user),
				UserData = GetUserDataFromUser(user),
				SessionID = user.SessionID
			};
		}

		FinanceData GetFinanceDataFromUser(User user) {
			var finance = new FinanceData() {
				MinCoinsLimit = Сonditions.Limit[user.Rank]
			};
			using (var db = DB.GetDBConnection()) {
				var login = user.Login;
				var userID = user.ID;
				finance.Reciepts = db.GetTable<Receipt>()
					.Where(r => r.UserID == userID)
					.ToList()
					.Select(r => new FinanceData.Receipt() {
						Coins = r.Coins,
						ID = r.ID,
						ExtendedDescription = r.ResponceComment,
						Description = !r.Success.HasValue? "[!ReceiptDescriptionNotReviewed]" : 
							(r.Success.Value? "[!ReceiptDescriptionSuccess]" 
							: "[!ReceiptDescriptionDisapproved]"),
						Status = !r.Success.HasValue ? 0 : (r.Success.Value ? 1 : -1),
						Title = !r.Success.HasValue ? "[!ReceiptTitleNotReviewed]" :
							(r.Success.Value ? "[!ReceiptTitleSuccess]"
							: "[!ReceiptTitleDisapproved]"),
						AccountData = r.AccountData,
						RequestDate = r.RequestTime,
						ResponceDate = r.ResponceTime
					})
					.ToList();
			}
			return finance;
		}


		void CompletedDailyChallenge(int daylyChalingeDay, List<TaskList.Task> tasks) {
			if (daylyChalingeDay < 0) daylyChalingeDay = Сonditions.DailyChalingeInvitation.Length-1;
			var needInvite = Сonditions.DailyChalingeInvitation[daylyChalingeDay];
			var needCoins = Сonditions.DailyChalingeCoins[daylyChalingeDay];
			if (needInvite) {
				tasks.Add(new TaskList.Task() {
					Complete = true,
					Title = "Пригласить друзей",
					Description = $"1/1",
					TaskType = (int)TaskList.TaskType.Friend
				});
			}
			tasks.Add(new TaskList.Task() {
				Complete = true,
				Title = $"Заработать {needCoins} Ним",
				Description = $"{needCoins}/{needCoins}",
				TaskType = (int)TaskList.TaskType.Coins
			});
		}

		bool CanGetRewardDaylyChalinge(User user, int daylyChalingeDay, List<TaskList.Task> tasks =null) {
			var thisDayCoins = user.GetThisDayCoins();

			var canGetReward = true;
			var needInvite = Сonditions.DailyChalingeInvitation[daylyChalingeDay];
			var needCoins = Сonditions.DailyChalingeCoins[daylyChalingeDay];
			if (needInvite) {
				var needInviteReady = (user.InvitedPoint - user.UsedInvitedPoint) >= 1;
				canGetReward = canGetReward && needInviteReady;
				if (tasks != null) {
					tasks.Add(new TaskList.Task() {
						Complete = needInviteReady,
						Title = "Пригласить друзей",
						Description = $"{(needInviteReady ? "1" : "0")}/1",
						TaskType = (int)TaskList.TaskType.Friend
					});
				}
			}
			var coinsReady = thisDayCoins >= needCoins;
			var needCoinsCompleted = Math.Min(needCoins, thisDayCoins);
			if (tasks != null) {
				tasks.Add(new TaskList.Task() {
					Complete = coinsReady,
					Title = $"Заработать {needCoins} Ним",
					Description = $"{needCoinsCompleted}/{needCoins}",
					TaskType = (int)TaskList.TaskType.Coins
				});
			}
			canGetReward = canGetReward && coinsReady;
			return canGetReward;
		}

		bool RankUpdated(User user) {
			bool rankUpdated = (
				user.Rank + 1 < Сonditions.RankCrystalPrice.Length)
				&& (user.Crystals >= Сonditions.RankCrystalPrice[user.Rank + 1]);
			return rankUpdated;
		}

		UserData GetUserDataFromUser(User user) {
			user.GetDaylyChalingeDayAndAvaliable(out var daylyChallengeDay, out var avaliableDaylyChalinge);

			List<TaskList.Task> tasks = new List<TaskList.Task>();
			bool canGetReward = false;
			if (avaliableDaylyChalinge) {
				canGetReward = CanGetRewardDaylyChalinge(user, daylyChallengeDay, tasks);
			} else {
				daylyChallengeDay = daylyChallengeDay - 1;
				if (daylyChallengeDay < 0) daylyChallengeDay = 6;
				CompletedDailyChallenge(daylyChallengeDay, tasks);
			}
				
			bool rankUpdated = RankUpdated(user);
			var now = DateTime.Now;

			TaskList userDataTasks = new TaskList(tasks) {
				CrystalReward = Сonditions.DailyChalingeCrystalReward[daylyChallengeDay]
			};

			var deltaRating = user.Rating!=null && user.Rating.Rating.HasValue && user.Rating.OldRating.HasValue ?
				user.Rating.Rating.Value - user.Rating.OldRating.Value : 0;
			var currentRating = user.Rating != null && user.Rating.Rating.HasValue ? user.Rating.Rating.Value : 0;

			UserData userData = new UserData() {
				CanGetReward = canGetReward,
				Crystals = user.Crystals,
				Login = user.Login,
				NimOutLimit = Сonditions.Limit[user.Rank],
				Nims = user.Coins,
				Rank = user.Rank,
				RefLink = Base64Encode(user.Login),
				Tasks = userDataTasks,
				WeeklyProgress = avaliableDaylyChalinge ? daylyChallengeDay : daylyChallengeDay + 1,
				RankUpdated = rankUpdated,
				NextAdTime = Math.Max((int)user.NextAdRewardeTime.Subtract(now).TotalSeconds, 0),
				NextDaylyChalingeTime = Math.Max((int)(TimeSpan.FromHours(24) - now.Subtract(user.LastDaylyChalingeTime)).TotalSeconds, 0),
				//TaskUpdated =
				Rating = currentRating,
				DeltaRating = deltaRating
			};
			return userData;
		}

		public UserData RaiseRank(string sessionId) {
			var user = GetUser(sessionId);
			if (RankUpdated(user)) {
				user.Rank += 1;
				user.Crystals -= Сonditions.RankCrystalPrice[user.Rank];
				using (var db = DB.GetDBConnection()) {
					db.Update(user);
				}
			}
			return GetUserDataFromUser(user);
		}

		public AuthenticationData Authentication(string login, string password) {
			login = login.ToLower();
			User user;
			using (var db = DB.GetDBConnection()) {
				user = db.GetTable<User>().Where(u => u.Login == login).FirstOrDefault();
				if (user!=null) {
					UserLoadRating(user, db);
				}
			}
			if (user == null) {
				throw new ArgumentException("Login or password not found", nameof(login));
			}
			if (user.PasswordHash == GetHashPassword(password)) {
				user.SignInTime = DateTime.Now;
				return GetAuthenticationDataFromUser(user);
			} else {
				throw new ArgumentException("Login or password not found", nameof(password));
			}
		}

		public AuthenticationData SignUp(string login, string password, string referal, int oldCoins) {
			login = login.ToLower();
			string parentLogin; //= GetLoginFromReferal(referal);
			try {
				parentLogin = String.IsNullOrEmpty(referal) ? null : Base64Decode(referal);
			} catch {
				parentLogin = null;
				//throw new ArgumentException("Referal error", nameof(referal));
			}
			User newUser = new User() {
				Coins = oldCoins > 0 ? oldCoins : Сonditions.StartNimsPack,
				OldMoney = oldCoins,
				Crystals = 0,
				InvitedPoint = 0,
				UsedInvitedPoint = 0,
				ThisDayCoins = 0,
				IsRatedApp = false,
				ScammerActionsCount = 0,
				LastAdRewardeTime = _defaultDateTime,
				LastDaylyChalingeTime = _defaultDateTime,
				Login = login,
				ParentLogin = parentLogin,
				PasswordHash = GetHashPassword(password),
				Rank = 0,
				SessionID = Guid.NewGuid().ToString(),
				SignInTime = DateTime.Now,
				SignUpTime = DateTime.Now,
				DaylyChalingeDay = 0
			};
			using (var db = DB.GetDBConnection()) {
				var oldUser = db.GetTable<User>().Where(u => u.Login == login).FirstOrDefault();
				if (oldUser != null) throw new ArgumentException("This login already exists", nameof(login));
				var parenUser = db.GetTable<User>().Where(u => u.Login == parentLogin).FirstOrDefault();
				db.Insert(newUser);
				if (parenUser != null) {
					parenUser.InvitedPoint++;
					parenUser.Crystals += Сonditions.InvitationCrystals;
					newUser.Crystals += Сonditions.UsingInvitationCrystals;
					db.Update(parenUser);
				}
			}
			return GetAuthenticationDataFromUser(newUser);
		}

		public async Task<UserData> AdReward(string sessionId, bool isVideo) {
			var user = GetUser(sessionId);
			var login = user.Login;
			int maxDelay = 20;
			while (!_rewardBag.TryTake(out login)) {
				if (maxDelay < 0) {
					return GetUserDataFromUser(user);
				}
				maxDelay--;
				await Task.Delay(1000);
			}

			var rewardNimsesValue = isVideo ?
				Сonditions.StandartVideoReward + Сonditions.AddVideoReward[user.Rank] :
				Сonditions.StandartInterstitialReward + Сonditions.AddInterstitialReward[user.Rank];

			var now = DateTime.Now;
			var nowDay = now.Year * 1000 + now.DayOfYear;

			user.Coins += rewardNimsesValue;
			user.TotalCoins += rewardNimsesValue;
			CheckingTimesAndChangeThisDayCoins();

			using (var db = DB.GetDBConnection()) {
				db.Update(user);
			}

			return GetUserDataFromUser(user);

			void CheckingTimesAndChangeThisDayCoins() {
				var lastRewardTime = user.LastAdRewardeTime;
				if (lastRewardTime.Year * 1000 + lastRewardTime.DayOfYear != nowDay) {
					user.ThisDayCoins = 0;
				}
				var deltaRewarde = now.Subtract(user.LastAdRewardeTime).TotalSeconds;
				var seconds = (isVideo ? Сonditions.VideoRewardTime : Сonditions.InterstitialTime);
				var scammer = now < user.NextAdRewardeTime;
				if (scammer) user.ScammerActionsCount++;
				user.LastAdRewardeTime = now;
				user.NextAdRewardeTime = now.AddSeconds(seconds);
				user.ThisDayCoins += rewardNimsesValue;
			}
		}

		public UserData DailyChalingeReward(string sessionId, int clientDaylyChalingeday) {
			var user = GetUser(sessionId);
			user.GetDaylyChalingeDayAndAvaliable(out int daylyChalingeDay, out bool avaliableDaylyChalinge);
			if (!avaliableDaylyChalinge) throw new InvalidOperationException("DaylyChalinge timer error");
			if (clientDaylyChalingeday >= Сonditions.DailyChalingeCrystalReward.Length) throw new ArgumentNullException(nameof(clientDaylyChalingeday));
			if (daylyChalingeDay != clientDaylyChalingeday) {
				if (daylyChalingeDay == 0) daylyChalingeDay = clientDaylyChalingeday;
			}
			if (CanGetRewardDaylyChalinge(user, daylyChalingeDay)) {
				user.Crystals += Сonditions.DailyChalingeCrystalReward[daylyChalingeDay];
				user.ThisDayCoins = user.ThisDayCoins - Сonditions.DailyChalingeCoins[daylyChalingeDay];
				if (Сonditions.DailyChalingeInvitation[daylyChalingeDay]) user.UsedInvitedPoint++;
				user.LastDaylyChalingeTime = DateTime.Now;
				user.DaylyChalingeDay++;
				if (user.DaylyChalingeDay >= Сonditions.DailyChalingeCrystalReward.Length)
					user.DaylyChalingeDay = 0;

				using (var db = DB.GetDBConnection()) {
					db.Update(user);
				}
			}

			return GetUserDataFromUser(user);
		}
	}
}
