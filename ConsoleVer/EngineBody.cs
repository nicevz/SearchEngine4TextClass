using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace ConsoleVer
{
    /// <summary>
    /// This class serves as one engine
    /// Performing indexing and searching actions
    /// </summary>
    internal class EngineBody
    {
        /// <summary>
        /// colored printing method
        /// </summary>
        /// <param name="a">what you want to print</param>
        void mywrtL(string a)
        {
            Console.WriteLine("\u001b[35m" + "\u001b[1m" + a + "\u001b[0m");
        }

        int ThreadCount;
        int TaskLoad;
        int RecordCount;
        List<string> JSONRecords;

        LuceneVersion luceneVersion;
        Analyzer indexingAnalyzer;
        public IndexWriter writer;
        LuceneDirectory indexDir;
        IndexWriterConfig indexConfig;

        public EngineBody(int Threads, string FileLocation)
        {
            this.ThreadCount = Threads;
            this.JSONRecords = File.ReadLines(FileLocation).ToList();
            this.RecordCount = this.JSONRecords.Count;
            this.TaskLoad = this.RecordCount / ThreadCount;
        }

        /// <summary>
        /// Initialize lucene with:
        /// *Lucene version
        /// *Lucene path
        /// *Lucene analyzer for parsing text
        /// </summary>
        public void InitializeLucene()
        {
            // Specify the compatibility version we want
            luceneVersion = LuceneVersion.LUCENE_48;

            //Open the Directory using a Lucene Directory class
            string indexName = "index";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);

            indexDir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text 
            indexingAnalyzer = new EnglishAnalyzer(luceneVersion);

            //Create an index writer
            indexConfig = new IndexWriterConfig(luceneVersion, indexingAnalyzer);

            //Create/Overwrite index
            indexConfig.OpenMode = OpenMode.CREATE;
            writer = new IndexWriter(indexDir, indexConfig);
        }

        /// <summary>
        /// Main indexing method
        /// </summary>
        /// <param name="StartIndex">where this thread is supposed to start indexing</param>
        /// <param name="Count">how many records this thread is assigned</param>
        /// <returns></returns>
        ObservableCollection<ReviewObject> AddingDocuments(int StartIndex, int Count)
        {
            //Stopwatch obj for calculating indexing time
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ObservableCollection<ReviewObject> result = new ObservableCollection<ReviewObject>();
            foreach (var record in JSONRecords.GetRange(StartIndex, Count))
            {
                //Deserializing a review object
                ReviewObject tmpReviewObj = JsonSerializer.Deserialize<ReviewObject>(record);
                //Creating a doc for indexing
                Document doc = new Document();
                //Adding necessary attributes to this doc
                doc.Add(new TextField("ReviewText", tmpReviewObj.ReviewText, Field.Store.YES));
                doc.Add(new TextField("ProductID", tmpReviewObj.ProductID, Field.Store.YES));
                doc.Add(new TextField("Summary", tmpReviewObj.SummaryText, Field.Store.YES));
                doc.Add(new TextField("ReviewerID", tmpReviewObj.ReviewerID, Field.Store.YES));
                //some review has empty reviewer name, treat them as anonymous
                doc.Add(new TextField("ReviewerName", (tmpReviewObj.ReviewerName == null ? "Anonymous" : tmpReviewObj.ReviewerName), Field.Store.YES));
                doc.Add(new TextField("ReviewTime", tmpReviewObj.ReviewTime, Field.Store.YES));
                doc.Add(new Int32Field("UnixReviewTime", tmpReviewObj.UnixReviewTime, Field.Store.YES));
                doc.Add(new DoubleField("OverAll", tmpReviewObj.OverallRating, Field.Store.YES));
                //converting helpfulness to a single value
                doc.Add(new DoubleField("Helpfulness", tmpReviewObj.Helpfulness[0] / (tmpReviewObj.Helpfulness[1] == 0 ? 1 : tmpReviewObj.Helpfulness[1]), Field.Store.YES));
                writer.AddDocument(doc);
                result.Add(tmpReviewObj);
            }
            Console.WriteLine("=======ThreadID:{0}; TIME:{1}ms", Thread.CurrentThread.ManagedThreadId, sw.Elapsed.TotalMilliseconds);
            return result;
        }

        /// <summary>
        /// Multi-threading work dispatcher
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ObservableCollection<ReviewObject>>> TaskScheduler4AddingDocs()
        {
            //list of tasks
            List<Task<ObservableCollection<ReviewObject>>> tasks = new List<Task<ObservableCollection<ReviewObject>>>();
            //assigning same load to threads prior to the last one
            for (int i = 0; i < ThreadCount - 1; i++)
            {
                var startingIndex = i * this.TaskLoad;
                tasks.Add(Task.Run(() => AddingDocuments(startingIndex, this.TaskLoad)));
            }
            //last thread is responsible to all works left
            var lastStartingIndex = (ThreadCount - 1) * this.TaskLoad;
            var lastCount = RecordCount - (ThreadCount - 1) * this.TaskLoad;
            tasks.Add(Task.Run(() => AddingDocuments(lastStartingIndex, lastCount)));
            //return when all finished
            return await Task.WhenAll(tasks);
        }
        /// <summary>
        /// method for printing results
        /// </summary>
        /// <param name="i">the nth result</param>
        /// <param name="topDocs">all top docs</param>
        /// <param name="resultDoc">the specific doc result</param>
        void printingResults(int i,TopDocs topDocs,Document resultDoc)
        {
            Console.WriteLine("==============================================");
            mywrtL($"ReviewText of result {i + 1}:\n\t{resultDoc.Get("ReviewText")}");
            mywrtL($"ProductID of result {i + 1}:\n\t{resultDoc.Get("ProductID")}");
            mywrtL($"Summary of result {i + 1}:\n\t{resultDoc.Get("Summary")}");
            mywrtL($"ReviewerID of result {i + 1}:\n\t{resultDoc.Get("ReviewerID")}");
            mywrtL($"ReviewerName of result {i + 1}:\n\t{resultDoc.Get("ReviewerName")}");
            mywrtL($"ReviewTime of result {i + 1}:\n\t{resultDoc.Get("ReviewTime")}");
            mywrtL($"UnixReviewTime of result {i + 1}:\n\t{resultDoc.Get("UnixReviewTime")}");
            mywrtL($"OverAll of result {i + 1}:\n\t{resultDoc.Get("OverAll")}");
            mywrtL($"Helpfulness of result {i + 1}:\n\t{resultDoc.Get("Helpfulness")}");
            mywrtL($"Score of result {i + 1}:\n\t{topDocs.ScoreDocs[i].Score}");
            mywrtL($"DocID of result {i + 1}:\n\t{topDocs.ScoreDocs[i].Doc}");
            Console.WriteLine("==============================================");
        }
        /// <summary>
        /// Full text search on the review text field
        /// </summary>
        /// <param name="term">searching key word</param>
        /// <param name="num">top n results</param>
        public void FTS(string term,int num)
        {
            //define a searcher using reader from the index writer
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            //define a key word parser, on the review text field
            QueryParser parser = new QueryParser(luceneVersion, "ReviewText", indexingAnalyzer);
            //using the parsed key words to search for top docs
            TopDocs topDocs = searcher.Search(parser.Parse($"{term}"), num);

            mywrtL($"Matching results: {topDocs.TotalHits}");

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                //print it
                printingResults(i, topDocs, resultDoc);
            }
        }
        /// <summary>
        /// restrained full text search
        /// </summary>
        /// <param name="term1">key word for review text field</param>
        /// <param name="term2">pattern expected in the restraining field</param>
        /// <param name="term3">the required second field</param>
        /// <param name="num">top N results</param>
        public void RFTS(string term1,string term2,string term3,int num)
        {
            //define a searcher using reader from the index writer
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            //define a key word parser, on the review text field
            QueryParser parser = new QueryParser(luceneVersion, "ReviewText", indexingAnalyzer);
            Query query = parser.Parse($"+{term1} "+$"+{term3}:{term2}");
            //using the parsed key words to search for top docs
            TopDocs topDocs = searcher.Search(query, num);

            mywrtL($"Matching results: {topDocs.TotalHits}");

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                //print it
                printingResults(i, topDocs, resultDoc);
            }
        }
        /// <summary>
        /// search all related review objects given one required field
        /// </summary>
        /// <param name="term2">the required pattern</param>
        /// <param name="term3">the required field</param>
        /// <param name="num">top N results</param>
        public void RS(string term2, string term3, int num)
        {
            //define a searcher using reader from the index writer
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            //define a key word parser, on the review text field
            QueryParser parser = new QueryParser(luceneVersion, "ReviewText", indexingAnalyzer);
            Query query = parser.Parse($"+{term3}:{term2}");
            //using the parsed key words to search for top docs
            TopDocs topDocs = searcher.Search(query, num);

            mywrtL($"Matching results: {topDocs.TotalHits}");

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                //print it
                printingResults(i, topDocs, resultDoc);
            }
        }
        public static void stopWords()
        {
            foreach (var a in EnglishAnalyzer.DefaultStopSet)
            {
                Console.WriteLine(a);
            }
        }
    }
}
