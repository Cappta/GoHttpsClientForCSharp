using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
	}
}
