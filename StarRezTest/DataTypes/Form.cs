using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public class Form
    {
        public List<Field> Fields;
        public Func<bool> Submit;
        public string Url;

        public Form()
        {
        }

        public Form(string url, Func<bool> submit, params Field[] fields)
        {
            Fields = new List<Field>(fields);
            Submit = submit;
            Url = url;
        }
    }
}
