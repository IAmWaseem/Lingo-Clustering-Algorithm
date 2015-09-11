using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Util
{
	public class LinearApproximation
	{
		private double[] points;
		private double argMin;
		private double argMax;
		private double step;
		private double[] arguments;
		public LinearApproximation( double[] points, double argMin, double argMax )
		{
			this.points = points;
			this.argMin = argMin;
			this.argMax = argMax;

			arguments = new double[points.Length];
			step = ( argMax - argMin ) / ( points.Length - 1 );
			for ( int i = 0; i < arguments.Length; i++ )
			{
				arguments[i] = argMin + step * i;
			}
		}
		public double getValue( double argument )
		{
			if ( points.Length == 1 )
			{
				return points[0];
			}

			if ( argument <= arguments[0] )
			{
				return points[0];
			}
			else if ( argument >= arguments[arguments.Length - 1] )
			{
				return points[points.Length - 1];
			}
			else
			{
				int bucket = ( int )( ( points.Length - 1 ) * ( argument - argMin ) / ( argMax - argMin ) );
				return points[bucket] + ( ( argument - arguments[bucket] ) / step )
					* ( points[bucket + 1] - points[bucket] );
			}
		}
	}
}
