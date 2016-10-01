JSONAPI.NET
===========
[![jsonapi MyGet Build Status](https://www.myget.org/BuildSource/Badge/jsonapi?identifier=caf48269-c15b-4850-a29e-b41a23d9854d)](https://www.myget.org/)

News
----

The NuGet packages are out!
* [JSONAPI](https://www.nuget.org/packages/JSONAPI/)
* [JSONAPI.EntityFramework](https://www.nuget.org/packages/JSONAPI.EntityFramework/)

JSON API Compliance
----------------------

The master branch is roughly compatible with the RC3 version of JSON API. The major missing feature is inclusion of related resources. Many changes made to the spec since RC3 are not yet available in this library. Full 1.0 compliance is planned, so stay tuned!

What is JSONAPI.NET?
====================

JSONAPI.NET is a set of utility classes that aim to make it possible to implement [JSON API spec](http://jsonapi.org/) compliant RESTful web services quickly and easily using ASP.NET MVC WebAPI. In practice, the primary target is to build backend web services that work with the Ember Data library and [Ember.js](http://emberjs.com). Since the JSON API spec project originated from the Ember Data and Active Model Serializers project, Ember Data's included RESTAdapter is very close to JSON API spec compliant--but not quite. You will need to use the separately available [JSON API Adapter](https://github.com/kurko/ember-json-api) to connect with Ember Data, and it is currently a primary goal of this project to work well with this adapter (coverage of the whole JSON API spec is far from complete).

How do I use it?
----------------

At its most basic level, JSONAPI includes a Json.NET `JsonMediaTypeFormatter`, which can be used to transform WebAPI JSON output into JSON API spec compliant messages. You can use the media formatter by adding an instance of it to the Json.NET Formatters collection, probably in your WebApiConfig class, like this:

```C#
public static void Register(HttpConfiguration config)
{
  //...
  JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
  formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
  GlobalConfiguration.Configuration.Formatters.Add(formatter);
  //...
}
```

In connection with the media formatter, a `PluralizationService` is required to translate between singular and plural object names, since the spec (usually?) uses plural class/type names, and your class names are likely singular. The `JSONAPI.Core.PluralizationService` is provided as a horribly naive sample implementation (it simply adds or strips "s" from the end of the class name), but at least lets you manually add explicit mappings with the `AddMapping(string singular, string plural)` method. 

> :information_source: HINT: the `JSONAPI.EntityFramework` library includes a `PluralizationService` that uses EntityFramework's much more sophisticated inflector!

That will configure a working formatter. Note that the formatter (by default) will only handle requests having an `Accept` or `Content-type` header of `application/vnd.api+json`, the media type defined for the JSON API spec.

Essentially, that's it! At this point your WebAPI methods should be able to produce and consume JSON that conforms to (a subset of) the JSON API spec! There is, of course, much more that you can configure.

Serializing Relationships
-------------------------

One of the benefits of the JSON API spec is that it provides several ways to [serialize relationships](http://jsonapi.org/format/#document-structure-resource-relationships). This gives you a lot of flexibility to improve your API performance for both the server and client: If related objects are expensive to retrieve or compute, you can represent the relationship as a URL to avoid that cost if the client never accesses it. If it is not expensive, or you know the client will need them, you can bundle them in the same response to reduce the number of network requests. JSONAPI.NET provides you three attributes you can use to decorate your models to specify how a relationship property will be serialized.

* `SerializeAs`: Decorate a property with this attribute to specify that the related objects should be serialized as ids, a link, or with the serialized object(s) embedded within the parent object (embedding is NOT part of the spec, but is supported by Ember Data!)
* `IncludeInPayload`: Decorate a property with this attribute to cause the related objects to be included with the parent object in the same response message. This is called a Compound Document in the spec. This only makes sense to use with `[SerializeAs(SerializeAsOptions.Ids)]`.
* `LinkTemplate`: If you use `[SerializeAs(SerializeAsOptions.Link)]`, you must also use this attribute to specify the format of the link sent to the client. Two template parameters are supported, `{0}` will be replaced with the Id(s) of the related object, and `{1}` will be replaced with the Id of the parent object.

> :information_source: As a side note, the RelationAggregator class handles the work of pulling any included related objects into the document--which may be a recursive process: for example, if an object has a relationship of its own type and that relationship property is annotated to be included, then it will recursively include all related objects of the same type! However, those related objects will all be included in a flat array for that type, according to the spec.


# Filtering

JSONAPI defines the URL query parameter for filtering is `filter` and should be combined with the associations but there is not much more how the syntax should be. So the JSONAPI.Net framework provides the following filter syntax.

## Basic
If you want to get a Resource by Id you provide the Id in the URL as part of the path e.g:

``` URL
/posts/1 
```
With this call you get the post with Id 1 or a status code 404 if no post with the Id 1 exists.

If you would filter all related comments on post with the Id 1 you append the name of relationship e.g:

``` URL
/posts/1/comments 
```

In both above cases you can add the `filter` query parameter to filter the result on non Id properties. The property to filter on is specified in squared brackets like below.
The value(s) after the equal sing we would call filter value.

``` URL
/posts/1/comments?filter[autor]=Bob
```
This will only return objects where "Bob" is the value of the author property.

``` URL
/posts/1/comments?filter[category]=3
```
This will only return objects where 3 is the value of the category property.

## Multiple value filter

If you want to filter by multiple values you can concatenate the values separated by comma. In case of strings you need to quote the strings to provide multiple values.

``` URL
/posts?filter[title]=Post one, which is awesome
 => this returns all posts with title "Post one, which is awesome"
```
``` URL
/posts?filter[title]="Post one","which is awesome"
 => this returns all posts with title "Post one" OR  "which is awesome"
```

If the field is numeric or DateTime you can concatenate values with comma.
``` URL
/posts?filter[category]=1,2,3
 => this returns all posts with category 1 OR 2 OR 3
```
``` URL
/posts?filter[date-created]=2016-09-01,2016-09-02
 => this returns all posts with date-created 2016-09-01 OR 2016-09-02
```

## Wildcard filters

In case of string properties you can provide a percent sign (%) at the beginning or end of the filter value. This will advice to not compare with equal but with contains.

``` URL
/posts?filter[title]="%one","%awesome%"
 => this returns all posts with title ending on "one" OR containing the word "awesome"
```

> :information_source: HINT: the comparison with wildcards is made case **insensitive**.

> :information_source: HINT: If there is a comma inside of the quoted filter value the term gets not split.

> :information_source: HINT: The percent sign is used to start an encoded character in the URL so the filter values **must unconditionally be encoded** before put in an URL. The above example should look like this when sent to server:`filter%5Btitle%5D=%22%25one%22%2C%22%25awesome%25%22` 
  
## DateTime filters
DateTime filters with equal can be a pain. If you store DateTime with full resolution (milliseconds) you must provide the full resolution to make the filter value equal the stored value.

To avoid this problem JSONAPI.Net is automatically filtering by a DateTime range. If you provide a "day" (YYYY-MM-DD) as filter value the filter will be this: `BETWEEN day 00:00:00.000 AND day 23:59:59.999`.

Now if the property is DateTime or DateTimeOffset you can provide the following types of filter values:

| Part   | Format              | Filter                                            |
|--------|---------------------|---------------------------------------------------|
| year   | YYYY                | YYYY-01-01 00:00:00.000 - YYYY-12-31 23:59:59.999 |
| month* | YYYY-MM             | YYYY-MM-01 00:00:00.000 - YYYY-MM-31 23:59:59.999 |
| day    | YYYY-MM-DD          | YYYY-MM-DD 00:00:00.000 - YYYY-MM-DD 23:59:59.999 |
| hour   | YYYY-MM-DD HH       | YYYY-MM-DD HH:00:00.000 - YYYY-MM-DD HH:59:59.999 |
| minute | YYYY-MM-DD HH:mm    | YYYY-MM-DD HH:mm:00.000 - YYYY-MM-DD HH:mm:59.999 |
| second | YYYY-MM-DD HH:mm:ss | YYYY-MM-DD HH:mm:ss.000 - YYYY-MM-DD HH:mm:ss.999 |

*) assuming this month has 31 days. JSONAPI.Net automatically determines the last day of month by adding one month to the given date. This respects months with less than 31 days.

If you want to filter all posts created in month May of year 2016 you must provide the format for month filled by your needed date:


``` URL
/posts?filter[date-created]=2016-05
 => this returns all posts with date-created in may 2016
```

## Range filters
There is no implementation on filtering number or date ranges. If you need things like that you can open an issue or even better provide a pull request.

## Logical operation
Filters can be combined for multiple fields. You can filter posts created in May 2016 and containing awesome in title:

``` URL
/posts?filter[date-created]=2016-05&filter[title]="%awesome%"
 => this returns all posts with date-created in may 2016 with title containing "awesome"
```
So we have the following standard behavior to concatenate multiple filters together:
- multiple values on the same property are concatenated with OR
- multiple filters on multiple properties are always concatenated with AND


There is no implementation on filtering with a tree of logical AND and OR operations other than described above. If you need things like that you can open an issue or even better provide a pull request.


# Great! But what's all this other stuff?


JSONAPI.Net intends to be so much more than just a media formatter. It provides subclasses of WebAPI's `ApiController` that automate some of the boilerplate that goes into building a web service, especially one that can comply with the JSON API spec. While `JSONAPI.ApiController` doesn't do much, it can be subclassed to make default actions for the HTTP methods that WebAPI supports.

A `JSONAPI.IMaterializer` object can be added to that `ApiController` to broker between the WebAPI layer and your persistence layer. If you create an implementation of `IMaterializer`, you can begin to use the base `ApiController`'s handler methods, such as the Get() handler--which already handles the JSON API spec for requesting multiple objects with a list of comma-separated Ids!

# Didn't I read something about using Entity Framework?

The classes in the `JSONAPI.EntityFramework` namespace take great advantage of the patterns set out in the `JSONAPI` namespace. The `EntityFrameworkMaterializer` is an `IMaterializer` that can operate with your own `DbContext` class to retrieve objects by Id/Primary Key, and can retrieve and update existing objects from your context in a way that Entity Framework expects for change tracking…that means, in theory, you can use the provided `JSONAPI.EntityFramework.ApiController` base class to handle GET, PUT, POST, and DELETE without writing any additional code! You will still almost certainly subclass `ApiController` to implement your business logic, but that means you only have to worry about your business logic--not implementing the JSON API spec or messing with your persistence layer.



# Configuration JSONAPI.EntityFramework

- [ ] Add some hints about the configuration of JSONAPI.EntityFramework

## Manipulate entities before JSONAPI.EntityFramework persists them
To change your entities before they get persisted you can extend the `EntityFrameworkDocumentMaterializer<T>` class. You need to register your custom DocumentMaterializer in your `JsonApiConfiguration` like that: 
```C#
configuration.RegisterEntityFrameworkResourceType<MyEntityType>(c =>c.UseDocumentMaterializer<CustomDocumentMaterializer>());
```  
Afterwards you can override the `OnCreate`, `OnUpdate` or `OnDelete` methods in your `CustomDocumentMaterializer`.

```C#
protected override async Task OnCreate(Task<MyEntityType> record)
{
    await base.OnUpdate(record);
    var entity = await record;
    entity.CreatedOn = DateTime.Now;
    entity.CreatedBy = Principal?.Identity;
}
```

> :information_source: HINT: To get the `Principal` you can add the following part into your `Startup.cs` which registers the `Principal` in Autofac and define a constructor Parameter on your `CustomDocumentMaterializer` of type `IPrincipal`.

```C#

configurator.OnApplicationLifetimeScopeCreating(builder =>
{
// ... 
builder.Register(ctx => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
builder.Register((c, p) =>
    {
        var owin = c.Resolve<IOwinContext>();
        return owin.Authentication.User;
    })
    .As<IPrincipal>()
    .InstancePerRequest();
}
```


## Set the context path of JSONAPI.EntityFramework

Per default the routes created for the registered models from EntityFramework will appear in root folder. This can conflict with folders of the file system or other routes you may want to serve from the same project.
To solve the issue we can create an instance of the `BaseUrlService` and put it in the configuration.
The `BaseUrlService` can be created with the context path parameter which will be used to register the routes and put into responses.

```C#
var configuration = new JsonApiConfiguration();
configuration.RegisterEntityFrameworkResourceType<Post>();
// ... registration stuff you need

// this makes JSONAPI.NET create the route 'api/posts'
configuration.CustomBaseUrlService = new BaseUrlService("api");
```

## Set the public origin host of JSONAPI
Since JSONAPI.NET returns urls it could result wrong links if JSONAPI runs behind a reverse proxy. Configure the public origin address as follows:
```C#
var configuration = new JsonApiConfiguration();
configuration.RegisterEntityFrameworkResourceType<Post>();
// ... registration stuff you need

// this makes JSONAPI.NET to set the urls in responses to https://api.example.com/posts
// Important: don't leave the second string parameter! see below.
configuration.CustomBaseUrlService = new BaseUrlService(new Uri("https://api.example.com/"), "");

// this can also be combined with the context paht for routing like that:
// this makes JSONAPI.NET create the route 'api/posts' and response urls to be https://api.example.com:9443/api/posts
configuration.CustomBaseUrlService = new BaseUrlService(new Uri("https://api.example.com:9443/"), "api");

```

# Metadata



## Pagination

### total-pages / total-count
When using Entity Framework you can register type `EntityFrameworkQueryableResourceCollectionDocumentBuilder` to enable the `total-pages` and `total-count` meta properties.
When pagination is used the `total-pages` and `total-count` meta properties are provided to indicate the number of pages and records to the client.

