using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetEventGalleryProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_event_galleries(
                    input_event_id INTEGER
                )
                RETURNS TABLE (
                    Id INTEGER,
                    Name VARCHAR(255),
                    EventId INTEGER,
                    DefaultGalleryId INTEGER,
                    Filename TEXT,
                    PhotoCount BIGINT
                )
                AS $$
                BEGIN
                    RETURN QUERY
                    SELECT g.""Id"", g.""Name"", g.""EventId"", e.""DefaultGalleryId"", ext.""Filename"", ext.""PhotoCount""
                    FROM public.""Gallery"" g
                    LEFT JOIN (
                        SELECT ""GalleryId"", MIN(""ProcessedFilename"") AS ""Filename"", COUNT(""Id"") AS ""PhotoCount""
                        FROM public.""EventPhotos""
                        WHERE ""ProcessedFilename"" IS NOT NULL
                        GROUP BY ""GalleryId""
                    ) ext ON g.""Id"" = ext.""GalleryId""
                    LEFT JOIN public.""Events"" e ON e.""Id"" = g.""EventId""
                    WHERE g.""EventId"" = input_event_id
                    ORDER BY g.""Id"" ASC;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_event_galleries(INTEGER);");
        }
    }
}
