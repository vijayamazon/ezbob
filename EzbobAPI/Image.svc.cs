namespace EzbobAPI {
	using System;
	using System.Drawing;
	using System.IO;
	using System.ServiceModel.Web;

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Customer" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Customer.svc or Customer.svc.cs at the Solution Explorer and start debugging.
	public class Image : IImage {
		public Stream GetImage(int width, int height) {
			Bitmap bitmap = new Bitmap(width, height);
			for (int i = 0; i < bitmap.Width; i++) {
				for (int j = 0; j < bitmap.Height; j++)
					bitmap.SetPixel(i, j, (Math.Abs(i - j) < 2) ? Color.Blue : Color.Yellow);
			}
			MemoryStream ms = new MemoryStream();
			bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
			ms.Position = 0;
			WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
			return ms;
		}
	}
}
