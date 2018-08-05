namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 查询
    /// </summary>
    public class Query
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
