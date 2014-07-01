﻿namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Iesi.Collections.Generic;
	using System.Linq;
	using log4net;

	public class DirectorModel {
		private static readonly ILog _log = LogManager.GetLogger(typeof(DirectorModel));

		public const string Yes = "yes";
		public const string No = "no";
		public const string On = "on";

		public int Id { get; set; }
		public string Name { get; set; }
		public string Middle { get; set; }
		public string Surname { get; set; }
		public char Gender { get; set; }
		public int Position { get; set; }

		public string DateOfBirth { get; set; }
		public CustomerAddress[] DirectorAddress { get; set; }
		public CustomerAddress[] PrevDirectorAddress { get; set; }

		public string Email { get; set; }
		public string Phone { get; set; }

		public bool IsExperianShareholder { get; set; }
		public bool IsExperianDirector { get; set; }

		public string IsShareholder { get; set; }
		public string IsDirector { get; set; }

		public Director FromModel() {
			Director director = null;

			try {
				return director = new Director {
					Id = Id,
					Name = Name.Trim(),
					Surname = Surname.Trim(),
					Middle = (Middle ?? "").Trim(),
					DateOfBirth = DateOfBirth.IndexOf("-", StringComparison.Ordinal) == -1 ? DateTime.ParseExact(DateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
					DirectorAddressInfo = new DirectorAddressInfo {
						AllAddresses = DirectorAddress == null ? null : new HashedSet<CustomerAddress>(DirectorAddress)
					},
					Gender = (Gender)Enum.Parse(typeof(Gender), Gender.ToString()),
					Email = Email,
					Phone = Phone,
					IsDirector = IsDirector.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) || IsDirector.Equals(On, StringComparison.InvariantCultureIgnoreCase),
					IsShareholder = IsShareholder.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) || IsShareholder.Equals(On, StringComparison.InvariantCultureIgnoreCase),
				};
			}
			catch (Exception e) {
				_log.Error("Failed to convert DirectorModel to Director", e);
			} // try

			return director;
		} // FromModel

		public static DirectorModel FromDirector(Director director, List<Director> directors) {
			return new DirectorModel {
				Id = director.Id,
				Name = director.Name,
				Middle = director.Middle,
				Surname = director.Surname,
				Position = directors.IndexOf(director),
				DateOfBirth = FormattingUtils.FormatDateToString(director.DateOfBirth, "-/-/-"),
				DirectorAddress = director.DirectorAddressInfo.AllAddresses.Where(
					x =>
					x.AddressType == CustomerAddressType.LimitedDirectorHomeAddress ||
					x.AddressType == CustomerAddressType.NonLimitedDirectorHomeAddress
				).ToArray(),
				PrevDirectorAddress = director.DirectorAddressInfo.AllAddresses.ToArray(),
				Gender = director.Gender.ToString()[0],
				Email = director.Email,
				Phone = director.Phone,
				IsDirector = !director.IsDirector.HasValue || director.IsDirector.Value ? Yes : No,
				IsShareholder = director.IsShareholder.HasValue && director.IsShareholder.Value ? Yes : No,
			};
		} // FromDirector
	} // class DirectorModel
} // namespace
