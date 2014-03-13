namespace EzBob.Models {

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Database;
	using Web.Code;
	using Iesi.Collections.Generic;
	using System.Linq;
	using log4net;

	public class DirectorModel {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DirectorModel));

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

		public Director FromModel() {
			Director director = null;

			try {
				return director = new Director {
					Id = Id,
					Name = Name.Trim(),
					Surname = Surname.Trim(),
					Middle = (Middle ?? "").Trim(),
					DateOfBirth = DateOfBirth.IndexOf("-", StringComparison.Ordinal) == -1 ? DateTime.ParseExact(DateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null,
					DirectorAddressInfo = new DirectorAddressInfo{
                        AllAddresses = DirectorAddress == null ? null : new HashedSet<CustomerAddress>(DirectorAddress)
                    },
					Gender = (Gender)Enum.Parse(typeof(Gender), Gender.ToString()),
					Email = Email,
					Phone = Phone
				};
			}
			catch (Exception e) {
				_log.Error("Failed to convert DirectorModel to Director", e);
			}

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
			    DirectorAddress =
			        director.DirectorAddressInfo.AllAddresses.Where(
			            x =>
			            x.AddressType == CustomerAddressType.LimitedDirectorHomeAddress ||
			            x.AddressType == CustomerAddressType.NonLimitedDirectorHomeAddress).ToArray(),
                PrevDirectorAddress = director.DirectorAddressInfo.AllAddresses.ToArray(),
				Gender = director.Gender.ToString()[0],
				Email = director.Email,
				Phone = director.Phone
			};
		} // FromDirector
	} // class DirectorModel
} // namespace
