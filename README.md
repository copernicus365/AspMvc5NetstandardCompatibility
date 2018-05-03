# AspMvc5NetstandardCompatibility

Simple project that demonstrates the inability of ASP.NET MVC 5 razor views (.NET 4.7.1) to handle netstandard 2.0 libraries. In particular, the inability of `Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider` to handle netstandard references whatsoever. The project itself, other than in razor views, is workable (NOT exceptional, but with steps below its workable, there are still extremely frustrating problems, like incompatibility between netstandard and full framework with System.Net.Http). But overall your controllers and the rest of code can now handle netstandard referenced dlls, but the razor views just epically fail.

To duplicate the problem: 

1) Make a new netstandard project in Visual Studio (in this case using the latest VS 2017, even using the preview VS), in this case named: `FooProj_NetStandard20`

2) Make a new ASP.NET MVC 5 (.NET Framework) project targeting .NET 4.7.1, and then make a reference to the new netstandard project (`FooProj_NetStandard20`).

First though: Let us get the MVC project up to date with the latest way of doing handling nuget which is with PackageRefence s within the main csproj, instead of via packages.config. This removes many problems. I can't express how disappointing and irresponsible it is that this feature isn't part of VisualStudio, but that someone else had to make this extension, which I had to happen upon out in the wild. This is a recurrent theme, it seems like the wonderful .NET team is leaving what they must consider "legacy" code in the dust, in this case .NET full framework users. 

So install this (NuGetPackageReferenceUpgrader extension)[https://marketplace.visualstudio.com/items?itemName=CloudNimble.NuGetPackageReferenceUpgrader], right click on the packages.config file and select Upgrade to PackageRefences. It performs its work in a blink of an eye.

Then update all nuget references in the solution to get us all updated, and voila, we have a vanilla new MVC 5 app with a reference to a vanilla new netstandard 2.0 app. 

Now all we have to do to demonstrate the failure of Razor pages in MVC 5 to reference any type within a netstandard dll, is to make a simple type, any will do (even a simple enum) in the netstandard app. In my case I created two types: 

Class `Animal`, and enum `AnimalType`:

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

Now let's hop over to the MVC 5 web project, we'll use the existing HomeController, and pick let's say the About.cshtml page to send in a new model view type: `AboutViewModel`

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

So the key above is we are simply making a reference to our `AnimalType` enum created in the netstandard project. Now edit the About.cshtml page to send in our new view model:

```cs
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

One interesting tidbit is that extension methods defined in the netstandard project DO work from razor! However, it does not work if you applied a global using inclusion within your web.config (see web.config under Views, -> system.web.webPages.razor -> pages -> namespaces -> `<add namespace="name..." />`) that is, you have to apply the using statement to each razor page you want to use it in (in this case, at the top: `@using FooProj_NetStandard20;`)
