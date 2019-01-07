using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GoHttpsClientForCSharp.Extensions
{
	internal static class IntPtrExtensions
	{
		public static IEnumerable<byte> ReadBytesUntilZero(this IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { yield break; }

			for (var i = 0; Marshal.ReadByte(pointer + i) != 0; i++)
			{
				yield return Marshal.ReadByte(pointer + i);
			}
		}

		public static IEnumerable<string> EnumerateUTF8(this IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { yield break; }

			for (var i = 0; Marshal.ReadIntPtr(pointer + i) != IntPtr.Zero; i += IntPtr.Size)
			{
				yield return ToUTF8(Marshal.ReadIntPtr(pointer + i));
			}
		}

		public static string ToUTF8(this IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { return null; }

			var validBytes = pointer.ReadBytesUntilZero().ToArray();
			return Encoding.UTF8.GetString(validBytes);
		}

		public static byte[] ToByteArray(this IntPtr pointer)
		{
			if (pointer == IntPtr.Zero) { return null; }

			var length = Marshal.ReadInt32(pointer);
			var byteArray = new byte[length];

			var dataPtr = pointer + IntPtr.Size;
			for (var i = 0; i < length; i++)
			{
				byteArray[i] = Marshal.ReadByte(dataPtr + i);
			}
			return byteArray;
		}
	}
}
