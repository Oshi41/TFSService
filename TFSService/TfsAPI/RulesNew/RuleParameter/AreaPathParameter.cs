using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.RulesNew.RuleParameter
{
    public class AreaPathParameter : IRuleParameter
    {
        public AreaPathParameter(string areaPath)
        {
            AreaPath = areaPath;
        }

        public string AreaPath { get; }
    }
}
