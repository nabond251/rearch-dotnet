using ReactorData;

namespace Rearch.Reactor.Example.Models;

/// <summary>
/// Represents the filter for a list of todos.
/// </summary>
[Model]
partial class TodoListFilter
{
    public required string Query { get; set; }

    public bool CompletionStatus {  get; set; }
}
