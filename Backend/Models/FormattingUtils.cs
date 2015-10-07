namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Text;

	public static class FormattingUtils {
		public static DateTime ParseDate(string date) {
			return DateTime.ParseExact(date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None);
		}

		public static DateTime ParseDateWithoutTime(string date) {
			return DateTime.ParseExact(date, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
		}

		public static DateTime ParseDateWithCurrentTime(string date) {
			var now = DateTime.UtcNow;

			DateTime parsed = DateTime.ParseExact(
				date,
				"dd/MM/yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
			);

			return parsed.AddHours(now.Hour).AddMinutes(now.Minute);
		}

		public static string FormatDateTimeToString(DateTime? dt, string nullValue = "") {
			return dt.HasValue ? dt.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : nullValue;
		}

		public static string FormatDateTimeToStringWithoutSpaces(DateTime? dt, string nullValue = "") {
			return dt.HasValue ? dt.Value.ToString("dd-MM-yyyy_HH-mm", CultureInfo.InvariantCulture) : nullValue;
		}

		public static string FormatDateToString(DateTime? dt, string nullValue = "") {
			return dt.HasValue ? dt.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : nullValue;
		}

		public static string FormatPounds(decimal value) {
			return string.Format("{0:0,0.00}", value);
		}

		public static string FormatShort(decimal value) {
			return string.Format("{0:0.00}", Math.Truncate(value * 10) / 10);
		}

		public static string FormatMiddle(decimal value) {
			return string.Format("{0:N2}", value);
		}

		public static string FormatNoDecimals(decimal value) {
			return string.Format("{0:N0}", value);
		}

		public static string FormatPoundsNoDecimals(decimal numeric) {
			return numeric == 0 ? "" : "£" + numeric.ToString("N0", CultureInfo.CreateSpecificCulture("en-gb"));
		}

		public static string NumericFormats(decimal numeric) {
			return "£" + numeric.ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
		}

		public static string FormatPoundsWidhDash(decimal numeric) {
			return numeric == 0 ? "" : "£" + numeric.ToString("N", CultureInfo.CreateSpecificCulture("en-gb"));
		}
		public static string ConvertingNumberToWords(int number) {
			var unit = number % 10;
			var ten = number / 10;

			var resultTen = string.Empty;
			var resultUnit = string.Empty;

			if (number >= 10 && number <= 19) {
				switch (number) {
				case 10:
					resultTen = "Tenth";
					break;
				case 11:
					resultTen = "Eleventh";
					break;
				case 12:
					resultTen = "Twelfth";
					break;
				case 13:
					resultTen = "Thirteenth";
					break;
				case 14:
					resultTen = "Fourteenth";
					break;
				case 15:
					resultTen = "Fifteenth";
					break;
				case 16:
					resultTen = "Sixteenth";
					break;
				case 17:
					resultTen = "Seventeenth";
					break;
				case 18:
					resultTen = "Eighteenth";
					break;
				case 19:
					resultTen = "Nineteenth";
					break;
				}
			}
			else if (number >= 20 && number <= 100) {
				switch (ten) {
				case 2:
					resultTen = "Twenty";
					break;
				case 3:
					resultTen = "Thirty";
					break;
				case 4:
					resultTen = "Fourty";
					break;
				case 5:
					resultTen = "Fifty";
					break;
				case 6:
					resultTen = "Sixty";
					break;
				case 7:
					resultTen = "Seventy";
					break;
				case 8:
					resultTen = "Eighty";
					break;
				case 9:
					resultTen = "Ninety";
					break;
				}
				switch (unit) {
				case 1:
					resultUnit = "first";
					break;
				case 2:
					resultUnit = "second";
					break;
				case 3:
					resultUnit = "third";
					break;
				case 4:
					resultUnit = "fourth";
					break;
				case 5:
					resultUnit = "fifth";
					break;
				case 6:
					resultUnit = "sixth";
					break;
				case 7:
					resultUnit = "seventh";
					break;
				case 8:
					resultUnit = "eighth";
					break;
				case 9:
					resultUnit = "ninth";
					break;
				}
			}
			else if (number >= 0 && number <= 9) {
				switch (number) {
				case 1:
					resultTen = "First";
					break;
				case 2:
					resultTen = "Second";
					break;
				case 3:
					resultTen = "Third";
					break;
				case 4:
					resultTen = "Fourth";
					break;
				case 5:
					resultTen = "Fifth";
					break;
				case 6:
					resultTen = "Sixth";
					break;
				case 7:
					resultTen = "Seventh";
					break;
				case 8:
					resultTen = "Eighth";
					break;
				case 9:
					resultTen = "Ninth";
					break;
				}
			}
			return string.Format("{0} {1}", resultTen, resultUnit);
		}

		public static string ConvertToWord(int number) {
			var c = new NumberToEnglish();
			return c.changeNumericToWords(number);
		}
	}

	public static class ZipString {
		public static byte[] Zip(string str) {
			var bytes = Encoding.UTF8.GetBytes(str);

			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream()) {
				using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
					msi.CopyTo(gs);
				}

				return mso.ToArray();
			}
		}

		public static string Unzip(byte[] bytes) {
			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream()) {
				using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
					gs.CopyTo(mso);
				}

				return Encoding.UTF8.GetString(mso.ToArray());
			}
		}
	}

	public class NumberToEnglish {
		public String changeNumericToWords(double numb) {
			String num = numb.ToString();
			return changeToWords(num, false);
		}
		public String changeCurrencyToWords(String numb) {
			return changeToWords(numb, true);
		}
		public String changeNumericToWords(String numb) {
			return changeToWords(numb, false);
		}
		public String changeCurrencyToWords(double numb) {
			return changeToWords(numb.ToString(), true);
		}
		private String changeToWords(String numb, bool isCurrency) {
			String val = "", wholeNo = numb;
			String andStr = "", pointStr = "";
			String endStr = (isCurrency) ? ("Only") : ("");
			try {
				int decimalPlace = numb.IndexOf(".");
				if (decimalPlace > 0) {
					wholeNo = numb.Substring(0, decimalPlace);
					string points = numb.Substring(decimalPlace + 1);
					if (Convert.ToInt32(points) > 0) {
						andStr = (isCurrency) ? ("and") : ("point");// just to separate whole numbers from > points/cents
						endStr = (isCurrency) ? ("Cents " + endStr) : ("");
						pointStr = translateCents(points);
					}
				}
				val = String.Format("{0} {1}{2} {3}", translateWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
			}
			catch { ;}
			return val;
		}

		private String translateWholeNumber(String number) {
			string word = "";
			try {
				bool isDone = false;//test if already translated
				double dblAmt = (Convert.ToDouble(number));
				//if ((dblAmt > 0) && number.StartsWith("0"))
				if (dblAmt > 0) {//test for zero or digit zero in a nuemric
					bool beginsZero = number.StartsWith("0");//tests for 0XX

					int numDigits = number.Length;
					int pos = 0;//store digit grouping
					String place = "";//digit grouping name:hundres,thousand,etc...
					switch (numDigits) {
					case 1://ones' range
						word = ones(number);
						isDone = true;
						break;
					case 2://tens' range
						word = tens(number);
						isDone = true;
						break;
					case 3://hundreds' range
						pos = (numDigits % 3) + 1;
						place = " Hundred ";
						break;
					case 4://thousands' range
					case 5:
					case 6:
						pos = (numDigits % 4) + 1;
						place = " Thousand ";
						break;
					case 7://millions' range
					case 8:
					case 9:
						pos = (numDigits % 7) + 1;
						place = " Million ";
						break;
					case 10://Billions's range
						pos = (numDigits % 10) + 1;
						place = " Billion ";
						break;
					//add extra case options for anything above Billion...
					default:
						isDone = true;
						break;
					}
					if (!isDone) {//if transalation is not done, continue...(Recursion comes in now!!)
						word = translateWholeNumber(number.Substring(0, pos)) + place + translateWholeNumber(number.Substring(pos));
						//check for trailing zeros
						if (beginsZero)
							word = " and " + word.Trim();
					}
					//ignore digit grouping names
					if (word.Trim().Equals(place.Trim()))
						word = "";
				}
			}
			catch { ;}
			return word.Trim();
		}
		private String tens(String digit) {
			int digt = Convert.ToInt32(digit);
			String name = null;
			switch (digt) {
			case 10:
				name = "Ten";
				break;
			case 11:
				name = "Eleven";
				break;
			case 12:
				name = "Twelve";
				break;
			case 13:
				name = "Thirteen";
				break;
			case 14:
				name = "Fourteen";
				break;
			case 15:
				name = "Fifteen";
				break;
			case 16:
				name = "Sixteen";
				break;
			case 17:
				name = "Seventeen";
				break;
			case 18:
				name = "Eighteen";
				break;
			case 19:
				name = "Nineteen";
				break;
			case 20:
				name = "Twenty";
				break;
			case 30:
				name = "Thirty";
				break;
			case 40:
				name = "Fourty";
				break;
			case 50:
				name = "Fifty";
				break;
			case 60:
				name = "Sixty";
				break;
			case 70:
				name = "Seventy";
				break;
			case 80:
				name = "Eighty";
				break;
			case 90:
				name = "Ninety";
				break;
			default:
				if (digt > 0) {
					name = tens(digit.Substring(0, 1) + "0") + "" + ones(digit.Substring(1));
				}
				break;
			}
			return name;
		}
		private String ones(String digit) {
			int digt = Convert.ToInt32(digit);
			String name = "";
			switch (digt) {
			case 1:
				name = "One";
				break;
			case 2:
				name = "Two";
				break;
			case 3:
				name = "Three";
				break;
			case 4:
				name = "Four";
				break;
			case 5:
				name = "Five";
				break;
			case 6:
				name = "Six";
				break;
			case 7:
				name = "Seven";
				break;
			case 8:
				name = "Eight";
				break;
			case 9:
				name = "Nine";
				break;
			}
			return name;
		}

		private String translateCents(String cents) {
			string cts = "";
			for (int i = 0; i < cents.Length; i++) {
				string digit = cents[i].ToString(CultureInfo.InvariantCulture);
				string engOne = "";
				engOne = digit.Equals("0") ? "Zero" : ones(digit);
				cts += " " + engOne;
			}
			return cts;
		}
	}
}