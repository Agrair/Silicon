using Newtonsoft.Json;

namespace Silicon.Models
{
    public class TriviaQuestion
    {
        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("correct_answer")]
        public string Answer { get; set; }

        [JsonProperty("incorrect_answers")]
        public string[] FalseAnswers { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }
    }
}
