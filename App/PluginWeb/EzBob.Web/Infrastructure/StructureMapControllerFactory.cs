namespace EzBob.Web.Infrastructure {
	using StructureMap;
	using System.Web.Mvc;
	using System.Web.Routing;

	public class StructureMapControllerFactory : DefaultControllerFactory {
		public override IController CreateController(RequestContext requestContext, string controllerName) {
			IController result;

			try {
				System.Type controllerType = base.GetControllerType(requestContext, controllerName);
				result = (ObjectFactory.GetInstance(controllerType) as IController);
			}
			catch (System.Exception) {
				result = base.CreateController(requestContext, controllerName);
			}

			return result;
		} // CreateController
	} // class StructureMapControllerFactory
} // namespace
