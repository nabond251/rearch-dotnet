// <copyright file="Mutation{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Represents a mutation.
/// </summary>
/// <typeparam name="T">Type of the mutated data.</typeparam>
/// <param name="State">
/// The current <see cref="AsyncValue{T}"/>? state of the mutation
/// (will be null when the mutation is inactive/cleared).
/// </param>
/// <param name="Mutate">
/// An <see cref="Action{T}"/> that triggers the mutation.
/// </param>
/// <param name="Clear">
/// An <see cref="Action"/> that stops and clears the mutation.
/// </param>
public record Mutation<T>(
    AsyncValue<T>? State,
    Action<Task<T>> Mutate,
    Action Clear);
