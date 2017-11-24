using LinqToDB.Mapping;
using System;

namespace CraneWebAPI.Model {
	[Table(Name = "v_languages")]
	public class Language {
		[Column(Name = "lang_id")]
		public int ID { get; set; }

		[Column(Name = "lang_name")]
		public string Name { get; set; }

		[Column(Name = "lang_local_name")]
		public string LocalName { get; set; }
	}

	[Table(Name = "v_translations")]
	public class Translation {

		[Column(Name = "lang_id")]
		public int LanguageID { get; set; }

		[Column(Name = "ph_code")]
		public string Code { get; set; }

		[Column(Name = "translation")]
		public string Value { get; set; }

	}
}
