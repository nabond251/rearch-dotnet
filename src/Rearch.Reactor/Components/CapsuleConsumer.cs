// <copyright file="CapsuleConsumer.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

/// <summary>
/// A <see cref="Component"/> that has access to a
/// <see cref="ICapsuleHandle"/>, and can consequently consume
/// <see cref="Capsule{T}"/>s and <see cref="SideEffect{T}"/>s.
/// </summary>
public abstract partial class CapsuleConsumer : CapsuleConsumer<object>
{
}
