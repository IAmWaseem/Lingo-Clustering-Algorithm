using Lingo.Core;
using Lingo.Text.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public class LingoClusteringAlgorithm : ProcessingComponentBase, IClusteringAlgorithm
	{
		public string query = null;
		public List<Document> documents;
		public List<Cluster> clusters = null;
		public double scoreWeight = 0.0;
		public int desiredClusterCountBase = 30;
		public IPreprocessingPipeline preprocessingPipeline = new CompletePreprocessingPipeline();
		public TermDocumentMatrixBuilder matrixBuilder = new TermDocumentMatrixBuilder();
		public TermDocumentMatrixReducer matrixReducer = new TermDocumentMatrixReducer();
		public ClusterBuilder clusterBuilder = new ClusterBuilder();
		public LabelFormatter labelFormatter = new LabelFormatter();
		public MultilingualClustering multilingualClustering = new MultilingualClustering();
		public void process()
		{
			List<Document> originalDocuments = documents;
			//clusters = multilingualClustering.process(documents,
			//	new IMonolingualClusteringAlgorithm()
			//	{
			//		public List<Cluster> process(List<Document> documents,
			//			LanguageCode language)
			//		{
			//			LingoClusteringAlgorithm.this.documents = documents;
			//			LingoClusteringAlgorithm.this.cluster(language);
			//			return LingoClusteringAlgorithm.this.clusters;
			//		}
			//	});
			documents = originalDocuments;
		}

		private void cluster( LanguageCode language )
		{
			PreprocessingContext context = preprocessingPipeline.preprocess( documents, query, language );
			clusters = new List<Cluster>();
			if ( context.hasLabels() )
			{
				VectorSpaceModelContext vsmContext = new VectorSpaceModelContext( context );
				ReducedVectorSpaceModelContext reducedVsmContext = new ReducedVectorSpaceModelContext( vsmContext );
				LingoProcessingContext lingoContext = new LingoProcessingContext( reducedVsmContext );
				matrixBuilder.buildTermDocumentMatrix( vsmContext );
				matrixBuilder.buildTermPhraseMatrix( vsmContext );
				matrixReducer.reduce( reducedVsmContext, computeClusterCount( desiredClusterCountBase, documents.Count ) );
				clusterBuilder.buildLabels( lingoContext, matrixBuilder.termWeighting );
				clusterBuilder.assignDocuments( lingoContext );
				clusterBuilder.merge( lingoContext );
				int[] clusterLabelIndex = lingoContext.clusterLabelFeatureIndex;
				BitSet[] clusterDocuments = lingoContext.clusterDocuments;
				double[] clusterLabelScore = lingoContext.clusterLabelScore;
				for ( int i = 0; i < clusterLabelIndex.length; i++ )
				{
					Cluster cluster = new Cluster();
					int labelFeature = clusterLabelIndex[i];
					if ( labelFeature < 0 )
					{
						// Cluster removed during merging
						continue;
					}
					cluster.addPhrases( labelFormatter.format( context, labelFeature ) );
					cluster.setAttribute( Cluster.SCORE, clusterLabelScore[i] );
					BitSet bs = clusterDocuments[i];
					for ( int bit = bs.nextSetBit( 0 ); bit >= 0; bit = bs.nextSetBit( bit + 1 ) )
					{
						cluster.addDocuments( documents.ElementAt( bit ) );
					}
					clusters.Add( cluster );
				}
				Collections.Sort( clusters, Cluster.byReversedWeightedScoreAndSizeComparator( scoreWeight ) );
			}
			Cluster.appendOtherTopics( documents, clusters );
		}
		static int computeClusterCount( int desiredClusterCountBase, int documentCount )
		{
			return Math.Min( ( int )( ( desiredClusterCountBase / 10.0 ) * Math.Sqrt( documentCount ) ), documentCount );
		}

	}
}
