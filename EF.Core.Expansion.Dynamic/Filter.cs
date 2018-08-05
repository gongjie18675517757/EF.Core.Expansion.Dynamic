using System.Collections.Generic;

namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 过滤条件
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 多条件关系
        /// </summary>
        public MultipleMark MultipleMark { get; set; }

        /// <summary>
        /// 子条件
        /// </summary>
        public IEnumerable<Filter> Filters { get; set; }

        /// <summary>
        /// 多条件[比较条件]
        /// </summary>
        public IEnumerable<CompareCondition> CompareConditions { get; set; }

        /// <summary>
        /// 匹配条件
        /// </summary>
        public IEnumerable<MatchCondition> MuchConditions { get; set; }
    }
}
