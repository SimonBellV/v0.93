//-----------------------------------------------------------------------
// <copyright file="MoBackColumn.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using MoBack;

namespace MoBack
{
    /// <summary>
    /// This class defines the column in MoBack, and gives access to the name and types that the column currently holds.
    /// </summary>
    public class MoBackColumn : System.IEquatable<MoBackColumn>
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value> The name of the MoBackColumn. </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value> The MoBackValueType. </value>
        public MoBackValueType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MoBackColumn class.
        /// </summary>
        /// <param name="columnName"> Column name. </param>
        /// <param name="columnType"> Column type. </param>
        public MoBackColumn(string columnName, MoBackValueType columnType)
        {
            Type = columnType;
            Name = columnName;
        }

        #region Equality Overrides
        /// <summary> Overrides the == operator to return if value a is equal to value b in name and type. </summary>
        /// <param name="a"> A MoBackColum value. </param>
        /// <param name="b"> Another MoBackColumn value. </param>
        /// <returns> Returns a bool value true if the two parameters are equal, otherwise false. </returns>
        public static bool operator ==(MoBackColumn a, MoBackColumn b)
        {
            return a.Equals(b.Name) && a.Type == b.Type;
        }

        /// <summary> Overrides the != operator. </summary>
        /// <param name="a"> A MoBackColum value. </param>
        /// <param name="b"> Another MoBackColumn value. </param>
        /// <returns> A bool value true if value a is not equal to value b, otherwise false. </returns>
        public static bool operator !=(MoBackColumn a, MoBackColumn b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified MoBackColumn is equal to the current MoBackColumn.
        /// </summary>
        /// <param name="other"> Another MoBackColumn to compare with the current MoBackColumn. </param>
        /// <returns> True if the specified MoBackColumn is equal to the current MoBackColumn; otherwise, false. </returns>
        public bool Equals(MoBackColumn other)
        {
            return this == other;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current MoBackColumn.
        /// </summary>
        /// <param name="obj"> An object to compare with the current MoBackColumn.</param>
        /// <returns> True if the specified object is equal to the current value. </returns>
        public override bool Equals(object obj)
        {
            MoBackColumn otherColumnDef = obj as MoBackColumn;
            if (otherColumnDef == null) 
            {
                return false;
            } 
            else 
            {
                return this == otherColumnDef;
            }
        }
        #endregion
    }
}