namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using Infrastructure;
	using Infrastructure.Filters;
	using NHibernate;
	using NHibernate.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using Web.Models;

	public class WizardController : Controller {
		#region public

		#region constructor

		public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			ISession session,
			ICustomerReasonRepository customerReasonRepository,
			ICustomerSourceOfRepaymentRepository customerSourceOfRepaymentRepository,
			IVipRequestRepository vipRequestRepository) {
			_context = context;
			_questions = questions;
			_customerModelBuilder = customerModelBuilder;
			_session = session;
			_reasons = customerReasonRepository;
			_sourcesOfRepayment = customerSourceOfRepaymentRepository;
			_vipRequestRepository = vipRequestRepository;
			} // constructor

		#endregion constructor

		#region action Index

		[IsSuccessfullyRegisteredFilter]
		public ActionResult Index(string provider = "") {
			ViewData["Questions"] = _questions.GetAll().ToList();
			ViewData["Reasons"] = _reasons.GetAll().OrderBy(x => x.Id).ToList();
			ViewData["Sources"] = _sourcesOfRepayment.GetAll().OrderBy(x => x.Id).ToList();
			ViewData["CaptchaMode"] = CurrentValues.Instance.CaptchaMode.Value;
			bool wizardTopNaviagtionEnabled = CurrentValues.Instance.WizardTopNaviagtionEnabled;
			ViewData["WizardTopNaviagtionEnabled"] = wizardTopNaviagtionEnabled;
			bool targetsEnabled = CurrentValues.Instance.TargetsEnabled;
			ViewData["TargetsEnabled"] = targetsEnabled;
			bool targetsEnabledEntrepreneur = CurrentValues.Instance.TargetsEnabledEntrepreneur;
			ViewData["TargetsEnabledEntrepreneur"] = targetsEnabledEntrepreneur;
			
			ViewData["MarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = _session
				.Query<MP_MarketplaceGroup>()
				.ToArray();

			var wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer, Session);
			if (!string.IsNullOrEmpty(provider)) {
				wizardModel.WhiteLabel = new WhiteLabelModel {
					Name = provider,
					Phone = provider + "-phone",
					Logo =
						@"iVBORw0KGgoAAAANSUhEUgAAAHsAAAAsCAYAAABFTMcwAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAABn/SURBVHhe7XwHfFRF93bs9cWunwVQFEXpUiNNREDpqCi8UqUooIIISpEWWihSDKEm9CQkAYLp2fTeew/pyW62ZTdb0/P8z8zuwgYiJBEL38vJ75C7996ZuXeeU54zk8WisbERf6QNDQ031fr6+mZaV1fXTGtra2/Qmpqaq1pdXd1M9Xp9M9XpdDdVrVbbTDUaTTNVq9U3VZVKdVu1qqrqX613HNhtAbclQNqiLU3YzVSpVP6rtU1gtwdck7YEcGvANWlLAP9ZcFsCzFxbmrCbqUKh+FdrM7BvBe712hqwW+vJNwPYHOS2gNsSgObaEmDm2tKEmWtlZeUdpW0CuzXgmrS9ntwSwOYgtwSqSW8F5u0GSy6X31HaIth2YT/doPbhq3EkZDmsBV/AymfiXb0DtUWw5ZryFlQIUdUVpIh9EFXuTHre+Puu3ilqYQKYqSlc30waQGG6SXtX70BtM9h35c6Vu2D/D8ltBbue+lCpNahUKPnv/x8Np564TXVdA+oaGtHU1GQ8+/cKG7WJ/9s2sTABzNRUUrVVpLJKhETG4YTDJWw/YIeNu2yx7cBx2DlcRHBELMRSmfHOdghNKPvJyi3Ahd8FcLjgCQ/fIJQJKzipbK3o9NWISUyBI7V3vOAF3+AI6GqqjVdvIfQM+tp6RBfKcTA0H9b+ufBMF0LH5+rvA5zNQ3KZAm6pZciqUBnPtl7+NNjZVwqwfocNeo/4FE++MRQPdhyEBzoOwIOdBuLJrkPQc9hUrN2yH+lZV8zAuc4uDaZ6g5h7zt5Dp/FitxF48P/1Rc8hk+BOgLNo1FoRiWX4bvUWPNGpPzq8OhgfTP0KYqo9W5QbnqURftkVGG0TgadWeeENKwFWXUqFQl9jvN78WZnc8LmlF7yFGFpca6cnjFbQuH23++NsTJHxbOvlT4Gdk1+EucvW44muw/BwZ0s81GkwHnnVEo92GYJHXnuPf36YJvaprkMxa/EaJKVnGVveKLcKifuPnsPLvUfj0c6D0WfkNHgIQnnaaK0IK6RYsHwjLJ7pBYsX+mDguFmokP0B2FfF8Ey1jQ1Y7JKC59f54qvzyfDMFCFDVIXGppYiC2tj9i43fy0uhlcnc7jFHJSpdJh4PBZ9dgVDkCM2nm29tBtsZZUKK9bvwtPd3sf95M0Pv2aJl3qPwahpX+O/i9di9OeL8XLfsXikiyV5+SBuEEvXbIdErjD2wGzd8EPZj/9ce2GjsmPjBMQlp+OAvQOsbU/B7twlXCksuW5ymugd6lDNVu5qa8nrmwPBPHvRj5txL0WGe17uB8uJcyCm9NNc2PiNaDCuO/BnIM2VqTH0QAQG7wtDWIGU3UU/hpzd7BkMt189YH0xNRxfu6+BztXWE09qNDUwqvFeJuz+OjLmuqvPAcSUVqL/3jCMPxaLgkotP9cWuQq2CWi21HkrYRPBcl7vDz6DxYvvUtgehC4Dx2O99UHy3mwUlpQjKS2L5+1uQ6fgwc6DYPFSP/Qc+TncvIOhVmsRGhkLp0teOOvqjqj4JGi0Gt43W2JNycjCeTcfnKP86u0fCpFEhrSsXNg5XsTBk0487xaWlF2dwEplFXwDI2BNfGH1pj3YsP0A7M9dRGZO/tXUYQDbisB+F/e+3B/vEdhSuQFsiUJB7xOJM64eOOP8Ozz8gikSiCHV6OGdIcImn2y8tiUQI2wjcCKuCAllcmRUKOGZIUQBGUIjB+2aFMg0/FqKUAFldQ0iyUBCroghJM9MESpxOCIfVtSnbVgB4krkqG0wEVnWTyMK5Go4JZZiuyAH2/2z4ZJcSm21cE0rx5vbA/G1SyoP6W2VdoGtq67Gz1a/4rl3yKtfGYhHXxuMed+vpwmVGu8wiLSyEot/3oqnug3HAwT4s91HYtXmfcjJK8Ci5RvQY+hUvNF/HNZQX2WiCt6GRYy9h0+j14jP0HXQREye+R0iY5PgcNELluNmovuQyRj3xWKERMRRGG1AChnXj7/sRL9R0ynMj8HTbw7jz9XVchIm/Hcp7M66Up9qyBVVzcAeMmkeNxKNToctew9jwJgZeLXfOAwYPR37D53kzx6YK8boA6HouEmAJ9b44eWNAgzbF4I17mnY7J2JsTbhCKB7zMGua6zH0chCjDwQjuNRhcgUV2HO2QR8fioGyy4k45Nj0Rj0ayi6bw9C503+mEReGpov5SmhntLFxZQyTD4ew6/3sg5Gr51B6Ethe9H5JJ5C3toRRAaQSyM1N7DWSLvAllUqMOm/3+IRyp8PdByILoPGw97RzXi1uThc8kR3Im/3vTKA8vhgTFuwCmHR8fjwk/m454W+sHj8LcxasppHAyZy6ns1Ebr7KBJYPPkO3hk2BX7kdSxnP8IixONv4lVKD27eQcgtLMLilVboQCnE4qnunC907Pcxnuv1Ic/LFs/3xFuDJ8DF3Q8lxN6X/LSVh/H7COzhUxcgv7gUh08747X+H8PiP2/hfhpz1pI1SCMDYqE2TaTARs8MvG0dhJcImC/PJOBQ2BXsC87D2MOReMsqANFFLO9fm3ipVo+vnZPxynoBTsYUIyBPije3BeOxn7wwZH8Ill9IwbHIYuwPKcAo2yg8tdoXm3yzIdNWwyOjAgPJEF7fLMBi5xQcofuORNI7El/oSpHlWeqz5+5gnE8qNY7WNrEwgWwCujVgl4nEGD5+Du5/eQD3EssJs3kYbEnCYxPx/tT5NPl9KZwPxMdkJL5BERg3Ywn+Q8SNedqCHzahqFTI76+kkLph1yF0IA+9nwyk39gZCAiLge2J83j2nZGUNoiNk/E4XfbDgePn0GXAeN73i33GYNa366jco/LvN3sMoChwH+Vmiw7dMG3+CuojGst/2cXBZqSx75jpPM30+eBzPEDR6YUeH3DPz6QS75o0clI0yT4WA/aGkhdL+NmAPAl606R/fDQaZUodP2eS9IoqTLSLgeW+cIQWSHAqrgQdNwfi9a2BOBhRwEE1SCPsyBheswrEUjIAn2wxka84dN0agN/IoCp118rCCrUOiwjwB1d5Y+jBCEQWta+UvQq2CejWgM28ZOjHswjs/hzsoZPn8clsSWKTMzDq00XkZX147v6IQqt3YLgBbCrVrgdbfh3Y/a8D+x4Cu9f7n8H2lDM+JRCfeXsk7iWPHDfzW6qj03gfrKY+Q1xg+qKfMHHWMqzauBt+gWFYtm4n7iOO8R8iiy/0/pAi0gR0oGNGIKd//RPnBc2lifKzAv3I26YQ4LkSNT97NqEUncmrv7+Ugiqz8ouJL7FkZhifnYhDaoUCa72y8MpGf6z1zCQAr93LQjYzhG4Urn9yz8APl9M48Cvc0qiku2YQTBjJc0wqQ0cac/qZRBQp2k7OmFiYg9xasCukMoz+ZCGVVgOIiQ+gyZ8GVw+B8Sqg1esRT+w5L7+YQqgAg6jMuYcm+SHyqMlzliMwIhbjzcBeRGAXmzxbqcTG3YdbANuZg33vS++ix/BPYW1jjyET5+Jx6oPdt9JqL5QqAxhM6ig1Vak0UNA5lpdLykT4ZuUWI9jDeTv2PI92oRKRfi+iEF9cLjK2Nghjws7JZQQsgfB7BlTVtZRbm7CVcibL47aR+aihcczlWHQxuhKJWn45nXv5FxT6e+wMgVeWgZOYREmAWgmyiXAFYZt/Dj48FImelC7c0g3zwAyN/TBh+dyFcnlXuvdnMhrddWO2VtoFNlsK/YrX1zTRlLOfeft9bNhpe3WRIywqgXL6EsxeuhYzSd+wnIgHyHuYFy2iCU9MzcSEL7/F468PMYC9YjNKyw2TUUkE7ZedLXm2GdjDPuEheBCFatbHA50GYu12G2L0et4HE8bCtRQKtTo99/QS6v/rH6+B3eHN4XjirRHUfigeoT46Ua63sXPk/MUk2tp6bCFG/CIRswPhhfycRFONBZST394ezL3YvNZmxrGeWDYL2fspFMeXKjHcJhKjj0QhmVg4F2MFUUil0xynJPTcFYJfQ/LQf08oxhDgieWG+3hZx4+ozqd53RGQQ+QsEDbG52iP3AA2K31uJSzs2529iNcHTOAhlK2YvTd+NnwCwqlkkWDb3iN4seco7rnP9xhFbHwEB+R1Av0IseO8gmJMJLAf6zIE9xDYXy5ejdwrhpcoIe9asGITb8sM5AawCaye5Nm7iTEPnzKfgBvCc/NCqqFLKb0wYV4dFpOI1VsPYOlaa2zddwx+IVH4bo017qf2DxORe/ndjzBt4Sr0oPzPvJyRtmET5kIQdI17MGBnnktEFyJH7hmGRYwEAmPMsSiMPBiFHGNYN0gTihVqTDsVj547g3EpvRzumRV4hxj1QjKOsipDbjdBGFeiwPsHI/HBoSiciC1GPwKdpYpsqbHPa5wPpUotphAP6EX3mJ6jPdIMbNOfErVGCorLMGX2MjxMofyhVw1eO2TCHPy4YQ/m/7ARr/QZy8F6hMLkY8x7iLl/Mu8HFJSWcVDYMZtkxuZ7j5yGg3YO5PFZ+O24A94eOpnKufeIvVuiH5VE/gT2QXOCRjn7ONXR85dv5MbEeENP6uPIaRdIqHZmuXv2d7/gBTK4DjTGBIoyl7wD8MP63bjPyMYHjp+FqIQUbCNDYM9670v98RA9y0J6dpHYQMTypBoMoRJq0P5wxBI4TLyyK9D31xB8djKO6nBTbm2iHMxKrny8RFFgDBG36FI5EbJCXp/vCMqDuta0WGVA0S1NSNEhEPPIu1mEGEKEjo0VXdJ8oUdNeNiEX6FU4o/Rh6MpWpgWpdouV8E2Ac3+Vqw1wmpLN+9A9Bs9AxY0gQ8SmAycF3uNRpfBEwmYDwjMYXjMCGg3yyk4fvYCb6uoqsKy9TvxNPN4uvbkW8PRnULziMlfoevgSVRCsVxqWHJ9l+peDra9E08XrKR6Z9hUXPAM4OP3H/UFH5/dz4zmi4UrMWba19wI2ILP/XRtk/VBpGblYenqHVTuUUlGqWDolK9QpVYT2RRi7PQlnKTdQ1Gqc7+PcJBYfn0DRYdCOYXkAMq78Xyhg8nlDBEvxYb+FoHgKxJUUR4vUmhwODwfA8gILJZ7YAEx53Sqr1me77I1CK6pwmbhnu2c7Q/NRycyDJavr1DfcxwS8cIvvjwnF8g10Nc1IL9Sg30hVyiXB+PhlT5YcjGNoodh8ak90m6wmejp3vOXfWmyFuM5ApflWIvne8Pi2V400f04+2ZsmYH3QvdRWE6elZ3PFvCbEBIVT5FgNvcoCwrlFk/1gMUzPfhK3MhPF+CJNyif0zUGoCAkmufTJ4kjWDzdHW9QueXuF0KETIXj5M0Dx36Jh1gN/mxP6ucdXnPfQyB3onp8EaWEXBpTUqnkkYC1t3iuFwZ+/CVkCkN+PO3qgbfJgFjFwIxp2PiZ8KKK4WhkAV7aIMA67ywiVIaIl1Ghwuen49GB6mNL8sQvKcxPOxmPSUejKO8G48UNvtjol001uhKT7KIppIcg5jpvZenhW9cUDrYj1cxs98w7S4QRNhHovNmfSroYzHZIxmT7OFjuD0Of3SF4Zo0vdgVdgeZqhGi7/CmwmTBSFpOYih0H7DB32QZMmP09xpCnTJj1PebS5G7df5zXryyHdyAAZyxZTUAF8RUz5plLyNs+mvkdRs9YigV03wnHS3D3Ifa7fhflYStY7TlCtW8+AsNjsOwXa8z7YQPWbjuAlIwcPr5Wq4O7bzBWbNyDKV+twIdkeB9/+R0RyI2woZSQV1TC71PTfafP/04pZgPmkwEwgqfSGkoYFmkOnnDCvOV0jXL/SurLmaoIt9QSrHLPRCDV1YY1ahD7boAgR0JgJGGkbRSF1ijMJa88GVtEpVQhNvtm8Xq8mMojayJV1gG5fJnUXCRUNx+lmnudZxZn7Ky0Yv16Z4nxDUWFUbbRBHwUPiUj2k7tT8YXYY1nBtXtMnITs2TeRrEwB9n0576tFfNh2Q4UI1eJaVmIik/hjLuYyh1Glgopv28n0BkZYjm+B3nRemsbTqKy8gqQnJmDFAqzQokMDVR/1tRU83VrsVTOlznZxoauWkf5WE5ln5yv4LEND3NhS6LpOVcoD6cigcZm7J5taJiEbYyoNFrep1gm40ul5tc1ZAyGa4Zx2fgKYvJi8sLquualDltdY4spyUTWUoUKiAm8JjRAW1dLXqvne98MPKm2mm+D1jfblGlCbX0dXzSRacnR6D7zTRK26MKiQgIx+QIK49U0fxp6fwmd1/Pn+IfAboswz2Ke/BkRs1d6j0aHLpbE4Gdh18ETfEdLqWz7ZnzrhU1QeyfpVu3Mr5uPY3beDEwOrNlndp852DcKu04lLTkB2xW7+b03l78e7GbP1sR3q3475oCxXyzm+9PPdhvONyF+ttoHXyp72EqaVkee8ide6u+Vv+g56f3ZWkEDeXYdRQxW/zeZRaL2SDOwTd/SuN1yPXA6GoetQR847shB70SlT8c+Y9CfmPdMqrlZro2KS4ZEKuN8wrRN+W+V22WYrB9W5dRSuNbrqzkfqa1he/PNQ317xcLco/8qsP9I2OpWXkEJXIkMfbt6GwYRq+5KbLznkE/x/qT5mP3NOuzcbw8vv1DkEaNmpVId5bg7S5qDxEAzKRO2/Mq8loGrUqmhUKj4Mi/7bPDm22NITJqBbfq+1d8tjJSxnbTQyATYUj09//uNGDpuLnoMnoq+w6dhzNSFmLdkHTZss4H9mYsIDI5GNkUGth3KyNudI0TjiKzVkLdqNDoOrFSm4FpZqeJ/1KEj4lZL5dVfEc3+UbANNtvcclmIz80vRmBoLE45XMaGHbaY881ajJmyEMPGzsKYyQswfe5KfL9qO7bsPILDds5wdfNDYEgMUlKzUVIqQiUxeH11zW0Jfe0RVkoxr6ymZ9Bq9UQ+1ZDJibWLZaioYCqnFMUAZl8+VHPgmQFcH66vjwJ/Vv4Vnv1HwiaNlVnpmbnw8Q/HibMXYf3rcfy4dg8WLN2AGfNW4rOZKzBj7irMX7Ieq9b8ih277WFzxBFnHNzh5hEE/8BoREQlIyklG1k5BSgoLENpWQX/MyWZnH27U803dljpxd6dhU+mfF7YMZ+bGrrGNlV0HBgVeSAr9Sq5ZypRIZZDKJKirFxCxlaB4mIhioqE/Df7zMYrF0roPhqTvJiNyTZp6ig3twSk6dxtB9s8VzNlX5VtSRobq1FVXYQKXSHqbsfgTVR/UknRSD8N9Lu1Peo19TSR5QiPiYePIAJnHT1x6Igrdu89jV+s9mPluj1YtXoffvhpF+luMoC9WLvJBlY7jmHX3pP47ZAjjti74uS5yzjv4oNLlwPh4RUCX79IMoxYBIXEIyw8ERERSYgkI4mKSUF0TBpiYtMQG880HbEJGYhPyERCUjYSk3PIkHKRkpaHtIx8ZGYVIje3BIUF5RASwCxEK3kOruGe+09Kq8FW15QjU+qCVLkv8rUlUNQUQ1NP4bJBTzlXS8DV8OP6Bh2BV4OqWgl91kBXJ4emTmFUOWoayUtqS1Gi9EF5dSVvJ9KmIV+dB1VdFWob2PgNqKG+mCE0NRFxoXa6ehVdU0JRWwZZbTGKNXGQ0nFNfTWkykIUivIRmxWOSxGn4RnmD0FgFH73DIHzBT+cdfLEybO/w+7kJRyzv4ijBPbxkxdgd+oSTpy5jNPn3HGOjMbR2QcuFwS4eDkAl92DyAhC4UNGIAiIISOIQ1hEIgc/PjETqal5yM4uQj6BWloqhlhSCYVSxbdZa2vqqGRiwN4ej7xd0mqwxbocJIhOoKgqEMEV7oiocEJSJbFkZSxEmmgUaFKQpkhCSVU0ytRRdP0MMpVhSJH5IE0moN+edM4FmVVJyJKdR3DRz0hRlVCU0CJJ4ojfi+2RJPNFAbWRVZNnaIpQS9FEoolFqNCO2sVCqElGnMQZEWJ3BJbsQaTEAaXaTGQpBEiuDEa8xA2BpdZIrBRAUUfGQnPNJp1tybIcrqF34+GXciXPoRI5z6GiCglEIlKWU8VSOi+jnCo3hPkq9kV/DS+D2L44y60s/DIvZSTqdoXYv1pYed16sLWpCCjagsiKk0iSXkC4NAKBIhfkSJ2RLHGBZ+kJBAidEVXhihjhAXiX7ERoxSk654F0qQcSKo7CuWA3fMtOI4LASxNaI0sjIk9XUX+ecC86glSxPRLFZxEq9kS2uoQ8WY840VFcyP0GsWQwBZpcMq4gpCtiEUxgZ0jtECV2RYQsCuESHwiKdyO6fCeylV7I1smNf9RzV5ikpqbCwgSyCWj2X1q0JDJ9PlJlXtBQKNXVZCNUZA/fcifK4enIVATCu3Q/EmQXyQMzkF/pCn+hDWKkl0gjUapORG6lC9xLj5A3eiCqfB/8i1YhQ11OgNQgWeoAhyubkaH0R6E6Gl4l9lDUG4hinvwCPAo3Ut8eSJZdhlfZIcTLBQgq2QJBiTVSK/3J053gT4YXJjwDQeFqhIhOo6ymZaP9XxSGra2tbevBpmICtZQ/DdJE+VcEVb2CjhrpvA7qOjF5aRWoQqScrYa6Xgo9ea2eQjH7An8dHavq5fSZ8nh9BZS1Rahma74k+gZiqTWl1AflYnUIkhTxV72ynvpSUd/6hipqJ4O8thxaOlbXlVP+LqdQT89NY6nqK/n5KsrnVXXSq+3vCohTlGLZsmUtg+0Rc+IG9Yo9BZ/Ys/CkY8+YU/CNc4Rf3Dk6f5qf96PPPvTZmz57x57jn33ps0/cGbNzDnTurKFtvDO1O8X786Hzgvjz8Iw+DKfwbTgfcejquIZ2TrxvpgLeL+vDyXCexval9uxZ2Hh+1A8bxyv2JO/b/B3+V9Qt8iiis32hr1VxvhIUFIzFixdfA9sENPsfidyCTv8jejn4HNxDnOAR7ECfz9xw/a62Tl397REU5wWxTIQqIpgRkVFwcXFtGeziItFdvZO1UIiiQioJy4T8O2tFxaV0LMT/AXNBeGDQZtLdAAAAAElFTkSuQmCC",
					FooterText = provider + " footer",
					FinishWizardText = provider + " finish wizard text",
					Email = provider + "@gmail.com",
					LeadingColor = "#ffffff",
					SecondoryColor = "#acacac",
					ConnectorsToEnable = "bank",
					MobilePhoneTextMessage = provider + "mobile phone text message"
				};
				wizardModel.Customer.IsWhiteLabel = true;
			}
			return View(wizardModel);
		} // Index

		#endregion action Index

		#region action EarnedPointsStr

		//[Ajax]
		//[HttpGet]
		//[ValidateJsonAntiForgeryToken]
		//[Transactional]
		//public JsonResult EarnedPointsStr() {
		//	var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		//	Customer oCustomer = oDBHelper == null ? null : oDBHelper.FindCustomerByEmail(User.Identity.Name.Trim());
		//	string sPoints = "";

		//	if (oCustomer != null)
		//		sPoints = string.Format("{0:N0}", oCustomer.LoyaltyPoints());

		//	return Json(new { EarnedPointsStr = sPoints }, JsonResultBehavior.AllowGet);
		//} // EarnedPointsStr

		#endregion action EarnedPointsStr
		
		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Vip()
		{
			var vipModel = new VipModel {VipEnabled = CurrentValues.Instance.VipEnabled};

			if (vipModel.VipEnabled)
			{
				vipModel.Ip = Request.ServerVariables["REMOTE_ADDR"];
				if (_context.Customer != null)
				{
					vipModel.VipEmail = _context.Customer.Name;
					vipModel.RequestedVip = _context.Customer.Vip;

					if (_context.Customer.PersonalInfo != null)
					{
						vipModel.VipFullName = _context.Customer.PersonalInfo.Fullname;
						vipModel.VipPhone = string.IsNullOrEmpty(_context.Customer.PersonalInfo.DaytimePhone)
							                    ? _context.Customer.PersonalInfo.MobilePhone
							                    : _context.Customer.PersonalInfo.DaytimePhone;
					}
				}
				else
				{
					var numOfRequests = _vipRequestRepository.CountRequestsPerIp(vipModel.Ip);
					if (numOfRequests >= CurrentValues.Instance.VipMaxRequests)
					{
						vipModel.RequestedVip = true;
					}
				}
			}

			return Json(vipModel, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Vip(VipModel model)
		{
			var customer = _context.Customer;
			var vip = new VipRequest
				{
					Customer = customer,
					Email = model.VipEmail,
					FullName = model.VipFullName,
					Ip = Request.ServerVariables["REMOTE_ADDR"],
					Phone = model.VipPhone,
					RequestDate = DateTime.UtcNow
				};
			_vipRequestRepository.SaveOrUpdate(vip);
			if (customer != null)
			{
				customer.Vip = true;
			}
			var c = new ServiceClient();
			c.Instance.VipRequest(customer != null ? customer.Id : 0, model.VipFullName, model.VipEmail, model.VipPhone);
			return Json(new {});
		}

		#endregion public

		#region private

		private readonly IEzbobWorkplaceContext _context;
		private readonly ISecurityQuestionRepository _questions;
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly ISession _session;
		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sourcesOfRepayment;
		private readonly IVipRequestRepository _vipRequestRepository;

		#endregion private
	} // class WizardController
} // namespace
