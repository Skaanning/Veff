﻿using System;
using System.Linq;
using Veff.Flags;

namespace Veff.Persistence;

internal class VeffDbModel
{
    private readonly IVeffDbConnectionFactory _connectionFactory;
    private readonly string _randomSeed;

    public VeffDbModel(
        int id,
        string name,
        string description,
        int percent,
        string type,
        string strings,
        IVeffDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        Id = id;
        Name = name;
        Description = description;
        Percent = percent;
        Type = type;
        Strings = (strings ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray();
        _randomSeed = strings ?? "";
    }

    public string GetClassName()
        => Name.Split('.', 2)[0];

    public string GetPropertyName()
        => Name.Split('.', 2)[1];

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Percent { get; }
    public string Type { get; }
    public string[] Strings { get; }

    public Flag AsImpl()
    {
        if (Type.Equals(typeof(BooleanFlag).FullName))
            return new BooleanFlag(Id, Name, Description, Percent == 100, _connectionFactory);
            
        if (Type.Equals(typeof(PercentageFlag).FullName))
            return new PercentageFlag(Id, Name, Description, Percent, _randomSeed, _connectionFactory);

        if (Type.Equals(typeof(StringContainsFlag).FullName))
            return new StringContainsFlag(Id, Name, Description, Strings, _connectionFactory);

        if (Type.Equals(typeof(StringEndsWithFlag).FullName))
            return new StringEndsWithFlag(Id, Name, Description, Strings, _connectionFactory);
            
        if (Type.Equals(typeof(StringStartsWithFlag).FullName))
            return new StringStartsWithFlag(Id, Name, Description, Strings, _connectionFactory);
            
        if (Type.Equals(typeof(StringEqualsFlag).FullName))
            return new StringEqualsFlag(Id, Name, Description, Strings, _connectionFactory);

        throw new Exception($"Unknown type: {Type}");
    }
}