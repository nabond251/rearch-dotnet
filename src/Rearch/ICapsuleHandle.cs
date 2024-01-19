// <copyright file="ICapsuleHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// The handle given to a <see cref="Capsule{T}"/> to build its data.
/// </summary>
public interface ICapsuleHandle : ICapsuleReader, ISideEffectRegistrar
{
}
