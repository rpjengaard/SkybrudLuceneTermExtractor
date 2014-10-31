SkybrudLuceneTermExtractor
==========================

Extract terms from Luceneindex (if you want to make a searchword helper)


=== Code Example ===
'''
var lte = new Skybrud.Lucene.Term.Extractor
    {
        IndexPath = "/App_Data/TEMP/ExamineIndexes/External/Index/",
        FieldNames = new string[] { "nodeName" }
    };

    var terms = lte.GetTerms(Model.Content.AncestorOrSelf(2).Id);
'''
