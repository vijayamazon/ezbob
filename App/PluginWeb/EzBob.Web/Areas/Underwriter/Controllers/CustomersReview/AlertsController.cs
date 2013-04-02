using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class AlertsController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly AlertRepository _alertRepo;
        private readonly ICustomerRepository _customer;

        //-----------------------------------------------------------------------------------
        public AlertsController(IEzbobWorkplaceContext context, AlertRepository alertRepository, ICustomerRepository customer)
        {
            _context = context;
            _alertRepo = alertRepository;
            _customer = customer;
        }

        //-----------------------------------------------------------------------------------
        [Transactional]
        public ActionResult Index(int id, bool showPassed=false)
        {
            var customer = _customer.Get(id);
            var alerts = _alertRepo.GetAlertsByCustomer(customer.Id, showPassed).ToList().Select(a => new
            {
                AlertId = a.Id,
                a.AlertType,
                a.AlertText,
                a.Status,
                ActionMade = a.ActionToMake ?? "-",
                DateOfAction = a.ActionDate,
                Details = a.Details ?? "-",
                EmployeeName = a.Employee == null ? String.Empty: a.Employee.FullName,
                Role = a.Employee == null ? String.Empty : FormatRoles(a.Employee.Roles.ToArray()),
                CssRow = "alert" + a.AlertSeverity.ToString()
            });
            return this.JsonNet(alerts);
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        [Transactional]
        public ActionResult AlertOperation(int id)
        {
            var alert = (from a in _alertRepo.GetAll() where a.Id == id select new { a.Status, a.Details }).FirstOrDefault();
            return this.JsonNet(alert);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Transactional]
        public void SaveAlert(int Id, string Status, string Details)
        {
            var alert = (from a in _alertRepo.GetAll() where a.Id == Id select a).FirstOrDefault();
            if (alert == null) return;

            alert.Status = Status;
            alert.ActionDate = DateTime.Now;
            alert.Details = Details;
            alert.Employee = _context.User;
            _alertRepo.SaveOrUpdate(alert);
        }

        //-----------------------------------------------------------------------------------
        private string FormatRoles(Role[] roles)
        {
            if (roles == null|| roles.Length == 0) return String.Empty;
            return string.Join("; ", roles.Select(x=>x.Name));
        }

    }
}
