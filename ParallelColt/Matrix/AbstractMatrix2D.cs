using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt.Matrix
{
	public abstract class AbstractMatrix2D : AbstractMatrix
	{
		protected int columns;
		protected int rows;
		protected int rowStride;
		protected int columnStride;
		protected int rowZero;
		protected int columnZero;

		protected AbstractMatrix2D()
		{

		}

		protected int _columnOffset( int absRank )
		{
			return absRank;
		}

		protected int _columnRank( int rank )
		{
			return columnZero + rank * columnStride;
		}

		protected int _rowOffset( int absRank )
		{
			return absRank;
		}

		protected int _rowRank( int rank )
		{
			return rowZero + rank * rowStride;
		}

		protected void checkBox( int row, int column, int width, int height )
		{
			if ( column < 0 || width < 0 || column + width > columns || row < 0 || height < 0 || row + height > rows )
			{
				throw new IndexOutOfRangeException( ToString() + ", column:" + column + " row: " + row + "width: " + width + " height: " + height );
			}
		}

		protected void checkColumn( int column )
		{
			if ( column < 0 || column >= columns )
			{
				throw new IndexOutOfRangeException( ToString() + " column: " + column );
			}
		}

		protected void checkColumnIndexes( int[] indexes )
		{
			for ( int i = indexes.Length; --i >= 0; )
			{
				int index = indexes[i];
				if ( index < 0 || index >= columns )
				{
					checkColumn( index );
				}
			}
		}

		protected void checkRow( int row )
		{
			if ( row < 0 || row >= rows )
			{
				throw new IndexOutOfRangeException( ToString() + " row: " + row );
			}
		}

		protected void checkRowIndexes( int[] indexes )
		{
			for ( int i = indexes.Length; --i >= 0; )
			{
				int index = indexes[i];
				if ( index < 0 || index >= rows )
				{
					checkRow( index );
				}
			}
		}

		public void checkShape( AbstractMatrix2D B )
		{
			if ( columns != B.columns || rows != B.rows )
			{
				throw new ArgumentException( "Incompatible Dimensions. " + ToString() + " and " + B.ToString() );
			}
		}

		public void checkShape( AbstractMatrix2D B, AbstractMatrix2D C )
		{
			if ( columns != B.columns || rows != B.rows || columns != C.columns || rows != C.rows )
			{
				throw new ArgumentException( "Incompatible Dimensions. " + ToString() + " and " + B.ToString() + " and " + C.ToString() );
			}
		}

		public int Columns
		{
			get
			{
				return columns;
			}
		}

		public int ColumnStride
		{
			get
			{
				return columnStride;
			}
		}

		public long index( int row, int column )
		{
			return _rowOffset( _rowRank( row ) ) + _columnOffset( _columnRank( column ) );
		}


		public int Rows
		{
			get
			{
				return rows;
			}
		}

		public int RowStride
		{
			get
			{
				return rowStride;
			}
		}

		protected void setUp( int rows, int columns )
		{
			setUp( rows, columns, 0, 0, columns, 1 );
		}

		private void setUp( int rows, int columns, int rowZero, int columnZero, int rowStride, int columnStride )
		{
			if ( rows < 0 || columns < 0 )
			{
				throw new ArgumentException( "Negative Size" );
			}
			this.rows = rows;
			this.columns = columns;
			this.rowZero = rowZero;
			this.columnZero = columnZero;
			this.rowStride = rowStride;
			this.columnStride = columnStride;

			this.isNoView = true;

			if ( ( double )columns * rows > Int32.MaxValue )
			{
				throw new ArgumentException( "Matrix Too Large" );
			}
		}

		public override long size()
		{
			return rows * columns;
		}

		public String toStringShort()
		{
			// AbstractFormatter.shape(this);
		}
	}
}
