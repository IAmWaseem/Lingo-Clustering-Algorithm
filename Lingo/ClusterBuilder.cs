using Lingo.Text.Preprocessing;
using Lingo.Text.VSM;
using Lingo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public class ClusterBuilder
	{
		public double phraseLabelBoost = 1.5;
		public double phraseLengthPenaltyStart = 8;
		public double phraseLengthPenaltyStop = 8;
		public double clusterMergingThreshold = 0.7;
		public IFeatureScorer featureScorer = null;
		public ILabelAssigner labelAssigner = new UniqueLabelAssigner();
		private LinearApproximation documentSizeCoefficients = new LinearApproximation(
			new double[] { 
				1.0, 1.5, 1.3, 0.9, 0.7, 0.6, 0.3, 0.05, 0.05, 0.05, 0.05
			}, 0.0, 1.0 );
		void buildLabels( LingoProcessingContext context, ITermWeighting termWeighting )
		{
			PreprocessingContext preprocessingContext = context.preprocessingContext;
			VectorSpaceModelContext vsmContext = context.vsmContext;
			DoubleMatrix2D reducedTdMatrix = context.reducedVsmContext.baseMatrix;
			int[] wordsStemIndex = preprocessingContext.allWords.stemIndex;
			int[] labelsFeatureIndex = preprocessingContext.allLabels.featureIndex;
			int[] mostFrequentOriginalWordIndex = preprocessingContext.allStems.mostFrequentOriginalWordIndex;
			int[][] phrasesWordIndices = preprocessingContext.allPhrases.wordIndices;
			BitSet[] labelsDocumentIndices = preprocessingContext.allLabels.documentIndices;
			int wordCount = preprocessingContext.allWords.image.length;
			int documentCount = preprocessingContext.documents.Size();
			BitSet oneWordCandidateStemIndices = new BitSet();
			for ( int i = 0; i < labelsFeatureIndex.Length; i++ )
			{
				int featureIndex = labelsFeatureIndex[i];
				if ( featureIndex >= wordCount )
				{
					break;
				}
				oneWordCandidateStemIndices.set( wordsStemIndex[featureIndex] );
			}
			IntIntOpenHashMap stemToRowIndex = vsmContext.stemToRowIndex;
			IntIntOpenHashMap filteredRowToStemIndex = new IntIntOpenHashMap();
			IntArrayList filteredRows = new IntArrayList();
			int filteredRowIndex = 0;
			foreach ( IntIntCursor it in stemToRowIndex )
			{
				if ( oneWordCandidateStemIndices.get( it.key ) )
				{
					filteredRowToStemIndex.put( filteredRowIndex++, it.key );
					filteredRows.add( it.value );
				}
			}
			double[] featureScores = featureScorer != null ? featureScorer.getFeatureScores( context ) : null;
			int[] wordLabelIndex = new int[wordCount];
			for ( int i = 0; i < wordCount; i++ )
			{
				wordLabelIndex[i] = -1;
			}
			for ( int i = 0; i < labelsFeatureIndex.Length; i++ )
			{
				int featureIndex = labelsFeatureIndex[i];
				if ( featureIndex < wordCount )
				{
					wordLabelIndex[featureIndex] = i;
				}
			}
			DoubleMatrix2D stemCos = reducedTdMatrix.viewSelection(
			filteredRows.toArray(), null ).copy();
			for ( int r = 0; r < stemCos.rows(); r++ )
			{
				int labelIndex = wordLabelIndex[mostFrequentOriginalWordIndex[filteredRowToStemIndex.get( r )]];
				double penalty = getDocumentCountPenalty( labelIndex, documentCount, labelsDocumentIndices );
				if ( featureScores != null )
				{
					penalty *= featureScores[labelIndex];
				}
				stemCos.viewRow( r ).assign( Functions.mult( penalty ) );
			}
			DoubleMatrix2D phraseMatrix = vsmContext.termPhraseMatrix;
			int firstPhraseIndex = preprocessingContext.allLabels.firstPhraseIndex;
			DoubleMatrix2D phraseCos = null;
			if ( phraseMatrix != null )
			{
				phraseCos = phraseMatrix.zMult( reducedTdMatrix, null, 1, 0, false, false );
				if ( phraseLengthPenaltyStop < phraseLengthPenaltyStart )
				{
					phraseLengthPenaltyStop = phraseLengthPenaltyStart;
				}
				double penaltyStep = 1.0 / ( phraseLengthPenaltyStop - phraseLengthPenaltyStart + 1 );
				for ( int row = 0; row < phraseCos.rows(); row++ )
				{
					int phraseFeature = labelsFeatureIndex[row + firstPhraseIndex];
					int[] phraseWordIndices = phrasesWordIndices[phraseFeature - wordCount];

					double penalty;
					if ( phraseWordIndices.Length >= phraseLengthPenaltyStop )
					{
						penalty = 0;
					}
					else
					{
						penalty = getDocumentCountPenalty( row + firstPhraseIndex,
							documentCount, labelsDocumentIndices );

						if ( phraseWordIndices.Length >= phraseLengthPenaltyStart )
						{
							penalty *= 1 - penaltyStep
								* ( phraseWordIndices.Length - phraseLengthPenaltyStart + 1 );
						}
						if ( featureScores != null )
						{
							penalty *= featureScores[row + firstPhraseIndex];
						}
					}
					phraseCos.viewRow( row ).assign( Functions.mult( penalty * phraseLabelBoost ) );
				}
			}
			labelAssigner.assignLabels( context, stemCos, filteredRowToStemIndex, phraseCos );
		}

		private double getDocumentCountPenalty( int labelIndex, int documentCount, BitSet[] labelsDocumentIndeces )
		{
			return documentSizeCoefficients.getValue( labelsDocumentIndeces[labelIndex].cardinality() / ( double )documentCount );
		}
		void assignDocuments( LingoProcessingContext context )
		{
			int[] clusterLabelFeatureIndex = context.clusterLabelFeatureIndex;
			BitSet[] clusterDocuments = new BitSet[clusterLabelFeatureIndex.Length];
			int[] labelsFeatureIndex = context.preprocessingContext.allLabels.featureIndex;
			BitSet[] documentIndices = context.preprocessingContext.allLabels.documentIndices;
			IntIntOpenHashMap featureValueToIndex = new IntIntOpenHashMap();
			for ( int i = 0; i < labelsFeatureIndex.Length; i++ )
			{
				featureValueToIndex.put( labelsFeatureIndex[i], i );
			}

			for ( int clusterIndex = 0; clusterIndex < clusterDocuments.Length; clusterIndex++ )
			{
				clusterDocuments[clusterIndex] = documentIndices[featureValueToIndex.get( clusterLabelFeatureIndex[clusterIndex] )];
			}

			context.clusterDocuments = clusterDocuments;
		}
		void merge( LingoProcessingContext context )
		{
			BitSet[] clusterDocuments = context.clusterDocuments;
			int[] clusterLabelFeatureIndex = context.clusterLabelFeatureIndex;
			double[] clusterLabelScore = context.clusterLabelScore;

			// Merge Code Issue. Check

			//List<IntArrayList> mergedClusters = GraphUtils.findCoherentSubgraphs(
			//clusterDocuments.length, new GraphUtils.IArcPredicate()
			//{
			//	private BitSet temp = new BitSet();

			//	public boolean isArcPresent(int clusterA, int clusterB)
			//	{
			//		temp.clear();
			//		int size;
			//		BitSet setA = clusterDocuments[clusterA];
			//		BitSet setB = clusterDocuments[clusterB];

			//		// Suitable for flat clustering
			//		// A small subgroup contained within a bigger group
			//		// will give small overlap ratio. Big ratios will
			//		// be produced only for balanced group sizes.
			//		if (setA.cardinality() < setB.cardinality())
			//		{
			//			// addAll == or
			//			// reiatinAll == and | intersect
			//			temp.or(setA);
			//			temp.intersect(setB);
			//			size = (int) setB.cardinality();
			//		}
			//		else
			//		{
			//			temp.or(setB);
			//			temp.intersect(setA);
			//			size = (int) setA.cardinality();
			//		}

			//		return temp.cardinality() / (double) size >= clusterMergingThreshold;
			//	}
			//}, true);


			foreach (IntArrayList clustersToMerge in mergedClusters)
			{
				int mergeBaseClusterIndex = -1;
				double maxScore = -1;

				int [] buf = clustersToMerge.buffer;
				int max = clustersToMerge.size();
				for (int i = 0; i < max; i++)
				{
					int clusterIndex = buf[i];
					if (clusterLabelScore[clusterIndex] > maxScore)
					{
						mergeBaseClusterIndex = clusterIndex;
						maxScore = clusterLabelScore[clusterIndex];
					}
				}

				for (int i = 0; i < max; i++)
				{
					int clusterIndex = buf[i];
					if (clusterIndex != mergeBaseClusterIndex)
					{
						clusterDocuments[mergeBaseClusterIndex].or(
							clusterDocuments[clusterIndex]);
						clusterLabelFeatureIndex[clusterIndex] = -1;
						clusterDocuments[clusterIndex] = null;
					}

				}
			}
		}
	}
}
