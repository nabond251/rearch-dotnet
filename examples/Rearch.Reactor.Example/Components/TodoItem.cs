using MauiReactor;
using System;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;
using Microsoft.Maui.Controls;
using static Rearch.Reactor.Example.Capsules.TodoCapsules;

namespace Rearch.Reactor.Example.Components;

internal class TodoItem(Todo item) : CapsuleConsumer
{
    public override VisualNode Render(ICapsuleHandle use)
    {
        var title = item.Title;
        var description = item.Description;
        var id = item.Id;
        var completed = item.Completed;

        var (_, UpdateTodo, DeleteTodo) = use.Invoke(TodoItemsManagerCapsule);

        void Delete() => DeleteTodo(item);
        void ToggleCompletionStatus()
        {
            item.Title = title;
            item.Description = description;
            item.Id = id;
            item.Completed = !completed;

            UpdateTodo(item);
        }

        return Frame(
            Grid("27, 27", "Auto, *",
                CheckBox()
                    .IsChecked(item.Completed)
                    .OnCheckedChanged(ToggleCompletionStatus)
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
                    .GridColumn(1)),
            TapGestureRecognizer(ToggleCompletionStatus, 1),
            TapGestureRecognizer(Delete, 2)
        );
    }
}
