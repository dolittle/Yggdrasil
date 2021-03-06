﻿using System;
using Yggdrasil.Activation;

namespace Yggdrasil.Binding
{
	public class StandardBinding : IBinding
	{
		public StandardBinding(Type service, Type target, IStrategy strategy)
		{
			Service = service;
			Target = target;
			Strategy = strategy;
		}

		public Type Service { get; private set; }
		public Type Target { get; private set; }
		public IStrategy Strategy { get; private set; }
		public IScope Scope { get; set; }
	}
}