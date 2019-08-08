# Debug4MvcNetCore

ASP Mvc .Net Core debugging tool.
* Logs requests details
* Logs all messages
* Displays server information
* Display routing information
* Display hosting information
* Display EntityFramework context details
* Run SQL queryies on EntityFramework context


## Instalation
 ```cs
 Install-Package Debug4MvcNetCore
```

## Usage
 ```cs
public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostingEnvironment env)
{
            // Use Debug4MvcNetCore middleware
            app.UseDebug4MvcNetCore();
            
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Dashboard}/{action=Index}/{id?}");
            });
}
```
Run the app and navigate to http://localhost/debug

&nbsp;

&nbsp;

## Track requests details

&nbsp;

&nbsp;

### List all requests with status and max log level
![Debug4Mvc Requests](Debug4Mvc_Requests.PNG)

&nbsp;

&nbsp;

###  Show request details, including all logs.

![Debug4Mvc Request](Debug4Mvc_Request.PNG)

&nbsp;

&nbsp;

###  Show log details
![Debug4Mvc Log](Debug4Mvc_Log.PNG)

&nbsp;

&nbsp;

## Display EntityFramework Core context details

&nbsp;

&nbsp;

### Show migrations, tables and models
![Debug4Mvc Routing](Debug4Mvc_EntityFrameworkCore.PNG)

&nbsp;

&nbsp;

### Let you run SQL on context
![Debug4Mvc Routing](Debug4Mvc_EntityFrameworkCore_RunSql.PNG)

&nbsp;

&nbsp;

## Display routing information
![Debug4Mvc Routing](Debug4Mvc_Routing.PNG)

