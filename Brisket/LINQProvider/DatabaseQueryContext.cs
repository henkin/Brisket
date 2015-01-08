using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Brisket.Tests;

namespace Brisket.LINQProvider
{
    public class DatabaseQueryContext
    {
        // Executes the expression tree that is passed to it. 
        internal static object Execute(Database database, Expression expression, bool IsEnumerable)
        {
            // The expression must represent a query over the data source. 
            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("No query over the data source was specified.");

            // Find the call to Where() and get the lambda expression predicate.
            InnermostWhereFinder whereFinder = new InnermostWhereFinder();
            MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);
            LambdaExpression lambdaExpression = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;

            // Send the lambda expression through the partial evaluator.
            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            // Get the place name(s) to query the Web service with.
            LocationFinder lf = new LocationFinder(lambdaExpression.Body);
            List<string> locations = lf.Locations;

            // wut
            //if (locations.Count == 0)
            //    throw new InvalidQueryException("You must specify at least one place name in your query.");

            





            // what you actullya want to do - 
            // 'where' property == value

            // look up in index - run lambda on index - 
            // individually compare each one to lambda, and 
            // get list of IDs. then pull all the nodes that correspond. 







            // Call the Web service and get the results.
            IEntity[] places = null;//database.GetAll(); ; //WebServiceHelper.GetPlacesFromTerraServer(locations);
            

            // Copy the IEnumerable places to an IQueryable.
            IQueryable<IEntity> queryablePlaces = places.AsQueryable<IEntity>();

            // Copy the expression tree that was passed in, changing only the first 
            // argument of the innermost MethodCallExpression.
            ExpressionTreeModifier treeCopier = new ExpressionTreeModifier(queryablePlaces);
            Expression newExpressionTree = treeCopier.Visit(expression);

            // This step creates an IQueryable that executes by replacing Queryable methods with Enumerable methods. 
            if (IsEnumerable)
                return queryablePlaces.Provider.CreateQuery(newExpressionTree);
            else
                return queryablePlaces.Provider.Execute(newExpressionTree);
        }

        private static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }
    }
}