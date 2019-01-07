using System;

namespace GoHttpsClientForCSharp
{
	public struct IntPtrWrapper : IDisposable
	{
		public IntPtr Ptr { get; set; }
		
		public void Dispose()
			=> GoHttpsClientWrapper.Free(this.Ptr);
	}
}
