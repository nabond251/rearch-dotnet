using Rearch.Reactor.Example.Models;
using System;

namespace Rearch.Reactor.Example.Pages;

internal class CreateTodoPageProps()
{
    public Action<Todo> TodoCreator { get; set; }
}