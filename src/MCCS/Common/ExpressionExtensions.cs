using System.Linq.Expressions;

namespace MCCS.Common
{
    /// <summary>
    /// Expression表达式链式拼接扩展方法
    /// </summary>
    public static class ExpressionExtensions
    { 

        /// <summary>
        /// 使用AndAlso连接两个表达式
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            // 获取左表达式的参数
            var parameter = left.Parameters[0];

            // 创建参数替换访问器，将右表达式的参数替换为左表达式的参数
            var visitor = new ParameterReplacer(parameter);
            var rightBody = visitor.Visit(right.Body);

            // 创建AndAlso表达式
            var andExpression = Expression.AndAlso(left.Body, rightBody);

            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }

        /// <summary>
        /// 使用OrElse连接两个表达式
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            var parameter = left.Parameters[0];
            var visitor = new ParameterReplacer(parameter);
            var rightBody = visitor.Visit(right.Body);

            var orExpression = Expression.OrElse(left.Body, rightBody);

            return Expression.Lambda<Func<T, bool>>(orExpression, parameter);
        }

        /// <summary>
        /// 对表达式进行非运算
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>(
            this Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var notExpression = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters[0]);
        }
    }

    /// <summary>
    /// 参数替换访问器，用于统一表达式参数
    /// </summary>
    internal class ParameterReplacer(ParameterExpression parameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(parameter);
        }
    }
}
