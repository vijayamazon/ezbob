using System;
using System.Globalization;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Code;
using Iesi.Collections.Generic;
using System.Linq; 

namespace EzBob.Web.Areas.Customer.Models
{
    public class DirectorModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Middle { get; set; }
        public string Surname { get; set; }

        public string DateOfBirth { get; set; }
        public CustomerAddress[] DirectorAddress { get; set; }

        public Director FromModel()
        {
            Director director = null;

            try{
                director = new Director
                       {
                           Id = Id,
                           Name = Name.Trim(),
                           Surname = Surname.Trim(),
                           Middle = Middle.Trim(),
                           DateOfBirth = DateOfBirth.IndexOf("-", StringComparison.Ordinal) == -1 ? DateTime.ParseExact(DateOfBirth, "d/M/yyyy", CultureInfo.InvariantCulture) : (DateTime?) null,
                           DirectorAddress = DirectorAddress == null ? null : new HashedSet<CustomerAddress>(DirectorAddress)
                       };
            }
            catch(Exception)
            {
            }

            return director;

        }

        public static DirectorModel FromDirector(Director director)
        {
         
                return new DirectorModel
                           {
                               Id = director.Id,
                               Name = director.Name,
                               Middle = director.Middle,
                               Surname = director.Surname,
                               DateOfBirth = FormattingUtils.FormatDateToString(director.DateOfBirth, "-/-/-"),
                               DirectorAddress = director.DirectorAddress.ToArray()
                           };
        }
    }
}