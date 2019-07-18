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