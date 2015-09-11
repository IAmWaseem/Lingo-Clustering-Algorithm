using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public class LingoProcessingContext
	{
		public PreprocessingContext preprocessingContext;
		public VectorSpaceModelContext vsmContext;
		public ReducedVectorSpaceModelContext reducedVsmContext;
		int[] clusterLabelFeatureIndex;
		double[] clusterLabelScore;
		BitSet[] clusterDocuments;
		public LingoProcessingContext( ReducedVectorSpaceModelContext reducedVsmContext )
		{
			this.reducedVsmContext = reducedVsmContext;
			this.vsmContext = reducedVsmContext.vsmContext;
			this.preprocessingContext = vsmContext.preprocessingContext;
		}
	}
}
