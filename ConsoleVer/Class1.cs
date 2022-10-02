using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Collections.ObjectModel;
using System.Text.Json;
using Lucene.Net.QueryParsers.Classic;

namespace ConsoleVer
{
    internal class Class1
    {
        int ThreadCount;
        int TaskLoad;
        int RecordCount;
        List<string> JSONRecords;
        public Class1(int Threads, string FileLocation)
        {
            this.ThreadCount = Threads;
            this.JSONRecords = File.ReadLines(FileLocation).ToList();
            this.RecordCount = this.JSONRecords.Count;
            this.TaskLoad = this.RecordCount / ThreadCount;
        }
        LuceneVersion luceneVersion;
        Analyzer indexingAnalyzer;
        IndexWriter writer;
        LuceneDirectory indexDir;
        IndexWriterConfig indexConfig;
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
            indexConfig.OpenMode = OpenMode.CREATE;                             // create/overwrite index
            writer = new IndexWriter(indexDir, indexConfig);
        }
        ObservableCollection<ReviewObject> AddingDocuments(int StartIndex, int Count)
        {
            ObservableCollection<ReviewObject> result = new ObservableCollection<ReviewObject>();
            //Console.WriteLine("=======s:{0}; c:{1}", StartIndex, Count);
            foreach (var record in JSONRecords.GetRange(StartIndex, Count))
            {
                ReviewObject tmpReviewObj = JsonSerializer.Deserialize<ReviewObject>(record);
                Document doc = new Document();
                doc.Add(new TextField("ReviewText", tmpReviewObj.ReviewText, Field.Store.YES));
                doc.Add(new TextField("ProductID", tmpReviewObj.ProductID, Field.Store.YES));
                doc.Add(new TextField("Summary", tmpReviewObj.SummaryText, Field.Store.YES));
                doc.Add(new TextField("ReviewerID", tmpReviewObj.ReviewerID, Field.Store.YES));
                doc.Add(new TextField("ReviewerName", (tmpReviewObj.ReviewerName == null ? "NULL" : tmpReviewObj.ReviewerName), Field.Store.YES));
                doc.Add(new TextField("ReviewTime", tmpReviewObj.ReviewTime, Field.Store.YES));
                doc.Add(new Int32Field("UnixReviewTime", tmpReviewObj.UnixReviewTime, Field.Store.YES));
                doc.Add(new DoubleField("OverAll", tmpReviewObj.OverallRating, Field.Store.YES));
                doc.Add(new DoubleField("Helpfulness", tmpReviewObj.Helpfulness[0] / (tmpReviewObj.Helpfulness[1] == 0 ? 1 : tmpReviewObj.Helpfulness[1]), Field.Store.YES));
                writer.AddDocument(doc);
                result.Add(tmpReviewObj);
            }
            return result;
        }
        public async Task<IEnumerable<ObservableCollection<ReviewObject>>> TaskScheduler4AddingDocs()
        {
            List<Task<ObservableCollection<ReviewObject>>> tasks = new List<Task<ObservableCollection<ReviewObject>>>();
            for (int i = 0; i < ThreadCount - 1; i++)
            {
                var startingIndex = i * this.TaskLoad;
                tasks.Add(Task.Run(() => AddingDocuments(startingIndex, this.TaskLoad)));
            }
            var lastStartingIndex = (ThreadCount - 1) * this.TaskLoad;
            var lastCount = RecordCount - (ThreadCount - 1) * this.TaskLoad;
            tasks.Add(Task.Run(() => AddingDocuments(lastStartingIndex, lastCount)));
            return await Task.WhenAll(tasks);
        }

        public void LuceneSample()
        {
            writer.Commit();
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);

            QueryParser parser = new QueryParser(luceneVersion, "ReviewText", indexingAnalyzer);
            Query query = parser.Parse("ProductID:B0029LJIFG");
            TopDocs topDocs = searcher.Search(query, 10);//indicate we want the first 3 results


            Console.WriteLine($"Matching results: {topDocs.TotalHits}");

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                Console.WriteLine("==============================================");
                Console.WriteLine($"ReviewText of result {i + 1}:\n\t{resultDoc.Get("ReviewText")}");
                Console.WriteLine($"ProductID of result {i + 1}:\n\t{resultDoc.Get("ProductID")}");
                Console.WriteLine($"Summary of result {i + 1}:\n\t{resultDoc.Get("Summary")}");
                Console.WriteLine($"ReviewerID of result {i + 1}:\n\t{resultDoc.Get("ReviewerID")}");
                Console.WriteLine($"ReviewerName of result {i + 1}:\n\t{resultDoc.Get("ReviewerName")}");
                Console.WriteLine($"ReviewTime of result {i + 1}:\n\t{resultDoc.Get("ReviewTime")}");
                Console.WriteLine($"UnixReviewTime of result {i + 1}:\n\t{resultDoc.Get("UnixReviewTime")}");
                Console.WriteLine($"OverAll of result {i + 1}:\n\t{resultDoc.Get("OverAll")}");
                Console.WriteLine($"Helpfulness of result {i + 1}:\n\t{resultDoc.Get("Helpfulness")}");
                Console.WriteLine("==============================================");
            }
        }
    }
}
