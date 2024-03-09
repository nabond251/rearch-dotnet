using MauiReactor;
using System;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;

namespace Rearch.Reactor.Example.Components;

internal class TodoEditor(Action<Todo> created) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
        => Render<string>(state =>
            Grid("*", "*,Auto",
                Entry()
                    .Text(state.Value ?? string.Empty)
                    .OnTextChanged(text => state.Set(s => text, false)),
                Button("Create")
                    .GridColumn(1)
                    .OnClicked(() =>
                    {
                        created(new Todo
                        {
                            Title = string.IsNullOrEmpty(state.Value) ?
                                "New Task" :
                                state.Value
                        });
                        state.Set(s => string.Empty);
                    })
                )
            );
}
