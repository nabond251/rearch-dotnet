using Microsoft.Extensions.DependencyInjection;
using ReactorData;
using Rearch.Reactor.Example.Helpers;
using Rearch.Types;
using System;
using System.Threading.Tasks;

namespace Rearch.Reactor.Example.Capsules
{
    internal static class ContextCapsules
    {
        internal static async Task<IModelContext> ContextAsyncCapsule(ICapsuleHandle use)
        {
            await Task.Delay(1000);
            return (await ServiceHelper.ServicesAsync).GetRequiredService<IModelContext>();
        }

        internal static IModelContext ContextCapsule(ICapsuleHandle use) =>
            use.Invoke(ContextWarmUpCapsule).GetData().UnwrapOrElse(
                () => throw new InvalidOperationException(
                    "ContextWarmUpCapsule was not warmed up!"));

        internal static AsyncValue<IModelContext> ContextWarmUpCapsule(ICapsuleHandle use)
        {
            var task = use.Invoke(ContextAsyncCapsule);
            return use.Task(task);
        }
    }
}