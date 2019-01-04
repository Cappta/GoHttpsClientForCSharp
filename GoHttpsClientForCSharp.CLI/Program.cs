using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoHttpsClientForCSharp.CLI
{
	class Program
	{
		private static TimeSpan REFRESH_INTERVAL = TimeSpan.FromSeconds(1);

		static void Main(string[] args)
		{
			try
			{
				var goHttpsClient = new GoHttpsClient(TimeSpan.FromSeconds(10));
				var refreshStopwatch = Stopwatch.StartNew();

				var successCount = 0;
				var failureCount = 0;
				Parallel.For(0, 50, i =>
				{
					while (true)
					{
						var request = new HttpRequestMessage(HttpMethod.Get, "https://cert.paymentpage.com/");
						var response = goHttpsClient.Send(request);

						lock (goHttpsClient)
						{
							if (response.IsSuccessStatusCode) { successCount++; } else { failureCount++; }
							if (refreshStopwatch.Elapsed < REFRESH_INTERVAL) { continue; }

							Console.Clear();
							Console.WriteLine($"Successes: {successCount}/s");
							Console.WriteLine($"Failures : {failureCount}/s");

							refreshStopwatch.Restart();
							successCount = 0;
							failureCount = 0;
						}
					}
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
			Console.ReadLine();
		}
	}
}
