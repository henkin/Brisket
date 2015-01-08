using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Brisket.Tests;

namespace Brisket.LINQProvider
{
    internal class InnermostWhereFinder : ExpressionVisitor
    {
        private MethodCallExpression innermostWhereExpression;

        public MethodCallExpression GetInnermostWhere(Expression expression)
        {
            Visit(expression);
            return innermostWhereExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Where")
                innermostWhereExpression = expression;

            Visit(expression.Arguments[0]);

            return expression;
        }
    }

    internal class LocationFinder : ExpressionVisitor
    {
        private Expression expression;
        private List<string> locations;

        public LocationFinder(Expression exp)
        {
            this.expression = exp;
        }

        public List<string> Locations
        {
            get
            {
                if (locations == null)
                {
                    locations = new List<string>();
                    this.Visit(this.expression);
                }
                return this.locations;
            }
        }

        protected override Expression VisitBinary(BinaryExpression be)
        {
            if (be.NodeType == ExpressionType.Equal)
            {
                if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(IEntity), "Name"))
                {
                    locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(IEntity), "Name"));
                    return be;
                }
                else if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(IEntity), "State"))
                {
                    locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(IEntity), "State"));
                    return be;
                }
                else
                return base.VisitBinary(be);
            }
            else
                return base.VisitBinary(be);
        }
    }

    internal class ExpressionTreeModifier : ExpressionVisitor
    {
        private IQueryable<IEntity> queryablePlaces;

        internal ExpressionTreeModifier(IQueryable<IEntity> places)
        {
            this.queryablePlaces = places;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            // Replace the constant QueryableTerraServerData arg with the queryable Place collection. 
            if (c.Type == typeof(QueryableDatabaseData<IEntity>))
                return Expression.Constant(this.queryablePlaces);
            else
                return c;
        }
    }
}