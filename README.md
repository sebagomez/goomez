![](images/GoomezLogo.png?raw=true)

Goomez search is a [Lucene.Net](http://lucenenet.apache.org/) client that indexes the available software in our office. It'll index the extensions and locations stated in the GoomezCrawler.xml file.

It also has a web client for resolving queries to the index, it's a very Google-like web interface. I actually inspected the css properties from Google.com

It was originally a ASP.NET WebForms app, but It's been migrated to .NET Core and the web client is an [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) app with ~~ReactJS~~ [Blazor](https://blazor.net).

Log:
- August 9th 2022. Migrated to .NET 6 and Blazor
- February 18th, 2020. Migrated to .NET Core 3.1
- April 24th, 2019. Migrated to .NET Core 2.2
- June 9th, 2018. Migrated to .NET Core 2.1

![](images/Goomez.gif?raw=true)