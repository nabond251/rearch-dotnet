using MauiReactor;
using System;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;

namespace Rearch.Reactor.Example.Components;

internal class Item(Todo item, Action<Todo, bool> onItemDoneChanged) : CapsuleConsumer
{
    public override Grid Render(ICapsuleHandle use)
        => Grid("54", "Auto, *",
            CheckBox()
                .IsChecked(item.Completed)
                .OnCheckedChanged((s, args) => onItemDoneChanged(item, args.Value)),
            Label(item.Title)
                .TextDecorations(item.Completed ? TextDecorations.Strikethrough : TextDecorations.None)
                .VCenter()
                .GridColumn(1));
}
