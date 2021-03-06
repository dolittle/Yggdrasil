﻿using Machine.Specifications;

namespace Yggdrasil.Specs.Binding.for_SelfBindingConvention
{
	public class when_asking_if_interface_can_be_bound : given.a_self_binding_convention
	{
		static bool result;

        Establish context = () => type_definition_mock.SetupGet(t => t.IsInterface).Returns(true);

		Because of = () => result = convention.CanBeBound(typeof(IServiceWithImplementation));

		It should_result_in_false = () => result.ShouldBeFalse();
	}
}