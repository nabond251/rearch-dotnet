using ReactorData;
using System;
using System.Collections.Generic;
using System.Linq;
using Rearch.Reactor.Example.Models;
using static Rearch.Reactor.Example.Capsules.ContextCapsules;

namespace Rearch.Reactor.Example.Capsules;

public static class TodoCapsules
{
    internal static IQuery<Todo> TodoQueryCapsule(ICapsuleHandle use) =>
        use.Invoke(ContextCapsule).Query<Todo>(query => query.OrderBy(_ => _.Task));

    internal static (
        Action<Todo> AddTodo,
        Action<Todo> UpdateTodo,
        Action<IEnumerable<Todo>> DeleteTodos)
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
                context.Replace(todo, new Todo { Id = todo.Id, Task = todo.Task, Done = todo.Done });
                context.Save();
            },
            DeleteTodos: todos =>
            {
                context.Delete([.. todos]);
                context.Save();
            }
        );
    }

    internal static IQuery<Todo> TodoItemsCapsule(ICapsuleHandle use)
    {
        var query = use.Invoke(TodoQueryCapsule);

        var todos = use.Memo(() => query, [query]);

        return todos;
    }
}
