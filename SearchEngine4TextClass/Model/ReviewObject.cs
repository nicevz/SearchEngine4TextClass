using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SearchEngine4TextClass.Model
{
    internal class ReviewObject
    {
        [JsonPropertyName("reviewerID")]
        public string ReviewerID { get; set; }
        [JsonPropertyName("asin")]
        public string ProductID { get; set; }
        [JsonPropertyName("reviewerName")]
        public string ReviewerName { get; set; }
        [JsonPropertyName("helpful")]
        public int[] Helpfulness { get; set; }
        [JsonPropertyName("reviewText")]
        public string ReviewText { get; set; }
        [JsonPropertyName("overall")]
        public float OverallRating { get; set; }
        [JsonPropertyName("summary")]
        public string SummaryText { get; set; }
        [JsonPropertyName("unixReviewTime")]
        public int UnixReviewTime { get; set; }
        [JsonPropertyName("reviewTime")]
        public string ReviewTime { get; set; }
    }
}
