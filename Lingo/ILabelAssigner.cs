using Lingo.Text.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public interface ILabelAssigner
	{
		public void assignLabels(LingoProcessingContext context, DoubleMatrix2D stemCos, IntIntOpenHashMap filteredRowToStemIndex, DoubleMatrix2D phraseCos);
	}
}
