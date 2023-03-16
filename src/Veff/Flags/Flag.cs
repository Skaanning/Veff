﻿using Veff.Internal;
using Veff.Internal.Responses;

namespace Veff.Flags
{
    public abstract class Flag
    {
        public readonly IVeffDbConnectionFactory VeffDbConnectionFactory;

        protected Flag(
            IVeffDbConnectionFactory veffDbConnectionFactory)
        {
            VeffDbConnectionFactory = veffDbConnectionFactory;
        }

        public abstract FeatureFlagViewModel AsViewModel();
    }
}