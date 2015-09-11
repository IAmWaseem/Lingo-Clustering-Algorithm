using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Extensions
{
	public static class Int32Extensions
	{
		public static int bitCount( uint value )
		{
			int count = 0;
			while ( value != 0 )
			{
				count++;
				value &= value - 1;
			}
			return count;
		}
	}
}
