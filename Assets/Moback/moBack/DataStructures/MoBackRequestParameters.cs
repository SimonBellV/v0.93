//-----------------------------------------------------------------------
// <copyright file="MoBackRequestParameters.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using MoBack;
using MoBackInternal;

namespace MoBack
{
    /// <summary>
    /// Provides comparison functions for the user.
    /// </summary>
    public class MoBackRequestParameters
    {
        private Query query;
        private Pagination limitAndSkip;
        private List<KeyValuePair<string, bool>> sortBy;
        private List<string> returnColumns;
        private bool emptyParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRequestParameters"/> class.
        /// </summary>
        public MoBackRequestParameters()
        {
            emptyParameters = true;
        }

        #region Queries

        /// <summary>
        /// Finds whether a value is equal to the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters EqualTo(string column, object value)
        {
            IntegrateQuery(new MatchingQuery(column, value));
            return this;
        }

        /// <summary>
        /// Finds whether a value is less than the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters LessThan(string column, object value)
        {
            IntegrateQuery(new ComparisonQuery(column, value, "$lt"));
            return this;
        }

        /// <summary>
        /// Finds whether a value is greater than the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters GreaterThan(string column, object value)
        {
            IntegrateQuery(new ComparisonQuery(column, value, "$gt"));
            return this;
        }

        /// <summary>
        /// Finds whether a value is greater than or equal to the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters GreaterThanOrEqualTo(string column, object value)
        {
            IntegrateQuery(new ComparisonQuery(column, value, "$gte"));
            return this;
        }

        /// <summary>
        /// Finds whether a value is less than or equal to the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters LessThanOrEqualTo(string column, object value)
        {
            IntegrateQuery(new ComparisonQuery(column, value, "$lte"));
            return this;
        }

        /// <summary>
        /// Finds whether a value is not equal to the specified value.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="value"> A value. </param>
        public MoBackRequestParameters NotEqualTo(string column, object value)
        {
            IntegrateQuery(new ComparisonQuery(column, value, "$ne"));
            return this;
        }

        /// <summary>
        /// Finds if a value exists in a specified column.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        public MoBackRequestParameters HasValueFor(string column)
        {
            IntegrateQuery(new ComparisonQuery(column, true, "$exists"));
            return this;
        }

        /// <summary>
        /// Finds if a value does not exists in a specified column.
        /// </summary>
        /// <returns> A set of MoBackRequestParameters to be sent with a MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        public MoBackRequestParameters HasNoValueFor(string column)
        {
            IntegrateQuery(new ComparisonQuery(column, false, "$exists"));
            return this;
        }

        /// <summary>
        /// Finds anything equal to those included in the user-specified list of values.
        /// </summary>
        /// <returns> A set of parameters to be included in the MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="values"> A list of values. </param>
        public MoBackRequestParameters EqualToAnyOf(string column, params object[] values)
        {
            IntegrateQuery(new ComparisonQuery(column, values, "$in"));
            return this;
        }

        /// <summary>
        /// Finds anything not equal to those included in the user-specified list of values.
        /// </summary>
        /// <returns> A set of parameters to be included in the MoBackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="values"> A list of values. </param>
        public MoBackRequestParameters EqualToNoneOf(string column, params object[] values)
        {
            IntegrateQuery(new ComparisonQuery(column, values, "$nin"));
            return this;
        }

        /// <summary>
        /// Require that this AND all of the specified other constraints be satisfied.
        /// Note that for parameters that cannot be logically combined as such (skipping rows, rows limiting, columns limiting, sorting-by-column) 
        /// only the values from the calling object will be respected.
        /// </summary>
        /// <param name="additionalQueries"> Any addtional queries the user wishes to add into the parameters of a request. </param>
        /// <returns> A set of MoBackRequestParameters. </returns>
        public MoBackRequestParameters And(params MoBackRequestParameters[] additionalQueries)
        {
            Query[] queries = new Query[additionalQueries.Length + 1];
            queries[0] = this.query;
            for (int i = 0; i < additionalQueries.Length; i++) 
            {
                queries[i + 1] = additionalQueries[i].query;
            }
            query = new CompoundQuery("$and", queries);
            return this;
        }

        /// <summary>
        /// Require that this OR any of the specified other constraints be satisfied.
        /// Note that for parameters that cannot be logically combined as such (skipping rows, rows limiting, columns limiting, sorting-by-column) 
        /// only the values from the calling object will be respected.
        /// </summary>
        /// <param name="additionalQueries"> Any additional queries the user wishes to add to the request. </param>
        /// <returns> A set of parameterse to include with a request. </returns>
        public MoBackRequestParameters Or(params MoBackRequestParameters[] additionalQueries)
        {
            Query[] queries = new Query[additionalQueries.Length + 1];
            queries[0] = this.query;
            for (int i = 0; i < additionalQueries.Length; i++) 
            {
                queries[i + 1] = additionalQueries[i].query;
            }
            query = new CompoundQuery("$or", queries);
            return this;
        }
        
