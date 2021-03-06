using System;

namespace Yggdrasil
{
	/// <summary>
	/// Discovers types based upon basetypes
	/// </summary>
	public interface ITypeDiscoverer
	{
        /// <summary>
        /// Find any types matching the name - not the fully qualified name
        /// </summary>
        /// <param name="name">Name of the type</param>
        /// <returns>An array of types found</returns>
        Type[] FindAnyByName(string name);

		/// <summary>
		/// Find a type by its full name
		/// </summary>
		/// <returns>Type found</returns>
        Type FindByFullName(string fullName);

		/// <summary>
		/// Find a single implementation of a basetype
		/// </summary>
		/// <param name="T">Basetype to find for</param>
		/// <returns>Type found</returns>
		/// <remarks>
		/// If the base type is an interface, it will look for any types implementing the interface.
		/// If it is a class, it will find anyone inheriting from that class
		/// </remarks>
		/// <exception cref="ArgumentException">If there is more than one instance found</exception>
		Type FindSingle(Type type);

		/// <summary>
		/// Find multiple implementations of a basetype
		/// </summary>
        /// <param name="type">Basetype to find for</param>
		/// <returns>All types implementing or inheriting from the given basetype</returns>
		/// <remarks>
		/// If the base type is an interface, it will look for any types implementing the interface.
		/// If it is a class, it will find anyone inheriting from that class
		/// </remarks>
		Type[] FindMultiple(Type type);
	}
}