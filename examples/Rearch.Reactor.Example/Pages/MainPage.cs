﻿using MauiReactor;
using ReactorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rearch.Reactor.Example.Models;
using Rearch.Reactor.Components;

namespace Rearch.Reactor.Example.Pages;

partial class MainPage : CapsuleConsumer
{
    [Inject]
    readonly IModelContext _modelContext;

    private IQuery<Todo> TodoQueryCapsule(ICapsuleHandle use) =>
        _modelContext.Query<Todo>(query => query.OrderBy(_ => _.Task));

    private (
        Action<Todo> AddTodo,
        Action<Todo> UpdateTodo,
        Action<IEnumerable<Todo>> DeleteTodos)
        TodoItemsManagerCapsule(ICapsuleHandle use) =>
        (
            AddTodo: todo =>
            {
                _modelContext.Add(todo);
                _modelContext.Save();
            },
            UpdateTodo: todo =>
            {
                _modelContext.Update(todo);
                _modelContext.Save();
            },
            DeleteTodos: todos =>
            {
                _modelContext.DeleteRange(todos);
                _modelContext.Save();
            });

    private IQuery<Todo> TodoItemsCapsule(ICapsuleHandle use)
    {
        var query = use.Invoke(TodoQueryCapsule);

        var todos = use.Memo(() => query, [query]);

        return todos;
    }

    public override VisualNode Render(ICapsuleHandle use)
    {
        var todoItems = use.Invoke(TodoItemsCapsule);

        return ContentPage(
            Grid("Auto, *, Auto", "*",
                TodoEditor(OnCreatedNewTask),

                CollectionView()
                    .ItemsSource(todoItems, i => RenderItem(i, OnItemDoneChanged))
                    .GridRow(1),

                Button("Clear List")
                    .OnClicked(OnClearList)
                    .GridRow(2)

            ));

        void OnItemDoneChanged(Todo item, bool done)
        {
            var (_, UpdateTodo, _) = use.Invoke(TodoItemsManagerCapsule);

            item.Done = done;

            UpdateTodo(item);
        }

        void OnCreatedNewTask(Todo todo)
        {
            var (AddTodo, _, _) = use.Invoke(TodoItemsManagerCapsule);

            AddTodo(todo);
        }

        void OnClearList()
        {
            var (_, _, DeleteTodos) = use.Invoke(TodoItemsManagerCapsule);

            var todoItems = use.Invoke(TodoItemsCapsule);

            DeleteTodos(todoItems);
        }
    }

    static Grid RenderItem(Todo item, Action<Todo, bool> onItemDoneChanged)
        => Grid("54", "Auto, *",
            CheckBox()
                .IsChecked(item.Done)
                .OnCheckedChanged((s, args) => onItemDoneChanged(item, args.Value)),
            Label(item.Task)
                .TextDecorations(item.Done ? TextDecorations.Strikethrough : TextDecorations.None)
                .VCenter()
                .GridColumn(1));

    static VisualNode TodoEditor(Action<Todo> created)
        => Render<string>(state =>
            Grid("*", "*,Auto",
                Entry()
                    .Text(state.Value ?? string.Empty)
                    .OnTextChanged(text => state.Set(s => text, false)),
                Button("Create")
                    .GridColumn(1)
                    .OnClicked(() =>
                    {
                        created(new Todo { Task = state.Value ?? "New Task" });
                        state.Set(s => string.Empty);
                    })
                )
            );
}
