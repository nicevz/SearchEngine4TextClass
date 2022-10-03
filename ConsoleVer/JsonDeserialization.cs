using System.Collections.ObjectModel;
using System.Text.Json;

namespace ConsoleVer
{
    internal class JsonDeserialization
    {
        int ThreadCount;
        int TaskLoad;
        int RecordCount;
        int SamplingCount;
        List<string> JSONRecords;
        //string destJsonURL;

        ObservableCollection<string> SampledProductIDs = new ObservableCollection<string>();

        public JsonDeserialization(int ThreadCount, int SamplingCount, string FileLocation, string destJsonURL)
        {
            this.ThreadCount = ThreadCount;
            this.JSONRecords = File.ReadLines(FileLocation).ToList();
            this.RecordCount = this.JSONRecords.Count;
            this.TaskLoad = this.RecordCount / ThreadCount;
            this.SamplingCount = SamplingCount;
            //this.destJsonURL = destJsonURL;
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
        void SamplingPID()
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

        ObservableCollection<ReviewObject> GetReviewObjects(int StartIndex, int Count)
        {
            ObservableCollection<ReviewObject> result = new ObservableCollection<ReviewObject>();
            foreach (var record in JSONRecords.GetRange(StartIndex, Count))
            {
                ReviewObject tmpReviewObj = JsonSerializer.Deserialize<ReviewObject>(record);
                if (SampledProductIDs.Contains(tmpReviewObj.ProductID))
                {
                    result.Add(tmpReviewObj);
                }
            }
            return result;
        }
        async Task<IEnumerable<ObservableCollection<ReviewObject>>> TaskScheduler4GetRO()
        {
            List<Task<ObservableCollection<ReviewObject>>> tasks = new List<Task<ObservableCollection<ReviewObject>>>();
            for (int i = 0; i < ThreadCount - 1; i++)
            {
                var startingIndex = i * this.TaskLoad;
                tasks.Add(Task.Run(() => GetReviewObjects(startingIndex, this.TaskLoad)));
            }
            var lastStartingIndex = (ThreadCount - 1) * this.TaskLoad;
            var lastCount = RecordCount - (ThreadCount - 1) * this.TaskLoad;
            tasks.Add(Task.Run(() => GetReviewObjects(lastStartingIndex, lastCount)));
            return await Task.WhenAll(tasks);
        }

        public void GetSampledJSONFile()
        {
            SamplingPID();
            ObservableCollection<ReviewObject> allSampled = new ObservableCollection<ReviewObject>();
            foreach (var taskResult in TaskScheduler4GetRO().Result)
            {
                allSampled = new ObservableCollection<ReviewObject>(allSampled.Concat(taskResult));
            }
            List<string> strings = new List<string>();
            foreach (var a in allSampled)
            {
                strings.Add(JsonSerializer.Serialize(a));
            }
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "SampledJson.json"), strings.ToArray());
        }
    }
}
