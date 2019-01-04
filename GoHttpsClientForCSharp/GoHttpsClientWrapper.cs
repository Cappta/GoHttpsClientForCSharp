using GoHttpsClientForCSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
		public static bool SetRequestHeader(ObjectId requestID, string key, string value)
		{
			using (var goKey = new GoString(key))
			{
				using (var goValue = new GoString(value))
				{
					return SetRequestHeader(requestID, goKey, goValue);
				}
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern ObjectId PerformRequest(ObjectId clientID, ObjectId requestID);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseStatus")]
		private static extern IntPtr GoGetResponseStatus(ObjectId responseID);
		public static string GetResponseStatus(ObjectId responseID)
		{
			return PointerToString(GoGetResponseStatus(responseID));
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern int GetResponseStatusCode(ObjectId responseID);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseHeaderKeys")]
		private static extern IntPtr GoGetResponseHeaderKeys(ObjectId responseID);
		public static string[] GetResponseHeaderKeys(ObjectId responseID)
		{
			return EnumeratePointerStrings(GoGetResponseHeaderKeys(responseID)).ToArray();
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseHeaderValue")]
		private static extern IntPtr GoGetResponseHeaderValue(ObjectId responseID, GoString key);
		public static string[] GetResponseHeaderValue(ObjectId responseID, string key)
		{
			using (var goKey = new GoString(key))
			{
				return EnumeratePointerStrings(GoGetResponseHeaderValue(responseID, goKey)).ToArray();
			}
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION, EntryPoint = "GetResponseBody")]
		private static extern IntPtr GoGetResponseBody(ObjectId responseID);
		public static byte[] GetResponseBody(ObjectId responseID)
		{
			return PointerToByteArray(GoGetResponseBody(responseID));
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		private static extern IntPtr GetError(ObjectId errorId);
		public static void ThrowErrorIfAny(ObjectId errorId)
		{
			var error = PointerToString(GetError(errorId));
			if (string.IsNullOrEmpty(error)) { return; }

			if (error.Contains("net/http: request canceled (Client.Timeout exceeded while awaiting headers)")) { throw new TimeoutException(error); }
			throw new GolangException(error);
		}

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		public static extern void ReleaseObject(ObjectId objectId);

		[DllImport(DLL_PATH, CallingConvention = CALLING_CONVENTION)]
		private static extern void Free(IntPtr pointer);

		private static IEnumerable<string> EnumeratePointerStrings(IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { yield break; }
			
			for (var i = 0; Marshal.ReadIntPtr(pointer +  i) != IntPtr.Zero; i += IntPtr.Size)
			{
				yield return PointerToString(Marshal.ReadIntPtr(pointer + i));
			}
			Free(pointer);
		}

		private static string PointerToString(IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { return null; }

			var validBytes = pointer.ReadBytesUntilZero().ToArray();
			Free(pointer);
			return Encoding.UTF8.GetString(validBytes);
		}

		private static byte[] PointerToByteArray(IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { return null; }

			var length = Marshal.ReadInt32(pointer);
			var byteArray = new byte[length];

			var dataPtr = pointer + IntPtr.Size;
			for (var i=0; i<length; i++)
			{
				byteArray[i] = Marshal.ReadByte(dataPtr + i);
			}
			Free(pointer);
			return byteArray;
		}
	}
}
