using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ParallelColt
{
	public abstract class PersistentObject : ISerializable, ICloneable
	{

		public void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			// Serialize the Object
		}

		public object Clone()
		{
			try
			{
				return base.MemberwiseClone();
			}
			catch ( Exception ex )
			{
				Console.WriteLine( ex.ToString() );
			}
			return this;
		}
	}
}
