using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace GoHttpsClientForCSharp
{
	public class GoHttpsClient
	{
		private static readonly string[] BINARY_CONTENT_TYPES = new[] { "application/octet-stream" };

		private const string CONTENT_TYPE_HEADER = "Content-Type";

		private readonly ObjectId clientId;
		private TimeSpan timeout;

		public GoHttpsClient()
		{
			this.clientId = GoHttpsClientWrapper.CreateClient();
		}

		public GoHttpsClient(TimeSpan timeout) : this()
		{
			this.Timeout = timeout;
		}

		public TimeSpan Timeout
		{
			get { return this.timeout; }
			set
			{
				if (GoHttpsClientWrapper.SetClientTimeout(this.clientId, (int)value.TotalSeconds) == false) { throw new InvalidOperationException(); }

				this.timeout = value;
			}
		}

		public HttpResponseMessage Send(HttpRequestMessage request)
		{
			var method = request.Method.Method;
			var url = request.RequestUri.AbsoluteUri;
			var body = request.Content?.ReadAsByteArrayAsync().Result;

			using (var requestId = GoHttpsClientWrapper.CreateRequest(method, url, body))
			{
				GoHttpsClientWrapper.ThrowErrorIfAny(requestId);

				foreach (var header in request.Headers)
				{
					GoHttpsClientWrapper.SetRequestHeader(requestId, header.Key, string.Join(",", header.Value));
				}
				if (request.Content != null)
				{
					foreach (var header in request.Content.Headers)
					{
						GoHttpsClientWrapper.SetRequestHeader(requestId, header.Key, string.Join(",", header.Value));
					}
				}

				using (var responseId = GoHttpsClientWrapper.PerformRequest(this.clientId, requestId))
				{
					GoHttpsClientWrapper.ThrowErrorIfAny(responseId);

					var response = new HttpResponseMessage();
					response.ReasonPhrase = GoHttpsClientWrapper.GetResponseStatus(responseId);
					var statusCode = GoHttpsClientWrapper.GetResponseStatusCode(responseId);
					response.StatusCode = (HttpStatusCode)statusCode;
					response.RequestMessage = request;

					var headers = GoHttpsClientWrapper.GetResponseHeaderKeys(responseId);
					foreach (var header in headers)
					{
						var values = GoHttpsClientWrapper.GetResponseHeaderValue(responseId, header);

						try
						{
							response.Headers.Add(header, string.Join(",", values));
						}
						catch (InvalidOperationException) { /* Ignore */ }
					}

					response.Content = this.GetHttpContent(responseId);

					return response;
				}
			}
		}

		private HttpContent GetHttpContent(ObjectId responseId)
		{
			var responseBody = GoHttpsClientWrapper.GetResponseBody(responseId);
			if (responseBody == null || responseBody.Length == 0) { return null; }

			var contentType = GoHttpsClientWrapper.GetResponseHeaderValue(responseId, CONTENT_TYPE_HEADER).FirstOrDefault();
			if (string.IsNullOrWhiteSpace(contentType)) { return null; }

			if (BINARY_CONTENT_TYPES.Contains(contentType, StringComparer.OrdinalIgnoreCase)) { return new ByteArrayContent(responseBody); }

			var responseContent = Encoding.UTF8.GetString(responseBody);
			return new StringContent(responseContent);
		}
	}
}
