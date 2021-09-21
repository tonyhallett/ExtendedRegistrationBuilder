A CustomReflectionContext that extends the behaviour of RegistrationBuilder so as to work with methods.

This is a quick attempt that provides the required behaviour for my particular use case but requires further work.
For instance to be thread safe.

Usage
```
var reflectionContext = new RegistrationBuilderWithMethods();

// get the part builder
var partBuilder = reflectionContext.ForTypesMatching(t =>
{
    // determine
    return true;
});
partBuilder.ExportMethods(m =>
{
    //determine if method is eligible
    return true;
}, (m, exportBuilder) =>
{
    // use the exportBuilder 
});

```