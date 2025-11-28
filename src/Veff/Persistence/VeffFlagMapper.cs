using System;
using System.Collections.Generic;
using System.Linq;
using Veff.Flags;
using Veff.Flags.Attributes;

namespace Veff.Persistence;

public static class VeffFlagMapper
{
    public static Flag AsFlag(this IVeffFlag flag, IVeffDbConnectionFactory connectionFactory,
        IEnumerable<Attribute> customAttributes)
    {
        var flagName = customAttributes.OfType<FlagNameAttribute>().FirstOrDefault();
        if (flag.Type.Equals(typeof(BooleanFlag).FullName))
        {
            return new BooleanFlag(flag.Id, FlagNameAttribute.GetFlagName(flag, flagName), flag.Description, flag.Percent == 100, connectionFactory);
        }

        if (flag.Type.Equals(typeof(PercentageFlag).FullName))
        {
            return new PercentageFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, flag.Percent, string.Join("", flag.Strings) ?? "", connectionFactory);
        }

        if (flag.Type.Equals(typeof(StringContainsFlag).FullName))
        {
            return new StringContainsFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, flag.Strings, connectionFactory);
        }

        if (flag.Type.Equals(typeof(StringEndsWithFlag).FullName))
        {
            return new StringEndsWithFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, flag.Strings, connectionFactory);
        }

        if (flag.Type.Equals(typeof(StringStartsWithFlag).FullName))
        {
            return new StringStartsWithFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, flag.Strings, connectionFactory);
        }
            
        if (flag.Type.Equals(typeof(StringEqualsFlag).FullName))
        {
            return new StringEqualsFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, flag.Strings, connectionFactory);
        }

        if (flag.Type.Equals(typeof(DateFlag).FullName))
        {
            var fromDate = flag.Strings?.FirstOrDefault() == null
                ? (DateTime?)null
                : DateTime.Parse(flag.Strings.First());
            var toDate = flag.Strings?.Skip(1).FirstOrDefault() == null
                ? (DateTime?)null
                : DateTime.Parse(flag.Strings.Skip(1).First());
            return new DateFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, fromDate, toDate, connectionFactory);
        }

        throw new Exception($"Unknown type: {flag.Type}");
    }
}