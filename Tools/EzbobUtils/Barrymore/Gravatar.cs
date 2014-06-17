﻿namespace Ezbob.Utils {
	using System.Text;
	using System.Web;

	public static class Gravatar {
		#region method Url

		public static string Url(string sEmail, string sDefaultImgUrl = null, int nImgSize = 0) {
			var os = new StringBuilder();

			os.AppendFormat("https://secure.gravatar.com/avatar/{0}", MiscUtils.MD5((sEmail ?? string.Empty).Trim().ToLower()).ToLower());

			string sSeparator = "?";

			if (!string.IsNullOrWhiteSpace(sDefaultImgUrl)) {
				os.AppendFormat("{0}d={1}", sSeparator, HttpUtility.UrlEncode(sDefaultImgUrl));
				sSeparator = "&";
			} // if

			if (nImgSize > 0) {
				os.AppendFormat("{0}s={1}", sSeparator, nImgSize);
				sSeparator = "&";
			} // if

			return os.ToString();
		} // Url

		#endregion method Url
	} // class Gravatar
} // namespace
