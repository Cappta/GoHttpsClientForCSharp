using GoHttpsClientForCSharp.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace GoHttpsClientForCSharp
{
	internal static class GoHttpsClientWrapper
	{
		private const string DLL_PATH = "GoHttpsClient.dll";
		private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern ObjectId CreateClient();

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern bool SetClientTimeout(ObjectId clientId, int timeoutInSeconds);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		private static extern ObjectId CreateRequest(GoString method, GoString url, GoByteSlice body);
		public static ObjectId CreateRequest(string method, string url, byte[] body)
		{
			using (var goMethod = new GoString(method))
			{
				using (var goUrl = new GoString(url))
				{
					using (var goBody = new GoByteSlice(body))
					{
						return CreateRequest(goMethod, goUrl, goBody);
					}
				}
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		private static extern bool SetRequestHeader(ObjectId requestID, GoString key, GoString value);
		public static bool SetRequestHeader(ObjectId requestId, string key, string value)
		{
			using (var goKey = new GoString(key))
			{
				using (var goValue = new GoString(value))
				{
					return SetRequestHeader(requestId, goKey, goValue);
				}
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern ObjectId PerformRequest(ObjectId clientID, ObjectId requestID);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseStatus")]
		private static extern IntPtrWrapper GoGetResponseStatus(ObjectId responseID);
		public static string GetResponseStatus(ObjectId responseID)
		{
			using (var intPtrWrapper = GoGetResponseStatus(responseID))
			{
				return intPtrWrapper.Ptr.ToUTF8();
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern int GetResponseStatusCode(ObjectId responseID);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseHeaderKeys")]
		private static extern IntPtrWrapper GoGetResponseHeaderKeys(ObjectId responseID);
		public static string[] GetResponseHeaderKeys(ObjectId responseID)
		{
			using (var intPtrWrapper = GoGetResponseHeaderKeys(responseID))
			{
				return intPtrWrapper.Ptr.EnumerateUTF8().ToArray();
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseHeaderValue")]
		private static extern IntPtrWrapper GoGetResponseHeaderValue(ObjectId responseID, GoString key);
		public static string[] GetResponseHeaderValue(ObjectId responseID, string key)
		{
			using (var goKey = new GoString(key))
			{
				using (var intPtrWrapper = GoGetResponseHeaderValue(responseID, goKey))
				{
					return intPtrWrapper.Ptr.EnumerateUTF8().ToArray();
				}
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseBody")]
		private static extern IntPtrWrapper GoGetResponseBody(ObjectId responseID);
		public static byte[] GetResponseBody(ObjectId responseID)
		{
			using (var intPtrWrapper = GoGetResponseBody(responseID))
			{
				return intPtrWrapper.Ptr.ToByteArray();
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		private static extern IntPtrWrapper GetError(ObjectId errorId);
		public static void ThrowErrorIfAny(ObjectId errorId)
		{
			using (var intPtrWrapper = GetError(errorId))
			{
				var error = intPtrWrapper.Ptr.ToUTF8();
				if (string.IsNullOrEmpty(error)) { return; }
				
				if (error.Contains("net/http: request canceled (Client.Timeout exceeded while awaiting headers)")) { throw new TimeoutException(error); }
				throw new GolangException(error);
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern void ReleaseObject(ObjectId objectId);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern void Free(IntPtr pointer);
	}
}
