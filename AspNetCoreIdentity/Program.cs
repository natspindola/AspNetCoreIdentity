using AspNetCoreIdentity.Areas.Identity.Data;
using AspNetCoreIdentity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

// *** Configurando servi�os no container ***

// Politica de cookies (LGPD)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Adicionando suporte ao contexto do Identity via EF
builder.Services.AddDbContext<AspNetCoreIdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AspNetCoreIdentityContextConnection")));

// Adicionando configura��o padr�o do Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AspNetCoreIdentityContext>();

// Adicionando Autoriza��es personalizadas por policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PodeExcluir", policy => policy.RequireClaim("PodeExcluir"));

    options.AddPolicy("PodeLer", policy => policy.Requirements.Add(new PermissaoNecessaria("PodeLer")));
    options.AddPolicy("PodeEscrever", policy => policy.Requirements.Add(new PermissaoNecessaria("PodeEscrever")));
});

// Resolvendo DI para o Handler de Authorization
builder.Services.AddSingleton<IAuthorizationHandler, PermissaoNecessariaHandler>();

// Adicionando MVC no pipeline
builder.Services.AddControllersWithViews();

// Adicionando suporte a componentes Razor (ex. Telas do Identity)
builder.Services.AddRazorPages();

var app = builder.Build();


// *** Configurando o resquest dos servi�os no pipeline *** 

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/erro/500");
    app.UseStatusCodePagesWithRedirects("/erro/{0}");
    app.UseHsts();
}

// Redirecionamento para HTTPs
app.UseHttpsRedirection();

// Uso de arquivos est�ticos (ex. CSS, JS)
app.UseStaticFiles();

// Adicionando suporte a rota
app.UseRouting();

// Autenticacao e autoriza��o (Identity)
app.UseAuthentication();
app.UseAuthorization();

// Rota padr�o
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Mapeando componentes Razor Pages (ex: Identity)
app.MapRazorPages();

app.Run();