using System;
using System.Runtime.InteropServices;

namespace GoHttpsClientForCSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct GoByteSlice : IDisposable
	{
		public GoByteSlice(byte[] value)
		{
			if(value == null) { value = new byte[0]; }

			this.Buffer = Marshal.AllocHGlobal(value.Length + IntPtr.Size);

			Marshal.WriteInt32(this.Buffer, value.Length);

			var dataPtr = this.Buffer + IntPtr.Size;
			for (var i = 0; i < value.Length; i++)
			{
				Marshal.WriteByte(dataPtr + i, value[i]);
			}
		}
		
		public IntPtr Buffer;

		public void Dispose()
		{
			if (this.Buffer == IntPtr.Zero) { return; }

			Marshal.FreeHGlobal(this.Buffer);
			this.Buffer = IntPtr.Zero;
		}
	}
}
