using System;

namespace Veff.Exceptions;

internal class VeffConfigurationException(string msg) : Exception(msg);