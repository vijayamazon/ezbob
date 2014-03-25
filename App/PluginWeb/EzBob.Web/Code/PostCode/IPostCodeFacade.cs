namespace EzBob.Web.Code.PostCode {
	public interface IPostCodeFacade {
		IPostCodeResponse GetAddressFromPostCode(string postCode, int nUserID);
		IPostCodeResponse GetFullAddressFromPostCode(string id, int nUserID);
	} // interface IPostCodeFacade
} // namespace
