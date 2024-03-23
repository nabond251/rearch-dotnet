using MauiReactor;
using System;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using Microsoft.Maui.Controls;

namespace Rearch.Reactor.Example.Components;

internal class TodoItem(Todo item, Action<Todo, bool> onItemDoneChanged) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use) =>
        Frame(
            Grid("27, 27", "Auto, *",
                CheckBox()
                    .IsChecked(item.Completed)
                    .OnCheckedChanged((s, args) => onItemDoneChanged(item, args.Value))
                    .GridRowSpan(2),
                Label(item.Title)
                    .FormattedText(() =>
                    {
                        FormattedString formattedString = new FormattedString();
                        formattedString.Spans.Add(new Span
                        {
                            Text = item.Title,
                            FontAttributes = FontAttributes.Bold,
                        });
                        return formattedString;
                    })
                    .VCenter()
                    .GridColumn(1),
                Label(item.Description)
                    .VCenter()
                    .GridRow(1)
                    .GridColumn(1)));
}
