using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContasPagar",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CdFilial = table.Column<int>(nullable: false),
                    Descricao = table.Column<string>(maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", precision: 18, scale: 2, nullable: false),
                    Vencimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(nullable: false),
                    CriadoEm = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasPagar", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContasPagar");
        }
    }
}

