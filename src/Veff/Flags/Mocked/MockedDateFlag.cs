using System;

namespace Veff.Flags.Mocked;

public class MockedDateFlag(DateTime? from, DateTime? to) : DateFlag(-1, "mocked Name", "mocked description", from, to, null!);