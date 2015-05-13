﻿namespace Ezbob.Backend.CalculateLoan.Models.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	public static class ScheduleListExt {
		public static bool ContainsDate(this List<Scheduled> lst, DateTime date) {
			return (lst != null) && lst.Any(s => s.Date == date.Date);
		} // ContainsDate

		public static Scheduled FindByDate(this List<Scheduled> lst, DateTime date) {
			return (lst == null) ? null : lst.FirstOrDefault(s => s.Date == date.Date);
		} // FindByDate
	} // class ScheduleListExt
} // namespace
