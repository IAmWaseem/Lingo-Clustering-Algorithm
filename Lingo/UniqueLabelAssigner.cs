using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public class UniqueLabelAssigner : ILabelAssigner
	{
		public void assignLabels( LingoProcessingContext context, DoubleMatrix2D stemCos, IntIntOpenHashMap filteredRowToStemIndex, DoubleMatrix2D phraseCos )
		{
			PreprocessingContext preprocessingContext = context.preprocessingContext;
			int firstPhraseIndex = preprocessingContext.allLabels.firstPhraseIndex;
			int [] labelsFeatureIndex = preprocessingContext.allLabels.featureIndex;
			int [] mostFrequentOriginalWordIndex = preprocessingContext.allStems.mostFrequentOriginalWordIndex;
			int desiredClusterCount = stemCos.columns();

			IntArrayList clusterLabelFeatureIndex = new IntArrayList(
				desiredClusterCount);
			DoubleArrayList clusterLabelScore = new DoubleArrayList(desiredClusterCount);
			for (int label = 0; label < desiredClusterCount; label++)
			{
				Pair<int, int> stemMax = max(stemCos);
				Pair<int, int> phraseMax = max(phraseCos);

				if (stemMax == null && phraseMax == null)
				{
					break;
				}

				double stemScore = stemMax != null ? stemCos.getQuick(stemMax.objectA,
					stemMax.objectB) : -1;
				double phraseScore = phraseMax != null ? phraseCos.getQuick(
					phraseMax.objectA, phraseMax.objectB) : -1;

				if (phraseScore > stemScore)
				{
					phraseCos.viewRow(phraseMax.objectA).assign(0);
					phraseCos.viewColumn(phraseMax.objectB).assign(0);
					stemCos.viewColumn(phraseMax.objectB).assign(0);

					clusterLabelFeatureIndex.add(labelsFeatureIndex[phraseMax.objectA
						+ firstPhraseIndex]);
					clusterLabelScore.add(phraseScore);
				}
				else
				{
					stemCos.viewRow(stemMax.objectA).assign(0);
					stemCos.viewColumn(stemMax.objectB).assign(0);
					if (phraseCos != null)
					{
						phraseCos.viewColumn(stemMax.objectB).assign(0);
					}

					clusterLabelFeatureIndex
						.add(mostFrequentOriginalWordIndex[filteredRowToStemIndex
							.get(stemMax.objectA)]);
					clusterLabelScore.add(stemScore);
				}
			}

			context.clusterLabelFeatureIndex = clusterLabelFeatureIndex.toArray();
			context.clusterLabelScore = clusterLabelScore.toArray();
		}
		private Pair<int, int> max(DoubleMatrix2D matrix)
		{
			if (matrix == null)
			{
				return null;
			}

			int row = 0;
			int column = 0;
			double value = 0;

			for (int r = 0; r < matrix.rows(); r++)
			{
				for (int c = 0; c < matrix.columns(); c++)
				{
					double currentValue = matrix.getQuick(r, c);
					if (currentValue > value)
					{
						value = currentValue;
						row = r;
						column = c;
					}
				}
			}

			if (value > 0)
			{
				return new Pair<int, int>(row, column);
			}
			else
			{
				return null;
			}
		}
	}
}
