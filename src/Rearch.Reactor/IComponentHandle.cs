// <copyright file="IComponentHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor;

using MauiReactor;

/// <summary>
/// The <see cref="IComponentHandle"/> is to <see cref="Component"/>s what a
/// <see cref="ICapsuleHandle"/> is to <see cref="Capsule{T}"/>s.
///
/// <see cref="IComponentHandle"/>s provide a mechanism to watch
/// <see cref="Capsule{T}"/>s and register <see cref="SideEffect{T}"/>s, so all
/// Capsule-specific methodologies carry over.
/// </summary>
public interface IComponentHandle : ICapsuleReader, IComponentSideEffectRegistrar
{
}
