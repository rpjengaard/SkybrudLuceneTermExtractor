using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;

namespace Skybrud.Lucene.Term
{
    public class Extractor
    {
        #region Properties
        public string IndexPath { get; set; }
        public string[] FieldNames { get; set; }

        #endregion

        #region Constructors
        public Extractor()
        {
            FieldNames = new string[] {"nodeName"};
        }

        public Extractor(string indexPath)
        {
            IndexPath = indexPath;
            FieldNames = new string[] { "nodeName" };
        }

        public Extractor(string indexPath, string[] fieldNames)
        {
            IndexPath = indexPath;
            FieldNames = fieldNames;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get searchterms from examineindex
        /// </summary>
        /// <param name="rootId">Where to start from</param>
        /// <param name="minLength">Min length of words to find</param>
        /// <returns></returns>
        public TermExtractorWrapper GetTerms(int rootId = -1, int minLength = 5)
        {
            var debugTimeStart = DateTime.Now;
            var debugTimeEnd = DateTime.MinValue;
            var termsList = new List<global::Lucene.Net.Index.Term>();                       // data to return
            var indexPath = GetIndexPath();                         // path to index
            var directory = FSDirectory.Open(indexPath);            //
            var indexReader = IndexReader.Open(directory, true);    //
            var searcher = new IndexSearcher(indexReader);          //searcher for checking if the term is valid
            var analyzer = new global::Lucene.Net.Analysis.Standard.StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_29);
            
            foreach (var fieldName in FieldNames)
            {
                var tEnum = indexReader.Terms(new global::Lucene.Net.Index.Term(fieldName));

                do
                {
                    var t = tEnum.Term();

                    // filter in fieldname and minlength og term
                    if (t.Field() == fieldName && t.Text().Length >= minLength)
                    {
                        var searchPhrase = t.Text();
                        var parser = new global::Lucene.Net.QueryParsers.QueryParser(global::Lucene.Net.Util.Version.LUCENE_29, fieldName, analyzer);
                        var bq = new BooleanQuery();
                        var collector = TopScoreDocCollector.create(1, true);

                        var query = parser.Parse(searchPhrase);
                        bq.Add(query, BooleanClause.Occur.MUST);

                        searcher.Search(bq, collector);
                        var hits = collector.TopDocs().ScoreDocs;

                        if (hits.Any())
                        {
                            var docId = hits[0].doc;
                            var doc = searcher.Doc(docId);

                            if (doc.Get("path").Contains("," + rootId + ","))
                            {
                                termsList.Add(t);
                            }
                        }

                    }


                } while (tEnum.Next());
            }

            debugTimeEnd = DateTime.Now;

            return new TermExtractorWrapper
            {
                QueryTime = (debugTimeEnd - debugTimeStart).Milliseconds.ToString(),
                Terms = termsList
            };
        }

        /// <summary>
        /// Saving terms to .json file
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="rootId"></param>
        /// <param name="minLength"></param>
        public void SaveToJson(int rootId = -1, int minLength = 5)
        {
            var r = GetTerms(rootId, minLength);

            var jsonString = JsonConvert.SerializeObject(r.Terms.Select(x => x.Text()));

            var path = HttpContext.Current.Server.MapPath("/App_Data/SkybrudExamineTermsLists");

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            File.WriteAllText(string.Format("{0}/site_{1}.json", path, rootId), jsonString);
        }
        #endregion

        private DirectoryInfo GetIndexPath()
        {
            var path = string.IsNullOrEmpty(IndexPath)
                ? "/App_Data/TEMP/ExamineIndexes/External/Index/"
                : IndexPath;

            return new DirectoryInfo(HttpContext.Current.Server.MapPath(path));
        }

        public class TermExtractorWrapper
        {
            public string QueryTime { get; set; }
            public IEnumerable<global::Lucene.Net.Index.Term> Terms { get; set; }
        }
    }
}
