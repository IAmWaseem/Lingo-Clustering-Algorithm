using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public abstract class AbstractMatrix : PersistentObject
	{
		protected bool isNoView = true;

		protected AbstractMatrix()
		{

		}

		public void ensureCapacity( int minNonZero )
		{
			// Nothing to do here
		}

		public bool isView()
		{
			return !this.isNoView;
		}


		public abstract long size();

		public void trimToSize()
		{

		}

	}
}
