﻿using System;

namespace Teclyn.Core.Api
{
    public class AggregateInfo
    {
        public Type AggregateType { get; }
        public Type ImplementationType { get; }
        public string CollectionName { get; }
        public Type DefaultFilterType { get; }
        public Type AccessControllerType { get; }

        public AggregateInfo(Type aggregateType, Type implementationType, string collectionName, Type accessControllerType, Type defaultFilterType)
        {
            this.AggregateType = aggregateType;
            this.ImplementationType = implementationType;
            this.CollectionName = collectionName;
            this.AccessControllerType = accessControllerType;
            this.DefaultFilterType = defaultFilterType;
        }
    }
}