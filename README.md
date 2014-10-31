SkybrudLuceneTermExtractor
==========================

A small library to extract terms from an Umbraco Lucene Index. You can use it, if you for example, want to make a auto-suggestion-searchword function on your website (ala https://twitter.github.io/typeahead.js/).

The library creates a .json file with an array of terms in the /App_Data/ folder, which you can use with fx typeahead.js (https://twitter.github.io/typeahead.js/). The terms-array is made out of the lucene-fields you set up.


=== Example Controller to save .json file ===

```
namespace skybrud.Controllers
{
    public class SiteSearchApiController : UmbracoApiController
    {
        [HttpGet]
        public void SaveToJson(int rootId, int minLength)
        {
            var lte = new Skybrud.Lucene.Term.Extractor
            {
                IndexPath = "/App_Data/TEMP/ExamineIndexes/External/Index/",
                FieldNames = new string[] { "nodeName", "teaser" }
            };
            
            lte.SaveToJson(rootId, 5);
        }
    }
}
```
