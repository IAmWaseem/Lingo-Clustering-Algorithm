using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public class FormerFactory
	{
		public IFormer create( String format )
		{
			return new StringFormer( format );
		}
	}
}
