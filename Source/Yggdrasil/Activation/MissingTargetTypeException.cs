﻿using System;

namespace Yggdrasil.Activation
{
    public class MissingTargetTypeException : ArgumentException
    {
        public MissingTargetTypeException(Type service)
        {
            Service = service;
        }

        public Type Service { get; private set; }

    }
}
