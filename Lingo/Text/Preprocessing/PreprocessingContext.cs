using Lingo.Core;
using Lingo.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Text.Preprocessing
{
	public class PreprocessingContext
	{
		private static String UNINITIALIZED = "[uninitialized]\n";
		public String query;
		public List<Document> documents;
		public LanguageModel language;
		private ObjectOpenHashSet<MutableCharArray> tokenCache = ObjectOpenHashSet.newInstance();
		public PreprocessingContext( LanguageModel languageModel, List<Document> documents,
				String query )
		{
			this.query = query;
			this.documents = documents;
			this.language = languageModel;
		}

		public class AllTokens
		{
			public char[][] image;
			public short[] type;
			public byte[] fieldIndex;
			public int[] documentIndex;
			public int[] wordIndex; public int[] suffixOrder;
			public int[] lcp;
			public override string ToString()
			{
				if ( image == null )
				{
					return UNINITIALIZED;
				}
				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "token" ).alignLeft();
				t.addColumn( "type" );
				t.addColumn( "fieldIndex" );
				t.addColumn( "=>field" ).alignLeft();
				t.addColumn( "docIdx" );
				t.addColumn( "wordIdx" );
				t.addColumn( "=>word" ).alignLeft();
				for ( int i = 0; i < image.Length; i++, t.nextRow() )
				{
					t.rowData(
						i,
						image[i] == null ? "<null>" : new String( image[i] ),
						type[i],
						fieldIndex[i],
						fieldIndex[i] >= 0 ? allFields.name[fieldIndex[i]] : null,
						documentIndex[i],
						wordIndex[i],
						wordIndex[i] >= 0 ? new String( allWords.image[wordIndex[i]] ) : null );
				}
				if ( suffixOrder != null )
				{
					t = new TabularOutput( sw );
					t.addColumn( "#" );
					t.addColumn( "sa" );
					t.addColumn( "lcp" );
					t.addColumn( "=>words" ).alignLeft();

					sw.Write( "\n" );
					StringBuilder suffixImage = new StringBuilder();
					for ( int i = 0; i < suffixOrder.Length; i++, t.nextRow() )
					{
						t.rowData(
							i,
							suffixOrder[i],
							lcp[i] );

						int windowLength = 5;
						for ( int j = suffixOrder[i], max = Math.Min( suffixOrder[i] + windowLength, wordIndex.Length ); j < max; )
						{
							suffixImage.Append(
								wordIndex[j] >= 0 ? new String( allWords.image[wordIndex[j]] ) : "|" ).Append( " " );
							if ( ++j == max && j != wordIndex.Length )
								suffixImage.Append( " [...]" );
						}
						t.rowData( suffixImage.ToString() );
						suffixImage.Clear();
					}
					sw.Write( "\n" );
				}

				t.flush();
				sw.Write( "\n" );
				return sw.ToString();
			}
		}

		public AllTokens allTokens = new AllTokens();
		public class AllFields
		{
			public String[] name;
			public override string ToString()
			{
				if ( name == null )
				{
					return UNINITIALIZED;
				}

				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "name" ).format( "%-10s" ).alignLeft();

				int i = 0;
				foreach ( String n in name )
				{
					t.rowData( i++, n ).nextRow();
				}

				t.flush();
				sw.Write( "\n" );
				return sw.ToString();
			}
		}

		public static AllFields allFields = new AllFields();

		public class AllWords
		{
			public char[][] image;
			public short[] type;
			public int[] tf;
			public int[][] tfByDocument;
			public int[] stemIndex;
			public byte[] fieldIndices;
			public override string ToString()
			{
				if ( image == null )
				{
					return UNINITIALIZED;
				}
				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "image" ).alignLeft();
				t.addColumn( "type" );
				t.addColumn( "tf" );
				t.addColumn( "tfByDocument" ).alignLeft();
				t.addColumn( "fieldIndices" );

				if ( stemIndex != null )
				{
					t.addColumn( "stemIndex" );
					t.addColumn( "=>stem" ).alignLeft();
				}

				for ( int i = 0; i < image.Length; i++, t.nextRow() )
				{
					t.rowData(
						i,
						image[i] == null ? "<null>" : new String( image[i] ),
						type[i],
						tf[i],
						SparseArray.sparseToString( tfByDocument[i] ) );

					t.rowData( Arrays.toString( toFieldIndexes( fieldIndices[i] ) ).replace( " ", "" ) );

					if ( stemIndex != null )
					{
						t.rowData( stemIndex[i] );
						t.rowData( new String( allStems.image[stemIndex[i]] ) );
					}
				}

				t.flush();
				sw.Write( "\n" );
				return sw.ToString();
			}
		}

		public static AllWords allWords = new AllWords();

		public class AllStems
		{
			public char[][] image;
			public int[] mostFrequentOriginalWordIndex;
			public int[] tf;
			public int[][] tfByDocument;
			public byte[] fieldIndices;
			public override string ToString()
			{
				if ( image == null )
				{
					return UNINITIALIZED;
				}

				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "stem" );
				t.addColumn( "mostFrqWord" );
				t.addColumn( "=>mostFrqWord" ).alignLeft();
				t.addColumn( "tf" );
				t.addColumn( "tfByDocument" ).alignLeft();
				t.addColumn( "fieldIndices" );

				for ( int i = 0; i < image.Length; i++, t.nextRow() )
				{
					t.rowData(
						i,
						image[i] == null ? "<null>" : new String( image[i] ),
						mostFrequentOriginalWordIndex[i],
						new String( allWords.image[mostFrequentOriginalWordIndex[i]] ),
						tf[i],
						SparseArray.sparseToString( tfByDocument[i] ),
						Arrays.toString( toFieldIndexes( fieldIndices[i] ) ).replace( " ", "" ) );
				}

				t.flush();
				sw.Write( "\n" );
				return sw.ToString();
			}
		}
		public static AllStems allStems = new AllStems();

		public class AllPhrases
		{
			public int[][] wordIndices;
			public int[] tf;
			public int[][] tfByDocument;
			public override string ToString()
			{
				if ( wordIndices == null )
				{
					return UNINITIALIZED;
				}

				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "wordIndices" );
				t.addColumn( "=>words" ).alignLeft();
				t.addColumn( "tf" );
				t.addColumn( "tfByDocument" ).alignLeft();

				for ( int i = 0; i < wordIndices.Length; i++, t.nextRow() )
				{
					t.rowData(
						i,
						Arrays.toString( wordIndices[i] ).replace( " ", "" ),
						getPhrase( i ),
						tf[i],
						SparseArray.sparseToString( tfByDocument[i] ) );
				}

				t.flush();
				sw.Write( "\n" );
				return sw.ToString();
			}
			public CharSequence getPhrase( int index )
			{
				StringBuilder sb = new StringBuilder();
				for ( int i = 0; i < wordIndices[index].Length; i++ )
				{
					if ( i > 0 ) sb.Append( " " );
					sb.Append( new String( allWords.image[wordIndices[index][i]] ) );
				}
				return sb;
			}
			public int size()
			{
				return wordIndices.Length;
			}
		}

		public static AllPhrases allPhrases = new AllPhrases();

		public class AllLabels
		{
			public int[] featureIndex;
			public BitSet[] documentIndices;
			public int firstPhraseIndex;
			public override string ToString()
			{
				if ( featureIndex == null )
					return UNINITIALIZED;

				StringWriter sw = new StringWriter();
				TabularOutput t = new TabularOutput( sw );
				t.flushEvery( Int32.MaxValue );
				t.addColumn( "#" );
				t.addColumn( "featureIdx" );
				t.addColumn( "=>feature" ).alignLeft();
				t.addColumn( "documentIdx" ).alignLeft();

				for ( int i = 0; i < featureIndex.Length; i++, t.nextRow() )
				{
					t.rowData(
						i,
						featureIndex[i],
						getLabel( i ),
						documentIndices != null ? documentIndices[i].toString().replace( " ", "" ) : "" );
				}

				t.flush();
				sw.Write( "\n" );
				return t.toString();
			}

			private CharSequence getLabel( int index )
			{
				int wordsSize = allWords.image.Length;
				if ( featureIndex[index] < wordsSize )
					return new String( allWords.image[featureIndex[index]] );
				else
					return allPhrases.getPhrase( featureIndex[index] - wordsSize );
			}
		}

		public static AllLabels allLabels = new AllLabels();

		public bool hasWords()
		{
			return allWords.image.Length > 0;
		}

		public bool hasLabels()
		{
			return allLabels.featureIndex != null && allLabels.featureIndex.Length > 0;
		}
		public String toString()
		{
			return "PreprocessingContext 0x" + this.GetHashCode().ToString() + "\n"
				+ "== Fields:\n" + allFields.ToString()
				+ "== Tokens:\n" + allTokens.ToString()
				+ "== Words:\n" + allWords.ToString()
				+ "== Stems:\n" + allStems.ToString()
				+ "== Phrases:\n" + allPhrases.ToString()
				+ "== Labels:\n" + allLabels.ToString();
		}
		private static int[][] bitsCache;
		static PreprocessingContext()
		{
			bitsCache = new int[0x100][];
			for ( int i = 0; i < 0x100; i++ )
			{
				bitsCache[i] = new int[Int32Extensions.bitCount( ( uint )( i & 0xFF ) )];
				for ( int v = 0, bit = 0, j = i & 0xff; j != 0; j = ( int )( ( uint )j >> 1 ), bit++ )
				{
					if ( ( j & 0x1 ) != 0 )
						bitsCache[i][v++] = bit;
				}
			}
		}
		public static int[] toFieldIndexes( byte b )
		{
			return bitsCache[b & 0xff];
		}
		public void preprocessingFinished()
		{
			this.tokenCache = null;
		}
		public char[] intern( MutableCharArray chs )
		{
			if ( tokenCache.contains( chs ) )
			{
				return tokenCache.lkey().getBuffer();
			}
			else
			{
				char[] tokenImage = new char[chs.length()];
				Array.Copy( chs.getBuffer(), chs.getStart(), tokenImage, 0, chs.length() );
				tokenCache.add( new MutableCharArray( tokenImage ) );
				return tokenImage;
			}
		}
	}
}
