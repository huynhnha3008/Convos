using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class HyphenNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c) && i > 0)
                {
                    builder.Append('-');
                }
                builder.Append(char.ToLower(c));
            }
            return builder.ToString();
        }
    }
}
