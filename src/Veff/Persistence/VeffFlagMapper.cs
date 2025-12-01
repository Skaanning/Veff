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
            var strings = ((VeffDbModel)flag).OriginalString?.Split(';');
            if (strings == null || strings.Length < 2)
            {
                strings = ["", ""];
            }
            
            var from = string.IsNullOrWhiteSpace(strings.FirstOrDefault()) ? (DateTime?)null : DateTime.Parse(strings.First());
            var to = string.IsNullOrWhiteSpace(strings.Skip(1).FirstOrDefault()) ? (DateTime?)null : DateTime.Parse(strings.Skip(1).First());
            return new DateFlag(flag.Id, FlagNameAttribute.GetFlagName(flag), flag.Description, from, to, connectionFactory);
        }

        throw new Exception($"Unknown type: {flag.Type}");
    }
}