        #endregion

        #region Filter and Sort
        /// <summary>
        /// How the response to be received by the user upon completion of the request is sorted. Ascending by default.
        /// </summary>
        /// <returns> The parameter to be used in a MobackRequest. </returns>
        /// <param name="column"> A column. </param>
        /// <param name="ascending"> If set to <c>true</c> ascending; otherwise, false and descending. </param>
        public MoBackRequestParameters SortResponseBy(string column, bool ascending = true)
        {
            if (sortBy == null) 
            {
                emptyParameters = false;
                sortBy = new List<KeyValuePair<string, bool>>();
            }

            sortBy.Add(new KeyValuePair<string, bool>(column, ascending));
            return this;
        }

        /// <summary>
        /// Limits the response rows.
        /// </summary>
        /// <returns> The response rows as specified by the user. </returns>
        /// <param name="maxRows"> Max amount of rows to be returned to the user. </param>
        public MoBackRequestParameters LimitResponseRows(int maxRows)
        {
            if (limitAndSkip == null) 
            {
                emptyParameters = false;
                limitAndSkip = new Pagination();
            } 
            
            limitAndSkip.SetLimit(maxRows);
            return this;
        }

        /// <summary>
        /// Skips the specified response rows.
        /// </summary>
        /// <returns> The response rows as specified by the user. </returns>
        /// <param name="rowsToSkip"> Rows to skip. </param>
        public MoBackRequestParameters SkipResponseRows(int rowsToSkip)
        {
            if (limitAndSkip == null) 
            {
                emptyParameters = false;
                limitAndSkip = new Pagination();
            } 
            
            limitAndSkip.SetSkip(rowsToSkip);
            return this;
        }

        /// <summary>
        /// Filters through the columns as specified by the user.
        /// </summary>
        /// <returns> The columns to filter through. </returns>
        /// <param name="column"> A column. </param>
        public MoBackRequestParameters FilterColumns(string column)
        {
            if (returnColumns == null) 
            {
                emptyParameters = false;
                returnColumns = new List<string>();
            }
            
            returnColumns.Add(column);
            return this;
        }

        #endregion
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="MoBack.MoBackRequestParameters"/>.
        /// </summary>
        /// <returns> A MobackRequestParameter set as a string. </returns>
        public override string ToString()
        {
            if (emptyParameters) 
            {
                return string.Empty;
            }
            else 
            {
                bool needsConcat = false;
                string urlParameters = "?";

                if (query != null) 
                {
                    urlParameters += query;
                    needsConcat = true;
                }

                if (limitAndSkip != null) 
                {
                    if (needsConcat)
                    {
                        urlParameters += '&';
                    }
                    urlParameters += limitAndSkip;
                    needsConcat = true;
                }

                if (sortBy != null) 
                {
                    if (needsConcat)
                    {
                        urlParameters += '&';
                    }
                    urlParameters += "order=";
                    urlParameters += ((bool)sortBy[0].Value) ? sortBy[0].Key : "-" + sortBy[0].Key;
                    for (int i = 1; i < sortBy.Count; i++) 
                    {
                        urlParameters = urlParameters + ',' + (((bool)sortBy[i].Value) ? sortBy[i].Key : "-" + sortBy[i].Key);
                    }
                    needsConcat = true;
                }

                if (returnColumns != null) 
                {
                    if (needsConcat)
                    {
                        urlParameters += '&';
                    }
                    urlParameters += "keys=";
                    urlParameters += returnColumns[0];
                    for (int i = 1; i < returnColumns.Count; i++) 
                    {
                        urlParameters = urlParameters + ',' + returnColumns[i];
                    }
                    needsConcat = true;
                }

                return urlParameters;
            }
        }

        /// <summary>
        /// Integrates the query with another query.
        /// </summary>
        /// <param name="toIntegrate"> The query to integrate with. </param>
        private void IntegrateQuery(Query toIntegrate)
        {
            if (query == null) 
            {
                emptyParameters = false;
                query = toIntegrate;
            } 
            else 
            {
                query.SetConstraint(toIntegrate);
            }
        }
    }
}

