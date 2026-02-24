using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContentMarketingAiPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedEditorId",
                table: "Posts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishDate",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PageViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeOnPage = table.Column<int>(type: "int", nullable: false),
                    ScrollDepth = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageViews_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowLogs_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AssignedEditorId",
                table: "Posts",
                column: "AssignedEditorId");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_PostId_CreatedDate",
                table: "PageViews",
                columns: new[] { "PostId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_PostId_CreatedDate",
                table: "WorkflowLogs",
                columns: new[] { "PostId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowLogs_UserId",
                table: "WorkflowLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AssignedEditorId",
                table: "Posts",
                column: "AssignedEditorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AssignedEditorId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "PageViews");

            migrationBuilder.DropTable(
                name: "WorkflowLogs");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AssignedEditorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AssignedEditorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishDate",
                table: "Posts");
        }
    }
}
