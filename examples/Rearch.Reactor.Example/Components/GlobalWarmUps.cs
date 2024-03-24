using MauiReactor;
using ReactorData;
using System.Collections.Generic;
using System.Linq;
using Rearch.Reactor.Components;
using Rearch.Types;
using static Rearch.Reactor.Example.Capsules.ContextCapsules;

namespace Rearch.Reactor.Example.Components;

partial class GlobalWarmUps(VisualNode child) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        return new List<AsyncValue<IModelContext>>
        {
            use.Invoke(ContextWarmUpCapsule)
        }
        .ToWarmUpComponent(
            child: child,
            loading: ContentPage(Label("Loading...").Center()),
            errorBuilder: errors =>
            ContentPage(VStack(
                children: errors
                .Select(error => Label(error.Error.ToString()))
                .ToArray())
            .Center()));
    }
}
