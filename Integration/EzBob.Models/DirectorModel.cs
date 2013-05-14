using System;
using System.Globalization;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Code;
using Iesi.Collections.Generic;
using System.Linq;
using log4net;

namespace EzBob.Web.Areas.Customer.Models {
	public class DirectorModel {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DirectorModel));

		public int Id { get; set; }
		public string Name { get; set; }
		public string Middle { get; set; }
		public string Surname { get; set; }
		public char Gender { get; set; }

		public string DateOfBirth { get; set; }
		public CustomerAddress[] DirectorAddress { get; set; }
        public CustomerAddress[] PrevDirectorAddress { get; set; }

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
					Gender = (Gender)Enum.Parse(typeof(EZBob.DatabaseLib.Model.Database.Gender), Gender.ToString())
				};
			}
			catch (Exception e) {
				_log.Error("Failed to convert DirectorModel to Director", e);
			}

			return director;
		} // FromModel

		public static DirectorModel FromDirector(Director director) {
			return new DirectorModel {
				Id = director.Id,
				Name = director.Name,
				Middle = director.Middle,
				Surname = director.Surname,
				DateOfBirth = FormattingUtils.FormatDateToString(director.DateOfBirth, "-/-/-"),
			    DirectorAddress =
			        director.DirectorAddressInfo.AllAddresses.Where(
			            x =>
			            x.AddressType == CustomerAddressType.LimitedDirectorHomeAddress ||
			            x.AddressType == CustomerAddressType.NonLimitedDirectorHomeAddress).ToArray(),
                PrevDirectorAddress = director.DirectorAddressInfo.AllAddresses.ToArray(),
				Gender = director.Gender.ToString()[0]
			};
		} // FromDirector
	} // class DirectorModel
} // namespace
