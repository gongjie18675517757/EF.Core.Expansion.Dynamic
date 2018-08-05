namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 包含条件
    /// </summary>
    public class MatchCondition
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 比较类型[in,!in]
        /// </summary>
        public string Compare { get; set; }

        /// <summary>
        /// 要匹配的值
        /// </summary>
        public string[] Values { get; set; }
    }
}
