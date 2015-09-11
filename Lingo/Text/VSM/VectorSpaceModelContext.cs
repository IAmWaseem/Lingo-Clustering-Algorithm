using Lingo.Text.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Text.VSM
{
	public class VectorSpaceModelContext
	{
		public PreprocessingContext preprocessingContext;
		public DoubleMatrix2D termDocumentMatrix;
		public DoubleMatrix2D termPhraseMatrix;
		public IntIntOpenHashMap stemToRowIndex;
		public VectorSpaceModelContext( PreprocessingContext preprocessingContext )
		{
			this.preprocessingContext = preprocessingContext;
		}
	}
}
