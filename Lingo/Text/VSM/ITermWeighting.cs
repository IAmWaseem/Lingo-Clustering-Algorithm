using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Text.VSM
{
	public interface ITermWeighting
	{
		public double calculateTermWeight( int termFrequency, int documentFrequency,
		int documentCount );
	}
}
