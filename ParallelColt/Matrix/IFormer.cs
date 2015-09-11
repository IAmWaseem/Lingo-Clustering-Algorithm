using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public interface IFormer
	{
		String form( double value );
		String form( float value );
		String form( int value );
		String form( long value );
		String form( double[] value );
		String form( float[] value );
	}
}
