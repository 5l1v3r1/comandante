# Comandante

ASP MVC .Net Core debugging tool.
* Logs requests details
* Logs all messages
* Displays server information
* Display routing information
* Display hosting information
* Display EntityFramework context details
* Run SQL queryies on EntityFramework context


# Instalation
 ```cs
 Install-Package Comandante
```

# Usage
 ```cs
public void ConfigureServices(IServiceCollection services)
{
   // Add Comandante service
    services.AddComandante();
}
        
public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostingEnvironment env)
{
    // Use Comandante middleware
    app.UseComandante();
}
```
Run the app and navigate to http://localhost/comandante

![Comandante](comandante_index.png)
&nbsp;

&nbsp;


# Entity Framework

![Comandante Entity Framework](comandante_entity_framework.png)

![Comandante Entity Framework Rows](comandante_entity_rows.png)

![Comandante Entity Framework Create](comandante_entity_create.png)

![Comandante Entity Framework Run SQL](comandante_run_sql.png)



# MVC

![Comandante MVC Hosting](comandante_hosting.png)

![Comandante MVC Routing](comandante_routing.png)

![Comandante MVC Services](comandante_services.png)

![Comandante MVC Service Method](comandante_service_method.png)

# Monitoring

![Comandante Monitoring Requests](comandante_requests.png)

