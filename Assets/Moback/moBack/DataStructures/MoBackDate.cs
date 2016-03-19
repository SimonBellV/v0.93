using UnityEngine;
using System;
using SimpleJSON;

namespace MoBackInternal {
	public static class MoBackDate {
		public static SimpleJSONClass DateToMobackFormatJSON (DateTime date) {
			SimpleJSONClass dateJsonStructure = new SimpleJSONClass ();
			dateJsonStructure ["__type"] = "Date";
			dateJsonStructure ["iso"] = String.Format ("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}.{6:000}Z",
			                                          date.Year,
			                                          date.Month,
			                                          date.Day,
			                                          date.Hour,
			                                          date.Minute,
			                                          date.Second,
			                                          date.Millisecond);

			return dateJsonStructure;
		}

		
		public static DateTime DateFromString(string dateString) {
			DateTime date = default(DateTime);
			DateTime.TryParseExact(dateString,
			                       "yyyy-MM-ddTHH:mm:ss.fffK",
			                       null,
			                       System.Globalization.DateTimeStyles.AdjustToUniversal,
			                       out date);
			return date;
		}

		public static DateTime DateFromMoBackJSON(SimpleJSONNode dateWrapper) {
			return DateFromString(dateWrapper["iso"]);
		}

	}
}