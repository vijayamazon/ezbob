﻿namespace Demo {
	using System.Web.Mvc;

	public class FilterConfig {
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		} // RegisterGlobalFilters
	} // class FilterConfig
} // namespace
