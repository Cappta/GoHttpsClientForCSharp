using System;
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

		public HttpResponseMessage Send(HttpRequestMessage httpRequestMessage)
		{
			using (var requestId = this.CreateRequest(httpRequestMessage))
			{
				GoHttpsClientWrapper.ThrowErrorIfAny(requestId);

				this.SetRequestHeaders(requestId, httpRequestMessage);

				var response = this.PerformRequest(requestId);
				response.RequestMessage = httpRequestMessage;

				return response;
			}
		}

		private ObjectId CreateRequest(HttpRequestMessage httpRequestMessage)
		{
			var method = httpRequestMessage.Method.Method;
			var url = httpRequestMessage.RequestUri.AbsoluteUri;
			var body = httpRequestMessage.Content?.ReadAsByteArrayAsync().Result;

			return GoHttpsClientWrapper.CreateRequest(method, url, body);
		}

		private void SetRequestHeaders(ObjectId requestId, HttpRequestMessage httpRequestMessage)
		{
			foreach (var header in httpRequestMessage.Headers)
			{
				GoHttpsClientWrapper.SetRequestHeader(requestId, header.Key, string.Join(",", header.Value));
			}

			if (httpRequestMessage.Content == null) { return; }
			foreach (var header in httpRequestMessage.Content.Headers)
			{
				GoHttpsClientWrapper.SetRequestHeader(requestId, header.Key, string.Join(",", header.Value));
			}
		}

		private HttpResponseMessage PerformRequest(ObjectId requestId)
		{
			using (var responseId = GoHttpsClientWrapper.PerformRequest(this.clientId, requestId))
			{
				GoHttpsClientWrapper.ThrowErrorIfAny(requestId);

				var httpResponseMessage = new HttpResponseMessage();
				httpResponseMessage.ReasonPhrase = GoHttpsClientWrapper.GetResponseStatus(responseId);
				var statusCode = GoHttpsClientWrapper.GetResponseStatusCode(responseId);
				httpResponseMessage.StatusCode = (HttpStatusCode)Math.Max(0, statusCode);
				this.SetResponseHeaders(responseId, httpResponseMessage);
				httpResponseMessage.Content = this.GetHttpContent(responseId);

				return httpResponseMessage;
			}
		}

		private void SetResponseHeaders(ObjectId responseId, HttpResponseMessage httpResponseMessage)
		{
			var headers = GoHttpsClientWrapper.GetResponseHeaderKeys(responseId);
			foreach (var header in headers)
			{
				var values = GoHttpsClientWrapper.GetResponseHeaderValue(responseId, header);

				try
				{
					httpResponseMessage.Headers.Add(header, string.Join(",", values));
				}
				catch (InvalidOperationException) { /* Ignore */ }
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
