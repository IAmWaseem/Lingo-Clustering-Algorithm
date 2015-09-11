using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingo.Core
{
	public class Document : ICloneable
	{
		public static String TITLE = "title";
		public static String SUMMARY = "snippet";
		public static String CONTENT_URL = "url";
		public static String CLICK_URL = "click-url";
		public static String THUMBNAIL_URL = "thumbnail-url";
		public static String SIZE = "size";
		public static String SCORE = "score";
		public static String SOURCES = "sources";
		public static String LANGUAGE = "language";
		public static Dictionary<String, Object> fields = new Dictionary<String, Object>();
		public static Dictionary<String, Object> fieldsView = new Dictionary<String, Object>();
		String id;
		private List<IDocumentSerializationListener> serializationListeners;

		public Document()
		{

		}
		public Document( String title ) : this(title, null)
		{
			
		}
		public Document( String title, String summary ) : this(title, summary, (String)null)
		{

		}
		public Document( String title, String summary, LanguageCode language ) : this(title, summary, null, language)
		{

		}

		public Document( String title, String summary, String contentUrl )
			: this( title, summary, contentUrl, null )
		{

		}

		public Document( String title, String summary, String contentUrl, LanguageCode language )
		{
			setField( TITLE, title );
			setField( SUMMARY, summary );
			if ( !String.IsNullOrWhiteSpace( contentUrl ) )
			{
				setField( CONTENT_URL, contentUrl );
			}
			if ( language != null )
			{
				setField( LANGUAGE, language );
			}
		}
		public Document( String title, String summary, String contentUrl, LanguageCode language, String id ) : this(title, summary, contentUrl, language)
		{
			this.id = id;
		}
		public int? GetId()
		{
			try
			{
				return id != null ? Int32.Parse( id ) : -1;
			}
			catch ( Exception )
			{
				
				throw;
			}
		}
		public object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
