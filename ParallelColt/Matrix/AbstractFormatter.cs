using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public abstract class AbstractFormatter : PersistentObject
	{
		public static String LEFT = "left";
		public static String RIGHT = "right";
		public static String CENTER = "center";
		public static String DECIMAL = "decimal";
		public static int DEFAULT_MIN_COLUMN_WIDTH = 1;
		public static String DEFAULT_COLUMN_SEPARATOR = " ";
		public static String DEFAULT_ROW_SEPARATOR = "\n";
		public static String DEFAULT_SLICE_SEPARATOR = "\n\n";

		protected String alignment = LEFT;
		protected String format = "%G";
		protected int minColumnWidth = DEFAULT_MIN_COLUMN_WIDTH;
		protected String columnSeparator = DEFAULT_COLUMN_SEPARATOR;
		protected String rowSeparator = DEFAULT_ROW_SEPARATOR;
		protected String sliceSeparator = DEFAULT_SLICE_SEPARATOR;
		protected bool printShape = true;
		private static String[] blanksCache;
		protected static FormerFactory factory = new FormerFactory();

		static AbstractFormatter()
		{
			setupBlanksCache();
		}

		protected AbstractFormatter()
		{

		}

		protected void align( String[][] strings )
		{
			int rows = strings.Length;
			int columns = 0;
			if ( rows > 0 )
				columns = strings[0].Length;
			int[] maxColWidth = new int[columns];
			int[] maxColLead = null;
			bool isDecimal = alignment.Equals( DECIMAL );
			if ( isDecimal )
				maxColLead = new int[columns];
			for ( int column = 0; column < columns; column++ )
			{
				int maxWidth = minColumnWidth;
				int maxLead = Int32.MinValue;
				for ( int row = 0; row < rows; row++ )
				{
					String s = strings[row][column];
					maxWidth = Math.Max( maxWidth, s.Length );
					if ( isDecimal )
						maxLead = Math.Max( maxLead, lead( s ) );
				}
				maxColWidth[column] = maxWidth;
				if ( isDecimal )
					maxColLead[column] = maxLead;
			}
			for ( int row = 0; row < rows; row++ )
			{
				alignRow( strings[row], maxColWidth, maxColLead );
			}
		}

		protected int alignmentCode( String alignment )
		{
			if ( alignment.Equals( LEFT ) )
				return -1;
			else if ( alignment.Equals( CENTER ) )
				return 0;
			else if ( alignment.Equals( RIGHT ) )
				return 1;
			else if ( alignment.Equals( DECIMAL ) )
				return 2;
			else
				throw new ArgumentException( "Unknown ALignment: " + alignment );
		}

		protected void alignRow( String[] row, int[] maxColWidth, int[] maxColLead )
		{
			StringBuilder s = new StringBuilder();
			int columns = row.Length;
			for ( int column = 0; column < columns; column++ )
			{
				s.Clear();
				String c = row[column];
				if ( alignment.Equals( RIGHT ) )
				{
					s.Append( blanks( maxColWidth[column] - s.Length ) );
					s.Append( c );
				}
				else if ( alignment.Equals( DECIMAL ) )
				{
					s.Append( blanks( maxColLead[column] - lead( c ) ) );
					s.Append( c );
					s.Append( blanks( maxColWidth[column] - s.Length ) );
				}
				else if ( alignment.Equals( CENTER ) )
				{
					s.Append( blanks( ( maxColWidth[column] - c.Length ) / 2 ) );
					s.Append( c );
					s.Append( blanks( maxColWidth[column] - s.Length ) );
				}
				else if ( alignment.Equals( LEFT ) )
				{
					s.Append( c );
					s.Append( blanks( maxColWidth[column - s.Length] ) );
				}
				else
				{
					throw new Exception( "Internal Error" );
				}
				row[column] = s.ToString();
			}
		}

		protected String blanks( int length )
		{
			if ( length < 0 )
			{
				length = 0;
			}
			if ( length < blanksCache.Length )
				return blanksCache[length];
			StringBuilder buf = new StringBuilder( length );
			for ( int k = 0; k < length; k++ )
			{
				buf.Append( ' ' );
			}
			return buf.ToString();
		}

		protected abstract String form( AbstractMatrix1D matrix, int index, IFormer formatter );

		protected abstract String[][] format( AbstractMatrix2D matrix );

		protected String[] formatRow( AbstractMatrix1D vector )
		{
			IFormer formatter = null;
			formatter = factory.create( this.format );

		}

		private static void setupBlanksCache()
		{

		}


	}
}
