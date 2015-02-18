namespace AmazonToken
{
	using System;

	internal class Program
    {
        private static void Main(string[] args)
        {
			if (args.Length < 2) {
				Console.Write("Usage: AmazonToken.exe <min mp id> <[num of retries]>");
				return;
			}
	        int minMpId = 0;
	        int numOfRetries = 4;
	        if (args.Length >= 1) {
				int.TryParse(args[0], out minMpId);
			}

			if (args.Length >= 2) {
				int.TryParse(args[1], out numOfRetries);
			}
	        
			var linesRetriver = new BackFill(minMpId, numOfRetries);
            linesRetriver.Execute();
        }
    }
}