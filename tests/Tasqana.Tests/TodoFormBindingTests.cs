using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Tasqana.Models.http;

namespace Tasqana.Tests;

public class TodoFormBindingTests
{
    [TestCase("", "")]
    [TestCase("New description", "New description")]
    [TestCase(null, null)]
    public async Task BindDescription_PreservesEmptyTextAndDistinguishesMissingField(string? input, string? expected)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        using var provider = services.BuildServiceProvider();
        var metadata = provider.GetRequiredService<IModelMetadataProvider>().GetMetadataForType(typeof(TodoDTO));
        var binder = provider.GetRequiredService<IModelBinderFactory>().CreateBinder(new ModelBinderFactoryContext
        {
            Metadata = metadata,
            BindingInfo = new BindingInfo { BindingSource = BindingSource.Form }
        });
        var values = new Dictionary<string, StringValues>();
        if (input != null) values["description"] = input;
        var http = new DefaultHttpContext { RequestServices = provider };
        var action = new ActionContext(http, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var context = DefaultModelBindingContext.CreateBindingContext(action,
            new FormValueProvider(BindingSource.Form, new FormCollection(values), CultureInfo.InvariantCulture),
            metadata, new BindingInfo { BindingSource = BindingSource.Form }, "");

        await binder.BindModelAsync(context);

        Assert.That(context.ModelState.ErrorCount, Is.Zero);
        Assert.That(context.Result.IsModelSet, Is.True);
        Assert.That(((TodoDTO)context.Result.Model!).description, Is.EqualTo(expected));
    }
}
