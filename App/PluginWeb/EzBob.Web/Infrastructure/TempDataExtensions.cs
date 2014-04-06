namespace EzBob.Web.Infrastructure {
	using System.Web.Mvc;

	public static class TempDataExtensions {
		#region method Put

		public static void Put<T>(this TempDataDictionary tempData, T value) where T : class {
			Put<T>(tempData, "", value);
		} // Put

		public static void Put<T>(this TempDataDictionary tempData, string key, T value) where T : class {
			tempData[typeof(T).FullName + key] = value;
		} // Put

		#endregion method Put

		#region method Get

		public static T Get<T>(this TempDataDictionary tempData, string key = "") where T : class {
			object obj;

			tempData.TryGetValue(typeof(T).FullName + key, out obj);

			return (obj == null)? default(T): (T)obj;
		} // Get

		#endregion method Get
	} // TempDataExtensions
} // namespace
