using BusinessObjects.Models;
using Repositories.Interfaces;
using Services.impl;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.impl
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly ConvosDbContext _context;
        public QuizRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
