using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.DTOs.QuizDto;

namespace BusinessObjects.DTOs
{
    public class CreateQuizByExelResponse
    {
        public string QuizTitle { get; set; }
        public string Description { get; set; }
        public string Skill { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
