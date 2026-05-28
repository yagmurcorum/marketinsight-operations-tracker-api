using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketInsight.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchlistItemConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_PriceAlerts_PriceAlertId",
                table: "ActionItems");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_NormalizedSymbol",
                table: "WatchlistItems",
                column: "NormalizedSymbol",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_PriceAlerts_PriceAlertId",
                table: "ActionItems",
                column: "PriceAlertId",
                principalTable: "PriceAlerts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_PriceAlerts_PriceAlertId",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistItems_NormalizedSymbol",
                table: "WatchlistItems");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_PriceAlerts_PriceAlertId",
                table: "ActionItems",
                column: "PriceAlertId",
                principalTable: "PriceAlerts",
                principalColumn: "Id");
        }
    }
}
