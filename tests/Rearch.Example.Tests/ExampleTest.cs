// <copyright file="ExampleTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Example.Tests;

/// <summary>
/// Test example.
/// </summary>
public class ExampleTest
{
    /// <summary>
    /// Test that example runs.
    /// </summary>
    [Fact]
    public void MainFunctionRunsCorrectly()
    {
        Example.Main();
        Assert.True(true);
    }
}
