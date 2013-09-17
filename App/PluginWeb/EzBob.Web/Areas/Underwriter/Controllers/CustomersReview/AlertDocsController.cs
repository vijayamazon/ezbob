﻿using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using StructureMap;
using ZohoCRM;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class AlertDocsController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
        private readonly IZohoFacade _crm;


        //-----------------------------------------------------------------------------------
        public AlertDocsController(IEzbobWorkplaceContext context, NHibernateRepositoryBase<MP_AlertDocument> docRepo, IZohoFacade crm)
        {
            _context = context;
            _docRepo = docRepo;
            _crm = crm;
        }

        public ActionResult List(int id)
        {
            var model = (from d in _docRepo.GetAll() where d.Customer.Id == id select AlertDoc.FromDoc(d)).ToArray();
            return this.JsonNet(model);
        }

        //-----------------------------------------------------------------------------------
        public void DeleteDocs(int[] docIds)
        {
            foreach (var d in docIds.Select(docId => _docRepo.Load(docId)))
            {
                _docRepo.Delete(d);
            }
        }

        //-----------------------------------------------------------------------------------
        public void UploadDoc(string description, int customerId)
        {
            var files = Request.Files;
            if (files.Count != 1) return;
            var file = Request.Files[0];
            if (file == null) return;

            var body = new byte[file.InputStream.Length];
            file.InputStream.Read(body, 0, file.ContentLength);

            var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
            var customer = customerRepo.Get(customerId);
            var doc = new MP_AlertDocument
            {
                BinaryBody = body,
                Customer = customer,
                Employee = _context.User,
                Description = description,
                UploadDate = DateTime.Now,
                DocName = file.FileName
            };

            try
            {
                _crm.AddFile(customer.ZohoId, file.FileName, body);
            }
            catch 
            {
                
            }
            
            _docRepo.SaveOrUpdate(doc);
        }

        //-----------------------------------------------------------------------------------
        public FileResult File(int id)
        {
            var f = _docRepo.Get(id);
            if(f != null)
            {
                FileResult fs = new FileContentResult(f.BinaryBody, "octet/stream");
                fs.FileDownloadName = f.DocName;
                return fs;
            }
            return null;
        }
    }
}
