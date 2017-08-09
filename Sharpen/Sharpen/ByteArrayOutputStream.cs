namespace Sharpen
{
	using System;
	using System.IO;

	public class ByteArrayOutputStream : OutputStream
	{
		public ByteArrayOutputStream ()
		{
			base.Wrapped = new MemoryStream ();
		}

		public ByteArrayOutputStream (int bufferSize)
		{
			base.Wrapped = new MemoryStream (bufferSize);
		}

		public long Size ()
		{
			return ((MemoryStream)base.Wrapped).Length;
		}

		public sbyte[] ToByteArray ()
		{
		    return Extensions.ConvertToByteArray(ToByteArrayInternal());
		}
        
        private  byte[] ToByteArrayInternal()
		{
			return ((MemoryStream)base.Wrapped).ToArray ();
		}
		
		public override void Close ()
		{
			// Closing a ByteArrayOutputStream has no effect.
		}
		
		public override string ToString ()
		{
            return System.Text.Encoding.UTF8.GetString(ToByteArrayInternal());
		}

	    public string ToString(string encoding)
	    {
            //  TODO: check encoding
            return System.Text.Encoding.GetEncoding(encoding).GetString(ToByteArrayInternal());
	    }
	}
}
