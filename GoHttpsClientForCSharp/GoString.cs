using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GoHttpsClientForCSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct GoString : IDisposable
	{
		public GoString(string value)
		{
			var utf8Value = Encoding.UTF8.GetBytes(value);

			this.Buffer = Marshal.AllocHGlobal(utf8Value.Length + 1);

			Marshal.Copy(utf8Value, 0, this.Buffer, utf8Value.Length);
			Marshal.WriteByte(this.Buffer + utf8Value.Length, 0);
		}

		public IntPtr Buffer;

		public void Dispose()
			=> Marshal.FreeHGlobal(this.Buffer);
	}
}