// Ignore the StyleCop error for this.
namespace MoBackInternal
{
    /// <summary>
    /// A class to define the matching values query.
    /// </summary>
    public class MatchingQuery : Query
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoBackInternal.MatchingQuery"/> class.
        /// </summary>
        /// <param name="column"> A column. </param>
        /// <param name="value"> An object value. </param>
        public MatchingQuery(string column, object value)
        {
            constraints = new SimpleJSONClass();
            MoBackValueType type = MoBackInternal.MoBackUtils.ExtractMobackType(ref value);
            constraints.Add(column, MoBackInternal.MoBackUtils.MoBackTypedObjectToJSON(value, type));
        }
    }

    /// <summary>
    /// A class to define the comparison query.
    /// </summary>
    public class ComparisonQuery : Query
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoBackInternal.ComparisonQuery"/> class.
        /// </summary>
        /// <param name="column"> A column.</param>
        /// <param name="value"> An object value. </param>
        /// <param name="comparisonOperator"> A comparison operator as a string. </param>
        public ComparisonQuery(string column, object value, string comparisonOperator)
        {
            constraints = new SimpleJSONClass();
            
            SimpleJSONClass constraint = new SimpleJSONClass();
            MoBackValueType type = MoBackInternal.MoBackUtils.ExtractMobackType(ref value);
            constraint.Add(comparisonOperator, MoBackInternal.MoBackUtils.MoBackTypedObjectToJSON(value, type));
            
            constraints.Add(column, constraint);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBackInternal.ComparisonQuery"/> class.
        /// </summary>
        /// <param name="column"> A column.</param>
        /// <param name="values"> An array object values. </param>
        /// <param name="comparisonOperator"> A comparison operator as a string. </param>
        public ComparisonQuery(string column, object[] values, string comparisonOperator)
        {
            constraints = new SimpleJSONClass();

            SimpleJSONClass constraint = new SimpleJSONClass();
            SimpleJSONArray constraintValues = new SimpleJSONArray();
            for (int i = 0; i < values.Length; i++) 
            {
                object value = values[i];
                MoBackValueType type = MoBackInternal.MoBackUtils.ExtractMobackType(ref value);
                constraintValues.Add(MoBackInternal.MoBackUtils.MoBackTypedObjectToJSON(value, type));
            }
            constraint.Add(comparisonOperator, constraintValues);

            constraints.Add(column, constraint);
        }
    }

    /// <summary>
    /// A class to define a compound query.
    /// </summary>
    public class CompoundQuery : Query
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoBackInternal.CompoundQuery"/> class.
        /// </summary>
        /// <param name="compoundOperator"> A compound operator. </param>
        /// <param name="queriesToCompound"> An array of queries to compound. </param>
        public CompoundQuery(string compoundOperator, params Query[] queriesToCompound)
        {
            constraints = new SimpleJSONClass();

            SimpleJSONArray constraintValues = new SimpleJSONArray();
            foreach (Query query in queriesToCompound) 
            {
                constraintValues.Add(query.constraints);
            }

            constraints.Add(compoundOperator, constraintValues);
        }
    }

    /// <summary>
    /// A class that defines a Query.
    /// </summary>
    public abstract class Query
    {
        public SimpleJSONClass constraints;

        /// <summary>
        /// Sets the constraint.
        /// </summary>
        /// <param name="additionalConstraint"> An additional constraint. </param>
        public void SetConstraint(Query additionalConstraint)
        {
            if (additionalConstraint == this) 
            {
                return; // Else we'll have a crash while merging.
            }
            MergeQueries(constraints, additionalConstraint.constraints);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="MoBackInternal.Query"/>.
        /// </summary>
        /// <returns> Returns a Query as a string. </returns>
        public override string ToString()
        {
            return "where=" + WWW.EscapeURL(constraints.ToString());
        }
        
        /// <summary>
        /// Merge queries, potentially recursively.
        /// </summary>
        /// <param name="first">First query. This also contains the merged result after the merge.</param>
        /// <param name="second">Second query.</param>
        private void MergeQueries(SimpleJSONClass first, SimpleJSONClass second)
        {
            // Needs to merge various things correctly. For a (non-encompassing) example of the problem, take:
            // {"playerName":"Sean Plott","cheatMode":{"thisIsanObject":true}}
            // {"playerName":{"$exists":true}}
            // {"cheatMode":{"note":"ThisIsPartOfAnObjectNotAConstraint", "cheat":"noSecretCows"}}
            // {"rank":{"$gte":1,"$lte":10}} 
            // {"rank":{"$in":[2,3,6]}}
            // {"$and":[{"StudentName":”Mark Lee”},{"standard":4}]} 
            // Should turn into:
            // {"playerName":"Sean Plott","cheatMode":{"note":"ThisIsPartOfAnObjectNotAConstraint", "cheat":"noSecretCows"}, "rank":{"$gte":1,"$lte":10,"$in":[2,3,6]},
            // "$and":[{"StudentName":”Mark Lee”},{"standard":4}]}
            
            // Approach:
            // 0.When keywords don't conflict, just add both
            // 1.Two objects containing $keywords should be merged, following these rules recursively (relevantly, constants override constants, but arrays with $names are merged)
            // 2.Arrays with $names should be merged, by appending second array after first
            // 3.For two constant values (strings, ints, etc), in the case of a conflict the second value should override the first.
            //  ^Objects not containing $keywords and arrays without $names should be treated as constants
            // 5.When a conflict is not otherwise covered by these rules, just use the second of the two values
            // 6.Consider implementing in the future: Between an object containing $keywords and a constant match being provied, pick the constant match 
            //   (because an exact match effectively supercedes all constraints (except maybe mutually conflicting constraints which should axiomatically return nothing... 
            //   but not sure how that should be dealt with anyway? life would be simpler if we had an $eq operator exposed)
            foreach (KeyValuePair<string, SimpleJSONNode> node in second.dict) 
            {
                if (first.dict.ContainsKey(node.Key)) 
                {
                    SimpleJSONNode firstNode = first[node.Key];
                    SimpleJSONNode secondNode = node.Value;
                    
                    System.Type firstType = firstNode.valueType;
                    System.Type secondType = secondNode.valueType;

                    if (firstType == typeof(SimpleJSONClass) || secondType == typeof(SimpleJSONClass)) 
                    {
                        bool firstIsClassContainingKeywords = firstType == typeof(SimpleJSONClass) && JsonClassContainsKeywords((SimpleJSONClass)firstNode);
                        bool secondIsClassContainingKeywords = secondType == typeof(SimpleJSONClass) && JsonClassContainsKeywords((SimpleJSONClass)secondNode);
                        if (firstIsClassContainingKeywords && secondIsClassContainingKeywords) 
                        {
                            // Merge recursively
                            SimpleJSONClass firstObject = (SimpleJSONClass)firstNode;
                            SimpleJSONClass secondObject = (SimpleJSONClass)secondNode;
                            
                            MergeQueries(firstObject, secondObject);
                        } 
                        else 
                        {
                            // Newer value takes precedence
                            first.Add(node.Key, node.Value);
                        }
                    } 
                    else if (firstType == typeof(SimpleJSONArray) && secondType == typeof(SimpleJSONArray) && NameIsKeyword(node.Key)) 
                    {
                        // Merge arrays
                        SimpleJSONArray firstArray = (SimpleJSONArray)firstNode;
                        SimpleJSONArray secondArray = (SimpleJSONArray)secondNode;
                        
                        for (int i = 0; i < secondArray.Count; i++) 
                        {
                            firstArray.Add(secondArray[i]);
                        }
                    } 
                    else 
                    {
                        // Newer value takes precedence
                        first.Add(node.Key, node.Value);
                    }
                } 
                else 
                {
                    // No conflict, just add
                    first.Add(node.Key, node.Value);
                }
            }
        }

        /// <summary>
        /// Find if the JSON class contains any of the specified keywords.
        /// </summary>
        /// <returns><c>true</c>, if class contains keywords, <c>false</c> otherwise.</returns>
        /// <param name="jsonObject"> A JSON object. </param>
        private bool JsonClassContainsKeywords(SimpleJSONClass jsonObject)
        {
            foreach (KeyValuePair<string, SimpleJSONNode> jsonNode in jsonObject.dict) 
            {
                if (NameIsKeyword(jsonNode.Key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the keyword is contained in the name of an object.
        /// </summary>
        /// <returns><c>true</c>, if is keyword was named, <c>false</c> otherwise.</returns>
        /// <param name="name"> A string type name. </param>
        private bool NameIsKeyword(string name)
        {
            return name != null && name.Length > 0 && name[0] == '$';
        }
    }

    /// <summary>
    /// This is a class to display an amount of data. The user has the ability to se the limit, and to also set the amount to skip.
    /// </summary>
    public class Pagination
    {
        private int limit;
        private int skip;
        private bool hasLimit;
        private bool hasSkip;

        /// <summary>
        /// Sets the limit.
        /// </summary>
        /// <param name="limit"> The amount to set as the limit. </param>
        public void SetLimit(int limit)
        {
            this.limit = limit;
            hasLimit = true;
        }

        /// <summary>
        /// Sets the amount to skip.
        /// </summary>
        /// <param name="skip"> The amount to skip. </param>
        public void SetSkip(int skip)
        {
            this.skip = skip;
            hasSkip = true;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="MoBackInternal.Pagination"/>.
        /// </summary>
        /// <returns> The limit and amount to skip as a string. </returns>
        public override string ToString()
        {
            if (hasLimit) 
            {
                if (hasSkip) 
                {
                    return String.Format("limit={0}+skip={1}", limit, skip);
                }
                return "limit=" + limit;
            } 
            else if (hasSkip)
            {
                return "skip=" + skip;
            } 
            else 
            {
                return string.Empty;
            }
        }
    }
}