using System;
namespace ApplicationMng.Model.Commands
{
	public interface IContext
	{
		Application CreateApplication(string strategyName, User user);
		void SubmitApplication(Application app, string prmsXml, string outlet, User user, SecurityApplication secApp, string itemsToBeSignedList, bool signatureRequired);
		void SaveApplication(Application application, string prmsXml, User user, SecurityApplication secApp, bool signatureRequired, string outlet, string nodeOutletName, string itemsToBeSignedList);
		void AddAttachment(string fileName, string docType, string description, byte[] body, long applicationId, string attachControlName, int userId);
	}
}
