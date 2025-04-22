using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AudioToText.Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAudioFileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "AudioFiles",
                newName: "ReceivedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedAt",
                table: "AudioFiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedAt",
                table: "AudioFiles");

            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "AudioFiles",
                newName: "UploadedAt");
        }
    }
}
