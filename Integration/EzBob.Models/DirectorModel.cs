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

		public bool IsExperian { get; set; }

		public int Id { get; set; }
		public string Name { get; set; }
		public string Middle { get; set; }
		public string Surname { get; set; }
		public char Gender { get; set; }
		public int Position { get; set; }
        public int? UserId { get; set; }
		public string DateOfBirth { get; set; }
		public CustomerAddress[] DirectorAddress { get; set; }
		public CustomerAddress[] PrevDirectorAddress { get; set; }

		public string Email { get; set; }
		public string Phone { get; set; }

		public string IsShareholder { get; set; }
		public string IsDirector { get; set; }

		public override string ToString() {
			return string.Format("{0}: {1} {2}", Id, Name, Surname);
		} // ToString

		public Director FromModel() {
			try {
				return new Director {
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
                    UserId = UserId
				};
			}
			catch (Exception e) {
				_log.Error("Failed to convert DirectorModel to Director", e);
				return null;
			} // try
		} // FromModel
        public Director UpdateFromModel( ref Director director)
        {
            try {

                        
            director.Name = Name;
            director.DateOfBirth = DateOfBirth.IndexOf("-", StringComparison.Ordinal) == -1 ? DateTime.ParseExact(DateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null;
            director.Middle = Middle;
            director.Surname = Surname;
            director.Gender = (Gender)Enum.Parse(typeof(Gender), Gender.ToString());
            director.Email = Email;
            director.Phone = Phone;
            director.IsDirector = IsDirector.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) || IsDirector .Equals(On, StringComparison.InvariantCultureIgnoreCase);
            director.IsShareholder = IsShareholder.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) ||IsShareholder.Equals(On, StringComparison.InvariantCultureIgnoreCase);
          
           
            return director;
             
            }
            catch (Exception e)
            {
                _log.Error("Failed to convert Update DirectorModel to Director", e);
                return null;
            } // try
        } // FromModel
        public Esigner ToEsigner(Director director,int customerId)
        {
            try
            {
                return new Esigner
                {

                    DirectorID = director.Id,
                    CustomerID = customerId,
                    FirstName = director.Name.Trim(),
                    LastName = director.Surname.Trim(),
                    MiddleName = (director.Middle ?? "").Trim(),
                    BirthDate = director.DateOfBirth,
                    Gender = director.Gender.ToString(),
                    Email = director.Email,
                    CompanyId = director.Company.Id,
                    MobilePhone = director.Phone,
                    IsDirector = director.IsDirector,
                    IsShareholder = director.IsShareholder,
                    UserId = director.UserId,
                };
            }
            catch (Exception e)
            {
                _log.Error("Failed to convert DirectorModel to Esigner", e);
                return null;
            } // try
        } // FromModel

		public static DirectorModel FromDirector(Director director, List<Director> directors) {
			return new DirectorModel {
				IsExperian = false,
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

		public static DirectorModel FromExperianDirector(ExperianDirector director, TypeOfBusinessReduced nBusinessType) {
			return new DirectorModel {
				IsExperian = true,
				Id = director.ID,
				Name = director.FirstName,
				Middle = director.MiddleName,
				Surname = director.LastName,
				Position = 0,
				DateOfBirth = FormattingUtils.FormatDateToString(director.BirthDate, "-/-/-"),

				DirectorAddress = new [] {
					new CustomerAddress {
						Line1 = director.Line1,
						Line2 = director.Line2,
						Line3 = director.Line3,
						Town = director.Town,
						County = director.County,
						Postcode = director.Postcode,
						AddressType = nBusinessType == TypeOfBusinessReduced.Limited
							? CustomerAddressType.LimitedDirectorHomeAddress
							: CustomerAddressType.NonLimitedDirectorHomeAddress
					},
				},

				PrevDirectorAddress = new CustomerAddress[0],
				Gender = director.Gender.HasValue ? director.Gender.Value : ' ',
				Email = director.Email,
				Phone = director.MobilePhone,
				IsDirector = director.IsDirector ? Yes : No,
				IsShareholder = director.IsShareholder ? Yes : No,
			};
		} // FromExperianDirector
	} // class DirectorModel
} // namespace
