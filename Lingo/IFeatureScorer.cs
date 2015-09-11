using Lingo.Text.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo
{
	public interface IFeatureScorer
	{
		public double[] getFeatureScores( LingoProcessingContext lingoContext );
	}
}
