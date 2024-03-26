using ReactorData;
using System;
using System.Collections.Generic;
using System.Linq;
using Rearch.Reactor.Example.Models;
using static Rearch.Reactor.Example.Capsules.ContextCapsules;
using Rearch.Types;
using System.Reactive.Linq;
using System.Collections.Specialized;

namespace Rearch.Reactor.Example.Capsules;

public static class TodoCapsules
{
    /// <summary>
    /// Provides a way to create/update and delete todos.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Actions to add, update, or delete todos.</returns>
    internal static (
        Action<Todo> AddTodo,
        Action<Todo> UpdateTodo,
        Action<Todo> DeleteTodo)
        TodoItemsManagerCapsule(ICapsuleHandle use)
    {
        var context = use.Invoke(ContextCapsule);

        return (
            AddTodo: todo =>
            {
                context.Add(todo);
                context.Save();
            },
            UpdateTodo: todo =>
            {
                context.Replace(todo, new Todo
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Description = todo.Description,
                    Completed = todo.Completed
                });
                context.Save();
            },
            DeleteTodo: todo =>
            {
                context.Delete(todo);
                context.Save();
            }
        );
    }

    /// <summary>
    /// Represents the current filter to search with
    /// (<see cref="string.Empty"/> as a query string represents no current
    /// query).
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Filter data and actions.</returns>
    internal static (
        TodoListFilter Filter,
        Action<string> SetQueryString,
        Action ToggleCompletionStatus)
        FilterCapsule(ICapsuleHandle use)
    {
        var (query, setQuery) = use.State(string.Empty);
        var (completionStatus, setCompletionStatus) = use.State(false);
        return (
          new TodoListFilter
          {
              Query = query,
              CompletionStatus = completionStatus,
          },
          setQuery,
          () => setCompletionStatus(!completionStatus)
        );
    }

    /// <summary>
    /// Represents the todos list using the filter from the
    /// <see cref="FilterCapsule"/>.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Query of to-do list items.</returns>
    internal static IQuery<Todo> TodoQueryCapsule(ICapsuleHandle use)
    {
        var context = use.Invoke(ContextCapsule);
        var (filter, _, _) = use.Invoke(FilterCapsule);
        var filterQuery = filter.Query;
        var completionStatus = filter.CompletionStatus;

        // When query is null/empty, it does not affect the search.
        var todosQuery = use.Memo(
            () => context.Query<Todo>(
                query => query
                .Where(
                    todo =>
                    (
                        todo.Title.Contains(filterQuery, StringComparison.InvariantCultureIgnoreCase) ||
                        (
                            todo.Description != null &&
                            todo.Description.Contains(filterQuery, StringComparison.InvariantCultureIgnoreCase))) &&
                    todo.Completed == completionStatus)
                .OrderBy(todo => todo.Id)),
            [context, filterQuery, completionStatus]);

        return todosQuery;
    }

    internal static AsyncValue<List<Todo>> TodoListCapsule(ICapsuleHandle use)
    {
        var todosQuery = use.Invoke(TodoQueryCapsule);

        var todosObservable = use.Memo(
            () => Observable
            .FromEventPattern(
                (EventHandler<NotifyCollectionChangedEventArgs> ev) => new NotifyCollectionChangedEventHandler(ev),
                ev => todosQuery.CollectionChanged += ev,
                ev => todosQuery.CollectionChanged -= ev)
            .Select(e => (e.Sender as IEnumerable<Todo>)!)
            .StartWith(todosQuery),
            [todosQuery]);
        var todosState = use.Observable(todosObservable);
        return todosState.Select(todos => todos.ToList());
    }

    /// <summary>
    /// Represents the length of the <see cref="TodoListCapsule"/>.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Length of todos.</returns>
    internal static AsyncValue<int> TodoListCountCapsule(ICapsuleHandle use) =>
        use.Invoke(TodoListCapsule).Select(todos => todos.Count);
}
