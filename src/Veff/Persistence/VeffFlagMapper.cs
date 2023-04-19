using System;
using Veff.Flags;

namespace Veff.Persistence;

public static class VeffFlagMapper
{
    public static Flag AsFlag(this IVeffFlag flag, IVeffDbConnectionFactory connectionFactory)
    {
        if (flag.Type.Equals(typeof(BooleanFlag).FullName))
            return new BooleanFlag(flag.Id, flag.Name, flag.Description, flag.Percent == 100, connectionFactory);

        if (flag.Type.Equals(typeof(PercentageFlag).FullName))
            return new PercentageFlag(flag.Id, flag.Name, flag.Description, flag.Percent, string.Join("", flag.Strings) ?? "", connectionFactory);

        if (flag.Type.Equals(typeof(StringContainsFlag).FullName))
            return new StringContainsFlag(flag.Id, flag.Name, flag.Description, flag.Strings, connectionFactory);

        if (flag.Type.Equals(typeof(StringEndsWithFlag).FullName))
            return new StringEndsWithFlag(flag.Id, flag.Name, flag.Description, flag.Strings, connectionFactory);
            
        if (flag.Type.Equals(typeof(StringStartsWithFlag).FullName))
            return new StringStartsWithFlag(flag.Id, flag.Name, flag.Description, flag.Strings, connectionFactory);
            
        if (flag.Type.Equals(typeof(StringEqualsFlag).FullName))
            return new StringEqualsFlag(flag.Id, flag.Name, flag.Description, flag.Strings, connectionFactory);

        throw new Exception($"Unknown type: {flag.Type}");
    }
}