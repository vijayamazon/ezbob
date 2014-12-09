namespace EzBob.Tests
{
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Web.Infrastructure.Filters;
	using Moq;
	using NUnit.Framework;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	[TestFixture]
    public class GlobalAreaAuthorizationFilterFixture
    {
        [Test]
        public void white_list_passes_not_authenticated_requests()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Customer");
            routeData.Values.Add("controller", "Wizard");
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void no_area_passes_not_authenticated_requests()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Fake");
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void if_area_is_not_listed_then_do_pass()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Underwriter");
            routeData.Values.Add("controller", "Fake");
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void if_user_has_no_specified_role_do_not_pass()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Customer");
            routeData.Values.Add("controller", "Fake");

            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Customer")).Returns(true);
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            var mockRepo = new Mock<IUsersRepository>();
            var user = new User();
            user.Roles.Add(new Role() {Name = "Underwriter"});
            mockRepo.Setup(x => x.GetUserByLogin("test.test.com")).Returns(user);

            attr.SetUsersRepository(mockRepo.Object);

            Assert.That(result, Is.False);
        }

        [Test]
        public void if_user_has_specified_role_pass()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Customer");
            routeData.Values.Add("controller", "Fake");

            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Web")).Returns(true);
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var mockRepo = new Mock<IUsersRepository>();
            var user = new User();
            user.Roles.Add(new Role() { Name = "Web" });
            mockRepo.Setup(x => x.GetUserByLogin("test.test.com")).Returns(user);

            attr.SetUsersRepository(mockRepo.Object);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void if_user_has_specified_and_other_role_pass()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web");

            var mockHttpContext = new Mock<HttpContextBase>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Customer");
            routeData.Values.Add("controller", "Fake");

            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Underwriter")).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Web")).Returns(true);
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);

            var mockRepo = new Mock<IUsersRepository>();
            var user = new User();
            user.Roles.Add(new Role() { Name = "Web" });
            user.Roles.Add(new Role() { Name = "Underwriter" });
            mockRepo.Setup(x => x.GetUserByLogin("test.test.com")).Returns(user);

            attr.SetUsersRepository(mockRepo.Object);

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void if_user_has_specified_and_other_role_but_strict_mode_fail()
        {
            var attr = new AuthorizeAttributeHelper("Customer", "Web", false, true);

            var mockHttpContext = new Mock<HttpContextBase>();

            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Customer");
            routeData.Values.Add("controller", "Fake");

            var user = new User();
            user.Roles.Add(new Role() { Name = "Web" });
            user.Roles.Add(new Role() { Name = "Underwriter" });

            mockHttpContext.Setup(c => c.User.Identity.IsAuthenticated).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Underwriter")).Returns(true);
            mockHttpContext.Setup(c => c.User.IsInRole("Web")).Returns(true);
            mockHttpContext.Setup(x => x.Request.RequestContext.RouteData).Returns(routeData);
            mockHttpContext.Setup(x => x.User.Identity.Name).Returns("test.test.com");

            var mockRepo = new Mock<IUsersRepository>();

            mockRepo.Setup(x => x.GetUserByLogin("test.test.com")).Returns(user);

            attr.SetUsersRepository(mockRepo.Object); 

            var result = attr.PublicAuthorizeCore(mockHttpContext.Object);

            Assert.That(result, Is.False);
        }

        [Test]
        public void redirects_to_admin_logon_when_area_matches()
        {
            var attr = new AuthorizeAttributeHelper("Underwriter", "Web", true);

            var mockHttpContext = new Mock<RequestContext>();
            var context = new Mock<AuthorizationContext>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Underwriter");
            routeData.Values.Add("controller", "Customers");
            context.Setup(x => x.HttpContext.Request.RawUrl).Returns("google.ru");
            context.Setup(x => x.RouteData).Returns(routeData);
            context.Object.RequestContext = mockHttpContext.Object;
            mockHttpContext.Object.RouteData = routeData;

            attr.PublicHandleUnauthorizedRequest(context.Object);

            Assert.That(context.Object.Result, Is.InstanceOf<RedirectToRouteResult>());
        }

        [Test]
        public void returns_not_authorized_when_area_doesnot_match()
        {
            var attr = new AuthorizeAttributeHelper("Underwriter", "Web", true);

            var mockHttpContext = new Mock<RequestContext>();
            var context = new Mock<AuthorizationContext>();
            var routeData = new RouteData();
            routeData.DataTokens.Add("area", "Web");
            routeData.Values.Add("controller", "Customers");
            context.Setup(x => x.HttpContext.Request.RawUrl).Returns("google.ru");
            context.Setup(x => x.RouteData).Returns(routeData);
            context.Object.RequestContext = mockHttpContext.Object;
            mockHttpContext.Object.RouteData = routeData;

            attr.PublicHandleUnauthorizedRequest(context.Object);

            Assert.That(context.Object.Result, Is.InstanceOf<HttpUnauthorizedResult>());
        }
    }

    public class AuthorizeAttributeHelper : GlobalAreaAuthorizationFilter
    {
        private IUsersRepository _usersRepo;

        public AuthorizeAttributeHelper(string areaName, string roleName, bool isAdminPageRedirect = false, bool strict = false) : base(areaName, roleName, isAdminPageRedirect, strict)
        {
        }

        public virtual bool PublicAuthorizeCore(HttpContextBase httpContext)
        {
            return base.AuthorizeCore(httpContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return PublicAuthorizeCore(httpContext);
        }

        public virtual HttpValidationStatus PublicOnCacheAuthorization(HttpContextBase httpContext)
        {
            return base.OnCacheAuthorization(httpContext);
        }

        protected override HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        {
            return PublicOnCacheAuthorization(httpContext);
        }

        protected override IUsersRepository UsersRepository()
        {
            return _usersRepo;
        }

        public virtual void SetUsersRepository(IUsersRepository repository)
        {
            _usersRepo = repository;
        }

        public void PublicHandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
