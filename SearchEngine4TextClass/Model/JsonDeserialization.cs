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
        List<string> JSONRecords;
        ObservableCollection<string> SampledProductIDs = new ObservableCollection<string>();
        ObservableCollection<string> SamplingProducts(int StartIndex, int Count)
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
        async Task<IEnumerable<ObservableCollection<string>>> TaskScheduler4SamplingPID()
        {
            var taskPool = new List<Task<ObservableCollection<string>>>();
            for (int i = 0; i < ThreadCount - 1; i++)
            {
                var startIndex = i * TaskLoad;
                taskPool.Add(Task.Run(() => SamplingProducts(startIndex, TaskLoad));
            }
            var lastStartIndex = (ThreadCount - 1) * TaskLoad;
            var lastRecordCount = RecordCount - (ThreadCount - 1) * TaskLoad;
            taskPool.Add(Task.Run(() => SamplingProducts(lastStartIndex, lastRecordCount)));
            return await Task.WhenAll(taskPool);
        }
    }
}
