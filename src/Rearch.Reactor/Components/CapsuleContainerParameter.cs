// <copyright file="CapsuleContainerParameter.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

/// <summary>
/// <see cref="CapsuleContainer"/> parameter.
/// </summary>
internal sealed class CapsuleContainerParameter()
{
    /// <summary>
    /// Gets or sets container provided by
    /// <see cref="CapsuleContainerProvider{TComponent}"/> and used by
    /// <see cref="CapsuleConsumer"/>.
    /// </summary>
    internal CapsuleContainer? Container { get; set; }
}
