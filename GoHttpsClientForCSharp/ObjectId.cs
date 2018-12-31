using System;
using System.Runtime.InteropServices;

namespace GoHttpsClientForCSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ObjectId : IDisposable
	{
		public int Id;

		public void Dispose()
		{
			GoHttpsClientWrapper.ReleaseObject(this);
		}
	}
}
