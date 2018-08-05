namespace EF.Core.Expansion.Dynamic
{
    /// <summary>
    /// 比较条件
    /// </summary>
    public class CompareCondition
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 比例方式
        /// </summary>
        public string Compare { get; set; }
    }
}
