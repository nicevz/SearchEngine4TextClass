using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.QueryParsers.Classic;
using static Lucene.Net.Util.Packed.PackedInt32s;
using Lucene.Net.Analysis.En;

namespace ConsoleVer
{
    internal class JsonDeserialization
    {
        int ThreadCount;
        int TaskLoad;
        int RecordCount;
        int SamplingCount;
        List<string> JSONRecords;
        public ObservableCollection<string> SampledProductIDs = new ObservableCollection<string>();

        public JsonDeserialization(int Threads, int SampCount, string FileLocation)
        {
            this.ThreadCount = Threads;
            this.JSONRecords = File.ReadLines(FileLocation).ToList();
            this.RecordCount = this.JSONRecords.Count;
            this.TaskLoad = this.RecordCount / ThreadCount;
            this.SamplingCount = SampCount;
        }
        ObservableCollection<string> GetProductIDs(int StartIndex, int Count)
        {
            Console.WriteLine("s:{0}; c:{1}", StartIndex, Count);
            ObservableCollection<string> tempCollection4PID = new ObservableCollection<string>();
            foreach (string JSONRecord in JSONRecords.GetRange(StartIndex, Count))
            {
                ReviewObject tempReviewObject = JsonSerializer.Deserialize<ReviewObject>(JSONRecord);
                if (!tempCollection4PID.Contains(tempReviewObject.ProductID))
                {
                    tempCollection4PID.Add(tempReviewObject.ProductID);
                }
            }
            return tempCollection4PID;
        }
        async Task<IEnumerable<ObservableCollection<string>>> TaskScheduler4GettingPID()
        {
            var taskPool = new List<Task<ObservableCollection<string>>>();
            for (int i = 0; i < ThreadCount - 1; i++)
            {
                var startIndex = i * this.TaskLoad;
                taskPool.Add(Task.Run(() => GetProductIDs(startIndex, this.TaskLoad)));
            }
            var lastStartIndex = (ThreadCount - 1) * this.TaskLoad;
            var lastRecordCount = RecordCount - (ThreadCount - 1) * this.TaskLoad;
            taskPool.Add(Task.Run(() => GetProductIDs(lastStartIndex, lastRecordCount)));
            return await Task.WhenAll(taskPool);
        }
        public void SamplingPID()
        {
            ObservableCollection<string> allPID = new ObservableCollection<string>();
            foreach (var taskResult in TaskScheduler4GettingPID().Result)
            {
                allPID = new ObservableCollection<string>(allPID.Concat(taskResult));
            }
            allPID = new ObservableCollection<string>(allPID.Distinct());
            HashSet<int> radomIndex = new HashSet<int>();
            Random rnd = new Random(DateTime.Now.Ticks.GetHashCode());
            while (true)
            {
                if (radomIndex.Count == SamplingCount)
                {
                    break;
                }
                radomIndex.Add(rnd.Next(0, allPID.Count - 1));
            }
            foreach (var inx in radomIndex)
            {
                this.SampledProductIDs.Add(allPID[inx]);
            }
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
        public ObservableCollection<ReviewObject> AddingDocuments(int StartIndex, int Count)
        {
            ObservableCollection<ReviewObject> result = new ObservableCollection<ReviewObject>();
            Console.WriteLine("=======s:{0}; c:{1}", StartIndex, Count);
            foreach (var record in JSONRecords.GetRange(StartIndex, Count))
            {
                ReviewObject tmpReviewObj = JsonSerializer.Deserialize<ReviewObject>(record);
                if (SampledProductIDs.Contains(tmpReviewObj.ProductID))
                {
                    Document doc = new Document();
                    doc.Add(new TextField("ReviewText", tmpReviewObj.ReviewText, Field.Store.YES));
                    doc.Add(new StringField("ProductID", tmpReviewObj.ProductID, Field.Store.YES));
                    doc.Add(new StringField("Summary", tmpReviewObj.SummaryText, Field.Store.YES));
                    writer.AddDocument(doc);
                    result.Add(tmpReviewObj);
                }
            }
            return result;
        }
        public async Task<IEnumerable<ObservableCollection<ReviewObject>>> TaskScheduler4AddingDocs()
        {
            List<Task<ObservableCollection<ReviewObject>>> tasks = new List<Task<ObservableCollection<ReviewObject>>>();
            for(int i = 0; i < ThreadCount-1; i++)
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
            Query query = parser.Parse("good game");
            TopDocs topDocs = searcher.Search(query, n: 30);         //indicate we want the first 3 results


            Console.WriteLine($"Matching results: {topDocs.TotalHits}");

            for (int i = 0; i < topDocs.TotalHits; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);

                string ProductID = resultDoc.Get("ProductID");
                Console.WriteLine($"ProductID of result {i + 1}: {ProductID}");
                string Summary = resultDoc.Get("Summary");
                Console.WriteLine($"Summary of result {i + 1}: {Summary}");
            }
        }
    }
}
