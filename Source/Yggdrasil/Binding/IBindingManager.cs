﻿using System;

namespace Yggdrasil.Binding
{
	public interface IBindingManager
	{
		bool HasBinding(Type type);
		IBinding GetBinding(Type type);
		void Register(IBinding binding);
	}
}