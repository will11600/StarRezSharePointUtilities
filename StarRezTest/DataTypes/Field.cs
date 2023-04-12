using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public class Field
    {
        public By TargetElement { get; set; }
        public Func<string> Content { get; set; }

        public Field(By by, Func<string> func)
        {
            TargetElement = by;
            Content = func;
        }
    }
}
