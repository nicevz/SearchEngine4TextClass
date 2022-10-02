using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SearchEngine4TextClass.Model
{
    internal class JsonDeserialization
    {
        int ThreadCount;
        int TaskLoad;
        int RecordCount;
        int SamplingCount;
        List<string> JSONRecords;
        public ObservableCollection<string> SampledProductIDs = new ObservableCollection<string>();

        JsonDeserialization(int Threads, int SampCount, string FileLocation)
        {
            this.ThreadCount = Threads;
            this.JSONRecords = File.ReadLines(FileLocation).ToList();
            this.RecordCount=this.JSONRecords.Count;
            this.TaskLoad = this.RecordCount / ThreadCount;
            this.SamplingCount = SampCount;
        }
        ObservableCollection<string> GetProductIDs(int StartIndex, int Count)
        {
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
                var startIndex = i * TaskLoad;
                taskPool.Add(Task.Run(() => GetProductIDs(startIndex, TaskLoad)));
            }
            var lastStartIndex = (ThreadCount - 1) * TaskLoad;
            var lastRecordCount = RecordCount - (ThreadCount - 1) * TaskLoad;
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
                if(radomIndex.Count == SamplingCount)
                {
                    break;
                }
                radomIndex.Add(rnd.Next(0, RecordCount - 1));
            }
            foreach(var inx in radomIndex)
            {
                this.SampledProductIDs.Add(allPID[inx]);
            }
        }
    }
}
