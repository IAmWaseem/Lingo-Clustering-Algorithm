using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Text.Preprocessing
{
	public class LingoProcessingContext
	{
		private static String UNINITIALIZED = "[uninitialized\n";
		public String query;
		public List<Document> documents;
		public LanguageModel language;
	}
}
