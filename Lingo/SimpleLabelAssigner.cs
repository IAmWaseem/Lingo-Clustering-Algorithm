using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public class SimpleLabelAssigner : ILabelAssigner
	{
		public void assignLabels( LingoProcessingContext context, DoubleMatrix2D stemCos, IntIntOpenHashMap filteredRowToStemIndex, DoubleMatrix2D phraseCos )
		{
			PreprocessingContext preprocessingContext = context.preprocessingContext;
			int firstPhraseIndex = preprocessingContext.allLabels.firstPhraseIndex;
			int[] labelsFeatureIndex = preprocessingContext.allLabels.featureIndex;
			int[] mostFrequentOriginalWordIndex = preprocessingContext.allStems.mostFrequentOriginalWordIndex;
			int desiredClusterCount = stemCos.columns();
			int[] candidateStemIndices = new int[desiredClusterCount];
			double[] candidateStemScores = new double[desiredClusterCount];

			int[] candidatePhraseIndices = new int[desiredClusterCount];
			for ( int i = 0; i < desiredClusterCount; i++ )
			{
				candidatePhraseIndices[i] = -1;
			}
			double[] candidatePhraseScores = new double[desiredClusterCount];
			MatrixUtils.maxInColumns( stemCos, candidateStemIndices, candidateStemScores,
				Functions.ABS );
			if ( phraseCos != null )
			{
				MatrixUtils.maxInColumns( phraseCos, candidatePhraseIndices,
					candidatePhraseScores, Functions.ABS );
			}
			int[] clusterLabelFeatureIndex = new int[desiredClusterCount];
			double [] clusterLabelScore = new double [desiredClusterCount];
			for (int i = 0; i < desiredClusterCount; i++)
			{
				int phraseFeatureIndex = candidatePhraseIndices[i];
				int stemIndex = filteredRowToStemIndex.get(candidateStemIndices[i]);

				double phraseScore = candidatePhraseScores[i];
				if (phraseFeatureIndex >= 0 && phraseScore > candidateStemScores[i])
				{
					clusterLabelFeatureIndex[i] = labelsFeatureIndex[phraseFeatureIndex
						+ firstPhraseIndex];
					clusterLabelScore[i] = phraseScore;
				}
				else
				{
					clusterLabelFeatureIndex[i] = mostFrequentOriginalWordIndex[stemIndex];
					clusterLabelScore[i] = candidateStemScores[i];
				}
			}
			context.clusterLabelFeatureIndex = clusterLabelFeatureIndex;
			context.clusterLabelScore = clusterLabelScore;
		}
	}
}
