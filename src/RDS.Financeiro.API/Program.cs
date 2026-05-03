using RDS.Financeiro.Application;
using RDS.Financeiro.Infrastructure;
using RDS.Financeiro.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RDS.Financeiro API", Version = "v1" });
});

var app = builder.Build();

// Seed do Plano de Contas padrão
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<FinanceiroDbContext>();
    await PlanoContasSeed.AplicarAsync(ctx, cdFilial: 1);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
