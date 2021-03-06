﻿using System;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class AlertDoc
    {
        public int Id { get; set; }
        public string DocName { get; set; }
        public DateTime? CreateDate { get; set; }
        public string EmployeeName { get; set; }
        public string Description { get; set; }

        public static AlertDoc FromDoc(MP_AlertDocument doc)
        {
            return new AlertDoc
                       {
                           CreateDate = doc.UploadDate,
                           Description = doc.Description,
                           DocName = doc.DocName,
						   EmployeeName = doc.Employee != null ? doc.Employee.Name : "system",
                           Id = doc.Id
                       };
        }
    }
}