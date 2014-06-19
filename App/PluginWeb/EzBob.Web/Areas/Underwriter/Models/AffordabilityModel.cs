using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Areas.Underwriter.Models
{
	public class AffordabilityModel
	{
		AffordabilityType Type { get; set; }
		DateTime DateFrom { get; set; }
		DateTime DateTo { get; set; }
		decimal Revenues { get; set; }
		decimal? Opex { get; set; }
		decimal? ValueAdded { get; set; }
		decimal? Salaries { get; set; }
		decimal? Tax { get; set; }
		decimal? Ebitda { get; set; }
		decimal? LoanRepaiment { get; set; }
		decimal? Fcf { get; set; }
		//todo annualized trend
		//todo quarter trend
	}

	public enum AffordabilityType
	{
		HMRC,
		Bank,
		PSP,
		Ecomm,
		Accounting
	}
}