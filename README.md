# AspMvc5NetstandardCompatibility

Simple project that demonstrates the inability of ASP.NET MVC 5 razor views (.NET 4.7.1) to handle netstandard 2.0 libraries. Either something deep within MVC 5 is the problem, or (as discused below) `Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider` might be the problem. The project - in the controllers and so forth - is workable when referencing netstandard types, though even here there are extremely frustrating problems that arise, like chaos with `System.Net.Http`, and there seems to always be warnings about incompatibilities no matter what you do (no matter what `bindingRedirect`s you put in), and this is beyond frustrating and brittle. Even so, things appear to be workable after updating `packages.config` to PackageReferences (see below), but the razor views just epically fail.

To duplicate the problem, as I have done in this repo: 

1) Make a new netstandard project named here `FooProj_NetStandard20` in Visual Studio (using the latest VS 2017, even using Visual Studio preview which I actually was using doesn't help).

2) Make a new ASP.NET MVC 5 (.NET Framework) project targeting .NET 4.7.1, and then make a reference to the new netstandard project (`FooProj_NetStandard20`).

Then we must update the MVC project to use the latest way of handling nuget (why is this not in the standard templates for new projects yet?), which is with `PackageRefence`s within the main `csproj`, instead of via `packages.config`. This removes many problems. 

> If I may express how disappointing it is that this feature and many of the other things talked about here aren't part of Visual Studio, and that the messaging hasn't just told us, for instance, that MVC 5 razor doesn't yet support `netstandard`.

