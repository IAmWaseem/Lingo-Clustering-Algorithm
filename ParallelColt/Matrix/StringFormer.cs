using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public class StringFormer : IFormer
	{
		private String format;
		public StringFormer( String format )
		{
			this.format = format;
		}

		public string form( double value )
		{
			return String.Format( format, value );
		}

		public string form( float value )
		{
			return String.Format( format, value );
		}

		public string form( int value )
		{
			return String.Format( format, value );
		}

		public string form( long value )
		{
			return String.Format( format, value );
		}

		public string form( double[] value )
		{
			if ( value[0] == 0 && value[1] == 0 )
			{
				return "0";
			}
			if ( value[1] == 0 )
			{
				return String.Format( format, value[0] );
			}
			if ( value[1] < 0 )
			{
				return String.Format( format, value[0] ) + " - " + String.Format( format, -value[1] );
			}
			return String.Format( format, value[0] ) + " + " + String.Format( format, value[1] );
		}

		public string form( float[] value )
		{
			if ( value[0] == 0 && value[1] == 0 )
			{
				return "0";
			}
			if ( value[1] == 0 )
			{
				return String.Format( format, value[0] );
			}
			if ( value[1] < 0 )
			{
				return String.Format( format, value[0] ) + " - " + String.Format( format, -value[1] );
			}
			return String.Format( format, value[0] ) + " + " + String.Format( format, value[1] );
		}
	}
}
