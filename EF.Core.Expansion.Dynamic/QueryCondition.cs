namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public class QueryCondition
    {
        /// <summary>
        /// 模拟查询关键字
        /// </summary>
        public Keyword Keyword { get; set; }

        /// <summary>
        /// 比较条件
        /// </summary>
        public Filter Filter { get; set; } 
    }
}
