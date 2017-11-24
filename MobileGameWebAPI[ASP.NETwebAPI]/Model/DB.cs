using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Configuration;
using LinqToDB.Data;

namespace CraneWebAPI.Model
{
	public class DB {

		static string DBPassword = "1234qwe";

		class MySettings : ILinqToDBSettings {
			public IEnumerable<IDataProviderSettings> DataProviders {
				get { yield break; }
			}

			public class ConnectionStringSettings : IConnectionStringSettings {
				public string ConnectionString { get; set; }
				public string Name { get; set; }
				public string ProviderName { get; set; }
				public bool IsGlobal => false;
			}

			public string DefaultConfiguration => "PostgreSQL";
			public string DefaultDataProvider => "PostgreSQL";

			public IEnumerable<IConnectionStringSettings> ConnectionStrings {
				get {
					yield return
						new ConnectionStringSettings {
							Name = "crane",
							ProviderName = "PostgreSQL",
							ConnectionString = "Server=127.0.0.1;Port=5432;Database=nimses;User Id=postgres;Password=141@ge5#dh1;Pooling=true;MinPoolSize=10;MaxPoolSize=100;"
						};
				}
			}
		}

		static DB() {
			DataConnection.DefaultSettings = new MySettings();
		}

		public static DataConnection GetDBConnection() {
			return new DataConnection("crane");
		}


		public static void Test() {
			try {
				using (var dc = new DataConnection("crane")){
					var user = new User() {
						Login = "test2",
						PasswordHash = "1234"
					};
					dc.Insert(user);
				}
			} catch (Exception excep) {

			}
		}
	}
}