So install this [NuGetPackageReferenceUpgrader extension](https://marketplace.visualstudio.com/items?itemName=CloudNimble.NuGetPackageReferenceUpgrader), right click on the `packages.config` file and select "Upgrade to PackageRefences". It performs its work in a blink of an eye.

Then update all nuget references in the solution to get us all up to date, and voila, we have a vanilla new MVC 5 app with a reference to a vanilla new netstandard 2.0 app. 

Now all we have to do to demonstrate the failure of Razor pages in MVC 5 is to reference any type within a netstandard dll. In my case I created two types in that netstandard project, class `Animal`, and enum `AnimalType`:

```cs
// don't even need any usings 

namespace FooProj_NetStandard20
{
	public enum AnimalType { Cat, Dog, Zebra, Alligator }

	public class Animal
	{
		public string Name { get; set; }

		public AnimalType AnimalType { get; set; }

		public int Age { get; set; }

		public string GetInfo() => $"Animal name is: {Name}, of type {AnimalType}, aged {Age}";

		public static int AddEm(int a, int b) => a + b;
	}
}
```

Now let's hop over to the MVC 5 web project, we'll use the existing `HomeController`, and pick let's say the `About.cshtml` page to send in a new view model type: `AboutViewModel`

```cs
using FooProj_NetStandard20;

namespace AspMvc5WebApp471.Models
{
	public class AboutViewModel
	{
		public string Title { get; set; }
		public AnimalType AnimalTyp { get; set; }
	}
}
```

So the key above is we are simply referencing the `AnimalType` enum that was created in the netstandard project, making it's enum a property on our view model. Now edit the `About.cshtml` page to send in our new view model:

```cshtml
@model AboutViewModel
@{
	ViewBag.Title = "About";
}
<h2>@ViewBag.Title.</h2>
<h3>@ViewBag.Message</h3>

<p>Use this area to provide additional information.</p>

<p>@Model.Title</p>
<p>@Model.AnimalTyp</p> @*red squiggly error right here*@
```

The last line shows a compile time red squiggly error under the property reference to `.AnimalTyp` with a message popup saying: *@

> The type 'Enum' is defined in an assembly that is not referenced. You must add a reference to assembly 'netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'.	AspMvc5WebApp471

One interesting tidbit is that extension methods defined in the netstandard project DO work from razor! However, it does not work if you applied a global using inclusion within your `web.config` (see `web.config` under Views, -> system.web.webPages.razor -> pages -> namespaces -> `<add namespace="name..." />`) that is, you have to apply the using statement to each razor page you want to use it in (in this case, at the top of `About.cshtml`: `@using FooProj_NetStandard20;`)

## Solutions (please)?

I wish the wonderful ASP.NET team would have showed us the dignity to tell us, when all of these repeated announcement have come out about the supposed ability of .NET full framework to reference netstandard, that in fact, Razor and thus all ASP.NET MVC 5 projects, are not ready for it yet. But it seems to me like the team is almost utterly AWOL with regard to anything that's not .NET Core. I love .NET Core, but hey, don't leave us that high and dry. Even the most minimal steps could be taken with right management, even just one or two dedicated engineers to these issues, could have changed the game here, so this is utterly inexcusable. A good example is what we saw above, I had to be crawling around on backend github discussions to final hear news of the NuGetPackageReferenceUpgrader extension, which guaranteed is a project that the team themselves could have done and even baked into Visual Studio in a week's time. Yet we have received no guidance or help on this. As it is, Visual Studio 2017 should be asking you to update projects with old `packages.config`s, and the reason that is such a big deal, is this takes away half the errors one encounters with referencing netstandard libraries. 

So in this case, it appears to me the work that needs to be done is to update the `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` project so it can handle netstandard without these errors. I have actually tried to do this myself, but it didn't end up working. So here is a little update on that:

ASP.NET MVC 5 can only handle C# 5. For versions of C# after that (starting with C# 6), to get that support (like support for $"string {variable}" interpolation, which is an indispensable wonder), the web.config of the project has to make a reference to a compiler for razor as follows (this is included in the template for new ASP MVC projects nowadays):

```xml
<system.codedom>
	<compilers>
		<compiler language="c#;cs;csharp" extension=".cs" type="Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider, Microsoft.CodeDom.Providers.DotNetCompilerPlatform, Version=1.0.8.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" warningLevel="4" compilerOptions="/langversion:default /nowarn:1659;1699;1701"/>
	</compilers>
</system.codedom>
```

So next step is, I wondered if this (`Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, which is currently supplied through nuget) could just be recompiled, and it turns out that thankfully, they have graciously published this project open-source [here](https://github.com/copernicus365/RoslynCodeDomProvider), under the github name `RoslynCodeDomProvider`, though the actual nuget project it publishes is called `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`. It is this project that contains references to both `CSharpCodeProvider.cs` and `VBCodeProvider.cs`. This project is a thin facade it appears over the heavy lifters, which are some references to roslyn and so forth. 

All of this to say, I did try to compile all of this on top of .NET 4.7.1 (even tried on top of netstandard 2.0, but ran into problems with that). My build worked (on top of 4.7.1), and I was even able to set this as my razor compiler in web.config. To do that make a dll reference in your MVC project to your own compiled version of Microsoft.CodeDom.Providers.DotNetCompilerPlatform, so that `Microsoft.CodeDom.Providers.DotNetCompilerPlatform2.dll` is in the bin folder, and then set as follows:

```xml
<system.codedom>
	<compilers>
		<compiler language="c#;cs;csharp" extension=".cs" type="Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider, Microsoft.CodeDom.Providers.DotNetCompilerPlatform2, Version={version}, Culture=neutral, PublicKeyToken={key, use AssemblyName for this after signing the assembly in VS}" warningLevel="4" compilerOptions="/langversion:default /nowarn:1659;1699;1701"/>
	</compilers>
</system.codedom>
```

I was pretty amazed that this worked *at all*, but it didn't fix the problems. I've included this whole separate solution in this repo under the folder `RoslynCodeDomProvider2` compiled against 4.7.1, if someone else want to play around with trying to make that work, or they can just go to the source project and do the same.

Lastly, I must mention that even when we remove the custom compiler in ASP.NET MVC 5, which is the standard today, allowing MVC 5 to use newer C# features, that nonetheless, this still did not fix the problems. To do that, just comment out the entire `<system.codedom>...` section in the `web.config`. You'll notice you cannot then use C# 6+ features, string interpolation will now act as an error. But the exact same problem still appears with the same error messages when netstandard 2.0 types are referenced, so this problem might just not be fixed at all via updating `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`.

So, I suppose we need someone with some real expertise in these matters on the ASP.NET team to get razor views to work when referencing netstandard.
