﻿namespace Ezbob.API.AuthenticationAPI.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Web.Http;

	[RoutePrefix("api/Orders")]
	public class OrdersController : ApiController {
		[Authorize]
		[Route("")]
		public IHttpActionResult Get() {
			//ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
			//var Name = ClaimsPrincipal.Current.Identity.Name;
			//var Name1 = User.Identity.Name;
			//var userName = principal.Claims.Where(c => c.Type == "sub").Single().Value;
			return Ok(Order.CreateOrders());
		}

	}

	// [RoutePrefix("/api/sale-contract")] 




	#region Helpers

	public class Order {
		public int OrderID { get; set; }
		public string CustomerName { get; set; }
		public string ShipperCity { get; set; }
		public Boolean IsShipped { get; set; }

		public static List<Order> CreateOrders() {
			List<Order> OrderList = new List<Order> 
            {
                new Order {OrderID = 1122, CustomerName = "Andres", ShipperCity = "Good", IsShipped = true },
                new Order {OrderID = 1123, CustomerName = "John", ShipperCity = "London", IsShipped = false}
            };

			return OrderList;
		}
	}

	#endregion
}
