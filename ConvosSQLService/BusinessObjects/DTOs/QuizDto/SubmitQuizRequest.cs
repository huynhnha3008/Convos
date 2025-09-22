using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.QuizDto
{
    public class SubmitQuizRequest
    {
        public Guid quizId {  get; set; }
        public int score { get; set; }
    }
}
