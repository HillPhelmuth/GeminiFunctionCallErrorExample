// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Plugins.Core;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
var googleApiKey = builder.Configuration["GoogleAI:ApiKey"]!;
var settings = new GeminiPromptExecutionSettings { ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions };


// This will throw an exception when the TimePlugin contains a function with an enum parameter (DateMatchingLastDayName) which is not supported by the Google AI connector

var kernel = Kernel.CreateBuilder().AddGoogleAIGeminiChatCompletion("gemini-pro", googleApiKey).Build();
var timePlugin = KernelPluginFactory.CreateFromType<TimePlugin>();
var timeWithoutOffendingFunction = timePlugin.Where(f => f.Name != "DateMatchingLastDayName");
var workingTimePlugin = KernelPluginFactory.CreateFromFunctions("GoodTimes", timeWithoutOffendingFunction);
kernel.Plugins.Add(workingTimePlugin);
Console.WriteLine("This example uses the TimePlugin (without DateMatchingLastDayName):");
var result = await kernel.InvokePromptAsync("What is the current UTC date and time", new KernelArguments(settings));
Console.WriteLine(result.GetValue<string>());
kernel.Plugins.Remove(workingTimePlugin);
kernel.Plugins.Add(timePlugin);
Console.WriteLine("This example uses the TimePlugin with all available functions.");
var result2 = await kernel.InvokePromptAsync("What is the current UTC date and time", new KernelArguments(settings));
Console.WriteLine(result2.GetValue<string>());
