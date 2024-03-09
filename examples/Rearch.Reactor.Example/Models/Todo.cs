using ReactorData;

namespace Rearch.Reactor.Example.Models;

/// <summary>
/// Represents an item in the todos list.
/// </summary>
[Model]
partial class Todo
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public bool Completed {  get; set; }
}
