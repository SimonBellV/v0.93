//-----------------------------------------------------------------------
// <copyright file="StaticsAndConfigs.cs" company="moBack">
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MoBackInternal 
{
    public enum HTTPMethod 
    { 
        POST = 0, 
        GET = 1, 
        PUT = 2, 
        DELETE = 3 
    };
}

namespace MoBack 
{
    public enum MoBackValueType 
    { 
        String, 
        File, 
        Number, 
        Boolean, 
        Array, 
        MoBackObject, 
        Date, 
        GeoPoint, 
        Pointer, 
        Relation 
    };
}