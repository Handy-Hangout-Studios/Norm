using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class BotCategoryAttribute : Attribute
    {
        public string Name { get; set; }

        public BotCategoryAttribute(string name)
        {
            this.Name = name;
        }
    }
}
